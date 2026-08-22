namespace VitalTemp.Domain.Entities;

public class HealthData
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public string Source { get; set; } = "CDC PLACES";
    public string Indicator { get; set; } = string.Empty; // e.g. "ASTHMA", "DIABETES", "BPHIGH"
    public double Value { get; set; } // Prevalence percentage or rate
    public int Year { get; set; } = 2024;

    // Navigation Property
    public Location? Location { get; set; }
}
