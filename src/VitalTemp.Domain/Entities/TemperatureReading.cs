namespace VitalTemp.Domain.Entities;

public class TemperatureReading
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD
    public string Time { get; set; } = string.Empty; // HH:mm:ss
    public double TempF { get; set; }
    public double TempC { get; set; }
    public double? TempNormalized { get; set; } // Precomputed normalized temperature from 0.0 to 1.0
    public int Granularity { get; set; } = 60; // in minutes

    // Navigation Property
    public Location? Location { get; set; }
}
