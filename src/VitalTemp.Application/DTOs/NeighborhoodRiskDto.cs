namespace VitalTemp.Application.DTOs;

public class NeighborhoodRiskDto
{
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = "Phoenix";
    public string State { get; set; } = "AZ";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double TempAvgF { get; set; }
    public double TempAvgC => Math.Round((TempAvgF - 32) * 5 / 9, 1);
    public double ThermalAnomalyF { get; set; } // Difference from city baseline
    public string HealthIndicator { get; set; } = "ASTHMA";
    public double HealthValue { get; set; }
    public double RiskScore { get; set; } // 0.0 to 1.0
    public string RiskLevel { get; set; } = "Low"; // Low, Moderate, High, Critical
    public string HotspotCategory { get; set; } = "High-High Hotspot"; // High-High, Moderate, Low-Low
    public double CitywideCorrelation { get; set; } // r across tracts
    public double CitywidePValue { get; set; } // p-value across tracts
    public string Notes { get; set; } = string.Empty;
}

public class DashboardSummaryDto
{
    public int TotalNeighborhoods { get; set; }
    public double AverageTemperatureF { get; set; }
    public int HighRiskCount { get; set; }
    public double CitywideCorrelation { get; set; }
    public double CitywidePValue { get; set; }
    public string TopVulnerableArea { get; set; } = string.Empty;
    public string PrimaryIndicator { get; set; } = "ALL";
}
