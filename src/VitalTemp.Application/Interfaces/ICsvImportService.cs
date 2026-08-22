namespace VitalTemp.Application.Interfaces;

public class CsvImportResult
{
    public bool Success { get; set; }
    public int LocationsImported { get; set; }
    public int HealthRecordsImported { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

public interface ICsvImportService
{
    Task<CsvImportResult> ImportLocationsCsvAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task<CsvImportResult> ImportHealthDataCsvAsync(Stream csvStream, CancellationToken cancellationToken = default);
}
