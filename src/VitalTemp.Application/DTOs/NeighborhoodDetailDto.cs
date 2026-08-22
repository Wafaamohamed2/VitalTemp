namespace VitalTemp.Application.DTOs;

public class TemperaturePointDto
{
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public double TempF { get; set; }
    public double TempC { get; set; }
}

public class HealthMetricDto
{
    public string Indicator { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Source { get; set; } = "CDC PLACES";
    public int Year { get; set; }
}

public class NeighborhoodDetailDto
{
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = "Phoenix";
    public string State { get; set; } = "AZ";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double TempAvgF { get; set; }
    public double RiskScore { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public double Correlation { get; set; }
    public double PValue { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<TemperaturePointDto> TemperatureHistory { get; set; } = new();
    public List<HealthMetricDto> HealthMetrics { get; set; } = new();
}
