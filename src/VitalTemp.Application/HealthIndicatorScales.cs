namespace VitalTemp.Application;

/// <summary>
/// Single source of truth for the clinical normalization maxima used to compute the
/// heat-health risk score. Every backend risk calculation (RiskScoreCalculator,
/// NeighborhoodService, GeminiAiService) must resolve indicator scales through this class
/// so that the same tract can never be classified differently across components.
///
/// The frontend keeps a mirrored copy of these exact values for its offline fallback only;
/// in normal operation the backend owns the risk score (see NeighborhoodRiskDto / DashboardSummaryDto).
/// </summary>
public static class HealthIndicatorScales
{
    // Canonical CDC PLACES prevalence maxima. Values reflect realistic upper bounds
    // observed across Phoenix / Maricopa County tracts (e.g. Obesity ~40%, BPHIGH ~42%).
    private static readonly Dictionary<string, double> Scales = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ASTHMA", 15.0 },
        { "BPHIGH", 42.0 },
        { "DIABETES", 20.0 },
        { "CHD", 10.0 },
        { "OBESITY", 40.0 },
        { "MENTALDISTRESS", 20.0 },
        { "NOACTIVITY", 35.0 },
        { "DEPRESSION", 20.0 },
        { "FAIRHEALTH", 40.0 },
        { "STROKE", 10.0 }
    };

    /// <summary>Returns the normalization maximum for an indicator (defaults to 15.0 for unknown indicators).</summary>
    public static double GetScale(string indicator)
    {
        return Scales.TryGetValue(indicator, out var scale) ? scale : 15.0;
    }

    /// <summary>Normalizes a raw prevalence value to the 0.0 - 1.0 range for its indicator.</summary>
    public static double Normalize(double value, string indicator)
    {
        double scale = GetScale(indicator);
        return scale <= 0.0 ? 0.0 : Math.Clamp(value / scale, 0.0, 1.0);
    }
}
