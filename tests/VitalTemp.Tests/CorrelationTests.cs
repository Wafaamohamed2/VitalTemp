using VitalTemp.Infrastructure.Services;
using Xunit;

namespace VitalTemp.Tests;

public class CorrelationTests
{
    [Fact]
    public void CalculateCorrelation_WithStrongPositiveData_ReturnsHighPearsonR()
    {
        // Arrange: 8 Phoenix census tracts temperatures vs Asthma prevalence
        var testPairs = new List<(double TempF, double HealthVal)>
        {
            (100.5, 6.4),
            (102.3, 7.8),
            (105.4, 9.1),
            (106.8, 9.6),
            (108.5, 10.9),
            (109.1, 11.2),
            (113.1, 12.8),
            (114.9, 13.5)
        };

        var calculator = new RiskScoreCalculator(null!, null!);

        // Act
        var result = calculator.CalculateCorrelation(testPairs);

        // Assert: r should be strongly positive (greater than 0.85) and statistically significant
        Assert.True(result.PearsonR > 0.85, $"Expected Pearson r > 0.85, got {result.PearsonR}");
        Assert.True(result.PValue < 0.05, $"Expected p-value < 0.05, got {result.PValue}");
        Assert.Equal(8, result.SampleSize);
        Assert.Contains("positive correlation", result.Interpretation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CalculateCorrelation_WithInsufficientSample_ReturnsSafeDefaults()
    {
        // Arrange: Sample size < 2
        var testPairs = new List<(double TempF, double HealthVal)>
        {
            (105.0, 8.0)
        };

        var calculator = new RiskScoreCalculator(null!, null!);

        // Act
        var result = calculator.CalculateCorrelation(testPairs);

        // Assert
        Assert.Equal(1, result.SampleSize);
        Assert.Contains("Insufficient observations", result.Interpretation);
    }
}
