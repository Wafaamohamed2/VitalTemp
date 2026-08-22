namespace VitalTemp.Application.DTOs;

public class GeminiRecommendationDto
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public List<string> ImmediateActions { get; set; } = new();
    public List<string> InfrastructureMitigations { get; set; } = new();
    public List<string> PublicHealthDirectives { get; set; } = new();
    public string EstimatedHeatReduction { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string ModelUsed { get; set; } = "Google Gemini 1.5 Flash";
}
