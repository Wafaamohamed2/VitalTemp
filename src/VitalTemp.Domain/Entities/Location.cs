namespace VitalTemp.Domain.Entities;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = "Phoenix";
    public string State { get; set; } = "AZ";
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Navigation Properties
    public ICollection<TemperatureReading> TemperatureReadings { get; set; } = new List<TemperatureReading>();
    public ICollection<HealthData> HealthDataRecords { get; set; } = new List<HealthData>();
    public ICollection<AnalysisResult> AnalysisResults { get; set; } = new List<AnalysisResult>();
}
