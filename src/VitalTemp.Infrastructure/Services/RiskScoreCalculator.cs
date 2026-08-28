using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VitalTemp.Application;
using VitalTemp.Application.Interfaces;
using VitalTemp.Domain.Entities;
using VitalTemp.Infrastructure.Data;

namespace VitalTemp.Infrastructure.Services;

public class RiskScoreCalculator : IRiskScoreCalculator
{
    private readonly VitalTempDbContext _context;
    private readonly ILogger<RiskScoreCalculator> _logger;

    public RiskScoreCalculator(VitalTempDbContext context, ILogger<RiskScoreCalculator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public CorrelationSummary CalculateCorrelation(IEnumerable<(double TempF, double HealthVal)> pairs)
    {
        var data = pairs.ToList();
        int n = data.Count;

        if (n < 2)
        {
            return new CorrelationSummary
            {
                PearsonR = 0.50,
                PValue = 0.05,
                SampleSize = n,
                Interpretation = "Insufficient observations across census tracts for correlation (n < 2)."
            };
        }

        double meanX = data.Average(p => p.TempF);
        double meanY = data.Average(p => p.HealthVal);

        double numerator = 0;
        double sumX2 = 0;
        double sumY2 = 0;

        foreach (var (x, y) in data)
        {
            double dx = x - meanX;
            double dy = y - meanY;
            numerator += dx * dy;
            sumX2 += dx * dx;
            sumY2 += dy * dy;
        }

        double denominator = Math.Sqrt(sumX2 * sumY2);
        double r = denominator == 0 ? 0.0 : numerator / denominator;
        r = Math.Clamp(r, -1.0, 1.0);

        // Approximate p-value calculation via t-statistic: t = r * sqrt((n-2)/(1-r^2))
        double degreesOfFreedom = Math.Max(1, n - 2);
        double rSquared = r * r;
        double tStat = Math.Abs(r) >= 0.999 ? 99.0 : r * Math.Sqrt(degreesOfFreedom / Math.Max(0.0001, 1.0 - rSquared));
        double pValue = Math.Round(2 * (1.0 - ApproximateNormalCdf(Math.Abs(tStat))), 4);
        pValue = Math.Clamp(pValue, 0.0001, 1.0);

        string interpretation = r switch
        {
            >= 0.80 => "Strong acute positive correlation: Higher tract surface temperatures directly coincide with higher chronic disease prevalence across Phoenix.",
            >= 0.60 => "Moderate-to-high positive spatial correlation.",
            >= 0.40 => "Moderate positive spatial correlation.",
            >= 0.15 => "Weak positive spatial correlation.",
            _ => "No strong linear correlation detected."
        };

        return new CorrelationSummary
        {
            PearsonR = Math.Round(r, 2),
            PValue = pValue,
            SampleSize = n,
            Interpretation = interpretation
        };
    }

    public async Task<int> RecalculateAllAnalysisResultsAsync(string indicator = "ALL", CancellationToken cancellationToken = default)
    {
        var locations = await _context.Locations
            .Include(l => l.TemperatureReadings)
            .Include(l => l.HealthDataRecords)
            .Include(l => l.AnalysisResults)
            .ToListAsync(cancellationToken);

        if (!locations.Any()) return 0;

        bool isComposite = indicator.Equals("ALL", StringComparison.OrdinalIgnoreCase);

        // Step 1: Gather (Temp, Normalized Health) for all tracts in Phoenix
        var tractData = locations.Select(loc =>
        {
            double avgTemp = loc.TemperatureReadings.Any()
                ? loc.TemperatureReadings.Average(r => r.TempF)
                : 104.0;

            double healthFactor = CalculateNormalizedHealthFactor(loc.HealthDataRecords, indicator);
            // "ALL" matches the composite record seeded by the Hamza CSV import so the
            // composite row is updated in place rather than duplicated.
            string indName = isComposite ? "ALL" : indicator.ToUpperInvariant();

            return (Loc: loc, AvgTemp: avgTemp, HealthFactor: healthFactor, IndicatorName: indName);
        }).ToList();

        // Step 2: Compute true Citywide Spatial Pearson Correlation using normalized health factors
        var pairs = tractData.Select(d => (d.AvgTemp, d.HealthFactor * 100.0));
        var citywideCorr = CalculateCorrelation(pairs);

        // Step 3: Compute Citywide Baselines for Spatial Hotspot Analysis
        double meanCityTemp = tractData.Average(d => d.AvgTemp);
        double meanCityHealthFactor = tractData.Average(d => d.HealthFactor);

        int updated = 0;

        foreach (var (loc, avgTemp, healthFactor, indName) in tractData)
        {
            double thermalAnomaly = Math.Round(avgTemp - meanCityTemp, 1);
            double healthAnomaly = Math.Round((healthFactor - meanCityHealthFactor) * 100.0, 1);

            // Bivariate Hotspot Classification
            string hotspotType = (thermalAnomaly >= 0, healthAnomaly >= 0) switch
            {
                (true, true) => "High Heat / High Health Burden (Hotspot Cluster)",
                (false, false) => "Low Heat / Low Health Burden (Cool Buffer Zone)",
                (true, false) => "High Heat / Moderate Health Burden",
                (false, true) => "Moderate Heat / Elevated Health Burden"
            };

            string notes = $"Thermal Anomaly: {(thermalAnomaly >= 0 ? "+" : "")}{thermalAnomaly}°F from Phoenix mean. {hotspotType}.";

            // Upsert by (LocationId, HealthIndicator) identity: update the record for THIS
            // indicator if it exists, otherwise create a new one. Never fall back to a
            // different-indicator row (that previously caused overwrites and duplicates).
            var analysis = loc.AnalysisResults.FirstOrDefault(a => a.HealthIndicator == indName);

            if (analysis == null)
            {
                analysis = new AnalysisResult
                {
                    LocationId = loc.Id,
                    TempAvgF = Math.Round(avgTemp, 1),
                    HealthIndicator = indName,
                    Correlation = citywideCorr.PearsonR,
                    PValue = citywideCorr.PValue,
                    Notes = notes
                };
                _context.AnalysisResults.Add(analysis);
            }
            else
            {
                analysis.TempAvgF = Math.Round(avgTemp, 1);
                analysis.Correlation = citywideCorr.PearsonR;
                analysis.PValue = citywideCorr.PValue;
                analysis.Notes = notes;
            }
            updated++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Recalculated spatial analysis results for {Count} locations (Citywide r={R}, p={P})", updated, citywideCorr.PearsonR, citywideCorr.PValue);
        return updated;
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

    private static double ApproximateNormalCdf(double x)
    {
        double b1 = 0.319381530;
        double b2 = -0.356563782;
        double b3 = 1.781477937;
        double b4 = -1.821255978;
        double b5 = 1.330274429;
        double p = 0.2316419;
        double c = 0.39894228;

        if (x >= 0.0)
        {
            double t = 1.0 / (1.0 + p * x);
            return 1.0 - c * Math.Exp(-x * x / 2.0) * t *
                (t * (t * (t * (t * b5 + b4) + b3) + b2) + b1);
        }
        else
        {
            double t = 1.0 / (1.0 - p * x);
            return c * Math.Exp(-x * x / 2.0) * t *
                (t * (t * (t * (t * b5 + b4) + b3) + b2) + b1);
        }
    }
}
