using Microsoft.EntityFrameworkCore;
using VitalTemp.Application;
using VitalTemp.Application.DTOs;
using VitalTemp.Application.Interfaces;
using VitalTemp.Domain.Entities;
using VitalTemp.Infrastructure.Data;

namespace VitalTemp.Infrastructure.Services;

public class NeighborhoodService : INeighborhoodService
{
    private readonly VitalTempDbContext _context;
    private readonly IRiskScoreCalculator _riskScoreCalculator;

    public NeighborhoodService(VitalTempDbContext context, IRiskScoreCalculator riskScoreCalculator)
    {
        _context = context;
        _riskScoreCalculator = riskScoreCalculator;
    }

    public async Task<IEnumerable<NeighborhoodRiskDto>> GetRiskScoresAsync(string indicator = "ALL")
    {
        var locations = await _context.Locations
            .Include(l => l.TemperatureReadings)
            .Include(l => l.HealthDataRecords)
            .Include(l => l.AnalysisResults)
            .AsNoTracking()
            .ToListAsync();

        if (!locations.Any()) return Enumerable.Empty<NeighborhoodRiskDto>();

        bool isComposite = indicator.Equals("ALL", StringComparison.OrdinalIgnoreCase);

        // Step 1: Extract and mathematically compute risk factors per tract
        var tractItems = locations.Select(loc =>
        {
            var tempReading = loc.TemperatureReadings.FirstOrDefault();
            double avgTemp = tempReading != null ? tempReading.TempF : (loc.TemperatureReadings.Any() ? loc.TemperatureReadings.Average(r => r.TempF) : 104.0);
            double tempNorm = tempReading?.TempNormalized ?? Math.Clamp((avgTemp - 98.0) / (116.0 - 98.0), 0.0, 1.0);

            double healthFactor;
            double displayHealthValue;
            string displayIndicatorName;
            double riskScore;

            if (isComposite)
            {
                displayIndicatorName = "Composite Index";

                // Average of normalized values across all indicators that have NormalizedValue
                var normRecords = loc.HealthDataRecords.Where(h => h.NormalizedValue.HasValue).ToList();
                healthFactor = normRecords.Any() ? normRecords.Average(h => h.NormalizedValue!.Value) : CalculateNormalizedHealthFactor(loc.HealthDataRecords, "ALL");
                displayHealthValue = Math.Round(healthFactor * 100.0, 1); // e.g. 74.5%

                // Use Hamza's precomputed composite risk score if available
                var analysis = loc.AnalysisResults.FirstOrDefault(a => a.HealthIndicator == "ALL");
                if (analysis?.CompositeRiskScore.HasValue == true)
                {
                    riskScore = Math.Round(analysis.CompositeRiskScore.Value, 2);
                }
                else
                {
                    riskScore = Math.Round((tempNorm * 0.60) + (healthFactor * 0.40), 2);
                }
            }
            else
            {
                displayIndicatorName = indicator.ToUpperInvariant();
                var match = loc.HealthDataRecords.FirstOrDefault(h => h.Indicator.Equals(indicator, StringComparison.OrdinalIgnoreCase));

                if (match?.NormalizedValue.HasValue == true)
                {
                    healthFactor = match.NormalizedValue.Value;
                    riskScore = Math.Round((tempNorm * 0.60) + (healthFactor * 0.40), 2);
                    displayHealthValue = Math.Round(match.Value, 1);
                }
                else
                {
                    healthFactor = CalculateNormalizedHealthFactor(loc.HealthDataRecords, indicator);
                    riskScore = Math.Round((tempNorm * 0.60) + (healthFactor * 0.40), 2);
                    displayHealthValue = match != null ? Math.Round(match.Value, 1) : Math.Round(healthFactor * 100.0, 1);
                }
            }

            return new
            {
                Location = loc,
                AvgTemp = avgTemp,
                HealthFactor = healthFactor,
                DisplayHealthValue = displayHealthValue,
                DisplayIndicatorName = displayIndicatorName,
                RiskScore = riskScore
            };
        }).ToList();

        double meanCityTemp = tractItems.Average(t => t.AvgTemp);
        double meanCityHealthFactor = tractItems.Average(t => t.HealthFactor);

        // Step 2: Compute citywide spatial correlation using normalized health factors
        var citywideCorr = _riskScoreCalculator.CalculateCorrelation(tractItems.Select(t => (t.AvgTemp, t.HealthFactor * 100.0)));

        return tractItems.Select(item =>
        {
            var loc = item.Location;
            double avgTemp = item.AvgTemp;
            double healthFactor = item.HealthFactor;
            double riskScore = item.RiskScore;

            double thermalAnomaly = Math.Round(avgTemp - meanCityTemp, 1);
            double healthAnomaly = Math.Round((healthFactor - meanCityHealthFactor) * 100.0, 1);

            string hotspotCategory = (thermalAnomaly >= 0, healthAnomaly >= 0) switch
            {
                (true, true) => "High Heat / High Health Burden (Hotspot Cluster)",
                (false, false) => "Low Heat / Low Health Burden (Cool Buffer Zone)",
                (true, false) => "High Heat / Moderate Health Burden",
                (false, true) => "Moderate Heat / Elevated Health Burden"
            };

            string riskLevel = riskScore switch
            {
                >= 0.80 => "Critical",
                >= 0.65 => "High",
                >= 0.45 => "Moderate",
                _ => "Low"
            };

            string notes = $"Thermal Anomaly: {(thermalAnomaly >= 0 ? "+" : "")}{thermalAnomaly}°F vs city baseline. {hotspotCategory}.";

            return new NeighborhoodRiskDto
            {
                LocationId = loc.Id,
                Name = loc.Name,
                City = loc.City,
                State = loc.State,
                Latitude = loc.Latitude,
                Longitude = loc.Longitude,
                TempAvgF = Math.Round(avgTemp, 1),
                ThermalAnomalyF = thermalAnomaly,
                HealthIndicator = item.DisplayIndicatorName,
                HealthValue = item.DisplayHealthValue,
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                HotspotCategory = hotspotCategory,
                CitywideCorrelation = citywideCorr.PearsonR,
                CitywidePValue = citywideCorr.PValue,
                Notes = notes
            };
        }).OrderByDescending(x => x.RiskScore).ToList();
    }

