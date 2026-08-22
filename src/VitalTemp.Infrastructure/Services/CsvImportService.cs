using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VitalTemp.Application.Interfaces;
using VitalTemp.Domain.Entities;
using VitalTemp.Infrastructure.Data;

namespace VitalTemp.Infrastructure.Services;

public class CsvImportService : ICsvImportService
{
    private readonly VitalTempDbContext _context;
    private readonly ILogger<CsvImportService> _logger;

    public CsvImportService(VitalTempDbContext context, ILogger<CsvImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CsvImportResult> ImportLocationsCsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var result = new CsvImportResult();
        using var reader = new StreamReader(csvStream);
        string? headerLine = await reader.ReadLineAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Success = false;
            result.Message = "Empty CSV file provided.";
            return result;
        }

        var headers = headerLine.Split(',').Select(h => h.Trim().ToLowerInvariant().Replace("\"", "")).ToArray();
        int nameIdx = Array.FindIndex(headers, h => h.Contains("name") || h.Contains("tract") || h.Contains("neighborhood"));
        int cityIdx = Array.FindIndex(headers, h => h.Contains("city"));
        int stateIdx = Array.FindIndex(headers, h => h.Contains("state"));
        int latIdx = Array.FindIndex(headers, h => h.Contains("lat") || h.Contains("latitude") || h.Contains("y"));
        int lngIdx = Array.FindIndex(headers, h => h.Contains("lon") || h.Contains("lng") || h.Contains("longitude") || h.Contains("x"));

        if (nameIdx == -1 || latIdx == -1 || lngIdx == -1)
        {
            result.Success = false;
            result.Message = "Missing required columns in CSV (must contain Name/Tract, Latitude, and Longitude).";
            return result;
        }

        int count = 0;
        string? line;
        int lineNum = 1;

        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            lineNum++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = ParseCsvRow(line);
            if (cols.Length <= Math.Max(nameIdx, Math.Max(latIdx, lngIdx)))
            {
                result.Errors.Add($"Line {lineNum}: insufficient column count.");
                continue;
            }

            string name = cols[nameIdx].Trim();
            string city = cityIdx != -1 && cols.Length > cityIdx && !string.IsNullOrWhiteSpace(cols[cityIdx]) ? cols[cityIdx].Trim() : "Phoenix";
            string state = stateIdx != -1 && cols.Length > stateIdx && !string.IsNullOrWhiteSpace(cols[stateIdx]) ? cols[stateIdx].Trim() : "AZ";

            if (!double.TryParse(cols[latIdx], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) ||
                !double.TryParse(cols[lngIdx], NumberStyles.Any, CultureInfo.InvariantCulture, out double lng))
            {
                result.Errors.Add($"Line {lineNum}: Invalid latitude/longitude format.");
                continue;
            }

            var existing = await _context.Locations.FirstOrDefaultAsync(l => l.Name == name, cancellationToken);
            if (existing != null)
            {
                existing.Latitude = lat;
                existing.Longitude = lng;
                existing.City = city;
                existing.State = state;
            }
            else
            {
                _context.Locations.Add(new Location
                {
                    Name = name,
                    City = city,
                    State = state,
                    Latitude = lat,
                    Longitude = lng
                });
            }

            count++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        result.Success = true;
        result.LocationsImported = count;
        result.Message = $"Successfully imported/updated {count} locations in Phoenix.";
        return result;
    }

    public async Task<CsvImportResult> ImportHealthDataCsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var result = new CsvImportResult();
        using var reader = new StreamReader(csvStream);
        string? headerLine = await reader.ReadLineAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Success = false;
            result.Message = "Empty Health CSV file.";
            return result;
        }

        var headers = headerLine.Split(',').Select(h => h.Trim().ToLowerInvariant().Replace("\"", "")).ToArray();
        int tractIdx = Array.FindIndex(headers, h => h.Contains("location") || h.Contains("tract") || h.Contains("name"));
        int indIdx = Array.FindIndex(headers, h => h.Contains("indicator") || h.Contains("measure") || h.Contains("short_question_text"));
        int valIdx = Array.FindIndex(headers, h => h.Contains("value") || h.Contains("data_value") || h.Contains("rate") || h.Contains("prevalence"));
        int yearIdx = Array.FindIndex(headers, h => h.Contains("year"));

        if (tractIdx == -1 || valIdx == -1)
        {
            result.Success = false;
            result.Message = "Health CSV must contain Tract/Location and Value columns.";
            return result;
        }

        var locations = await _context.Locations.ToListAsync(cancellationToken);
        int count = 0;
        string? line;
        int lineNum = 1;

        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            lineNum++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = ParseCsvRow(line);
            string tractName = cols[tractIdx].Trim();
            string indicator = indIdx != -1 && cols.Length > indIdx ? cols[indIdx].Trim() : "ASTHMA";
            int year = yearIdx != -1 && cols.Length > yearIdx && int.TryParse(cols[yearIdx], out int y) ? y : 2024;

            if (!double.TryParse(cols[valIdx], NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
            {
                result.Errors.Add($"Line {lineNum}: Invalid numeric value '{cols[valIdx]}'.");
                continue;
            }

            var loc = locations.FirstOrDefault(l => l.Name.Contains(tractName, StringComparison.OrdinalIgnoreCase) || tractName.Contains(l.Name, StringComparison.OrdinalIgnoreCase));
            if (loc != null)
            {
                _context.HealthDataRecords.Add(new HealthData
                {
                    LocationId = loc.Id,
                    Source = "CDC PLACES",
                    Indicator = indicator,
                    Value = val,
                    Year = year
                });
                count++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        result.Success = true;
        result.HealthRecordsImported = count;
        result.Message = $"Successfully imported {count} CDC health records.";
        return result;
    }

    private static string[] ParseCsvRow(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var currentStr = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentStr.ToString().Trim('\"', ' '));
                currentStr.Clear();
            }
            else
            {
                currentStr.Append(c);
            }
        }
        result.Add(currentStr.ToString().Trim('\"', ' '));
        return result.ToArray();
    }
}
