using System.Linq;
using Microsoft.EntityFrameworkCore;
using VitalTemp.Domain.Entities;
using VitalTemp.Infrastructure.Data;

namespace VitalTemp.Infrastructure.Services;

/// <summary>
/// Single source of truth for the city-wide mean surface temperature, used as the
/// thermal-anomaly baseline by BOTH the map (NeighborhoodService) and the AI report
/// (GeminiAiService). There is intentionally no hardcoded constant (e.g. 110.4) anywhere:
/// the baseline is always the live average across all census tracts.
/// </summary>
public static class CityTemperatureBaseline
{
    /// <summary>Resolves a single tract's representative temperature using the same rule
    /// the dashboard map uses: first reading's TempF, else the average of its readings,
    /// else a 104.0°F fallback when no readings exist.</summary>
    public static double ResolveLocationAvgTemp(Location loc)
    {
        if (loc.TemperatureReadings.Count > 0)
        {
            var first = loc.TemperatureReadings.FirstOrDefault();
            return first != null ? first.TempF : loc.TemperatureReadings.Average(r => r.TempF);
        }

        return 104.0;
    }

    /// <summary>Dynamic city-wide mean temperature across every tract.</summary>
    public static async Task<double> ComputeAsync(VitalTempDbContext context, CancellationToken cancellationToken = default)
    {
        // Fetch only the first reading's temperature per tract as a scalar projection
        // instead of loading every TemperatureReading entity into memory. Transferred data
        // stays O(locations), so this scales well beyond 100 tracts.
        var perTractTemps = await context.Locations
            .Select(l => l.TemperatureReadings
                .OrderBy(r => r.Id)
                .Select(r => (double?)r.TempF)
                .FirstOrDefault())
            .ToListAsync(cancellationToken);

        if (perTractTemps.Count == 0)
        {
            return 104.0;
        }

        double sum = 0.0;
        foreach (var temp in perTractTemps)
        {
            sum += temp ?? 104.0;
        }

        return Math.Round(sum / perTractTemps.Count, 1);
    }
}