    public async Task<NeighborhoodDetailDto?> GetNeighborhoodDetailsAsync(int locationId, string indicator = "ALL")
    {
        var loc = await _context.Locations
            .Include(l => l.TemperatureReadings)
            .Include(l => l.HealthDataRecords)
            .Include(l => l.AnalysisResults)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == locationId);

        if (loc == null) return null;

        var riskDto = (await GetRiskScoresAsync(indicator)).FirstOrDefault(r => r.LocationId == locationId);

        return new NeighborhoodDetailDto
        {
            LocationId = loc.Id,
            Name = loc.Name,
            City = loc.City,
            State = loc.State,
            Latitude = loc.Latitude,
            Longitude = loc.Longitude,
            TempAvgF = riskDto?.TempAvgF ?? 104.0,
            RiskScore = riskDto?.RiskScore ?? 0.5,
            RiskLevel = riskDto?.RiskLevel ?? "Moderate",
            Correlation = riskDto?.CitywideCorrelation ?? 0.84,
            PValue = riskDto?.CitywidePValue ?? 0.002,
            Notes = riskDto?.Notes ?? string.Empty,
            TemperatureHistory = loc.TemperatureReadings.Select(tr => new TemperaturePointDto
            {
                Date = tr.Date,
                Time = tr.Time,
                TempF = tr.TempF,
                TempC = tr.TempC
            }).ToList(),
            HealthMetrics = loc.HealthDataRecords.Select(hd => new HealthMetricDto
            {
                Indicator = hd.Indicator,
                Value = hd.Value,
                Source = hd.Source,
                Year = hd.Year
            }).ToList()
        };
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(string indicator = "ALL")
    {
        var riskScores = (await GetRiskScoresAsync(indicator)).ToList();
        if (!riskScores.Any())
        {
            return new DashboardSummaryDto();
        }

        return new DashboardSummaryDto
        {
            TotalNeighborhoods = riskScores.Count,
            AverageTemperatureF = Math.Round(riskScores.Average(r => r.TempAvgF), 1),
            HighRiskCount = riskScores.Count(r => r.RiskLevel is "High" or "Critical"),
            CitywideCorrelation = riskScores.First().CitywideCorrelation,
            CitywidePValue = riskScores.First().CitywidePValue,
            TopVulnerableArea = riskScores.First().Name,
            PrimaryIndicator = indicator
        };
    }

    private static double CalculateNormalizedHealthFactor(IEnumerable<HealthData> records, string indicator)
    {
        var list = records.ToList();
        if (!list.Any()) return 0.5;

        if (!indicator.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var match = list.FirstOrDefault(h => h.Indicator.Equals(indicator, StringComparison.OrdinalIgnoreCase));
            if (match == null) return 0.5;

                double maxScale = HealthIndicatorScales.GetScale(indicator);

                return Math.Clamp(match.Value / maxScale, 0.0, 1.0);
        }
        else
        {
            // True Normalized Multi-Disease Index: Normalize each disease metric against its own clinical scale
            double sumNormalized = 0.0;
            int count = 0;

            foreach (var rec in list)
            {
                double scale = HealthIndicatorScales.GetScale(rec.Indicator);

                sumNormalized += Math.Clamp(rec.Value / scale, 0.0, 1.0);
                count++;
            }

            return count > 0 ? (sumNormalized / count) : 0.5;
        }
    }
}
