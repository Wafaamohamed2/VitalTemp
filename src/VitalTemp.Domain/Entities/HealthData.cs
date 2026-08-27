namespace VitalTemp.Domain.Entities;

public class HealthData
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public string Source { get; set; } = "CDC PLACES";
    public string Indicator { get; set; } = string.Empty; // e.g. "ASTHMA", "DIABETES", "BPHIGH"
    public double Value { get; set; } // Prevalence percentage or rate
    public double? NormalizedValue { get; set; } // Precomputed normalized value from 0.0 to 1.0
    public int Year { get; set; } = 2024;

    // Navigation Property
    public Location? Location { get; set; }
}
