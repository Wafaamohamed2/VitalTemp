namespace VitalTemp.Domain.Entities;

public class AnalysisResult
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public double TempAvgF { get; set; }
    public string HealthIndicator { get; set; } = string.Empty;
    public double Correlation { get; set; }
    public double PValue { get; set; }
    public double? CompositeRiskScore { get; set; } // Precomputed composite risk score (e.g. from Hamza's model)
    public string Notes { get; set; } = string.Empty;

    // Navigation Property
    public Location? Location { get; set; }
}
