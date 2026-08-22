namespace VitalTemp.Application.Interfaces;

public class CorrelationSummary
{
    public double PearsonR { get; set; }
    public double PValue { get; set; }
    public int SampleSize { get; set; }
    public string Interpretation { get; set; } = string.Empty;
}

public interface IRiskScoreCalculator
{
    CorrelationSummary CalculateCorrelation(IEnumerable<(double TempF, double HealthVal)> pairs);
    Task<int> RecalculateAllAnalysisResultsAsync(string indicator = "ALL", CancellationToken cancellationToken = default);
}
