namespace VitalTemp.Application;

/// <summary>
/// Single source of truth for risk-score classification thresholds.
/// Both the map (NeighborhoodService) and the Gemini AI fallback classify through this
/// class so the same tract can never be "Critical" in the AI report yet "Moderate" on the map.
/// Classification is by risk score only — there is intentionally no temperature override,
/// which previously caused the AI and the map to disagree.
/// </summary>
public static class RiskLevelClassifier
{
    public const double CriticalThreshold = 0.80;
    public const double HighThreshold = 0.65;
    public const double ModerateThreshold = 0.45;

    public static string Classify(double riskScore)
    {
        if (riskScore >= CriticalThreshold)
        {
            return "Critical";
        }

        if (riskScore >= HighThreshold)
        {
            return "High";
        }

        if (riskScore >= ModerateThreshold)
        {
            return "Moderate";
        }

        return "Low";
    }
}
