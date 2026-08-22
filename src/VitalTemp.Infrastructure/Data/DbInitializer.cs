using Microsoft.EntityFrameworkCore;
using VitalTemp.Domain.Entities;

namespace VitalTemp.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(VitalTempDbContext context)
    {
        // Ensure Database is created / migrated
        await context.Database.MigrateAsync();

        if (await context.Locations.AnyAsync())
        {
            return; // Data already seeded
        }

        var locations = new List<Location>
        {
            new() { Id = 1, Name = "Downtown Phoenix (Tract 1101)", City = "Phoenix", State = "AZ", Latitude = 33.4484, Longitude = -112.0740 },
            new() { Id = 2, Name = "Maryvale West (Tract 1096.03)", City = "Phoenix", State = "AZ", Latitude = 33.4980, Longitude = -112.1850 },
            new() { Id = 3, Name = "South Mountain (Tract 1145)", City = "Phoenix", State = "AZ", Latitude = 33.3750, Longitude = -112.0500 },
            new() { Id = 4, Name = "Alhambra (Tract 1060)", City = "Phoenix", State = "AZ", Latitude = 33.5100, Longitude = -112.1150 },
            new() { Id = 5, Name = "Camelback East (Tract 1052)", City = "Phoenix", State = "AZ", Latitude = 33.5090, Longitude = -112.0050 },
            new() { Id = 6, Name = "Encanto (Tract 1073)", City = "Phoenix", State = "AZ", Latitude = 33.4750, Longitude = -112.0800 },
            new() { Id = 7, Name = "North Mountain (Tract 1035)", City = "Phoenix", State = "AZ", Latitude = 33.5850, Longitude = -112.0850 },
            new() { Id = 8, Name = "Desert View (Tract 1042)", City = "Phoenix", State = "AZ", Latitude = 33.6800, Longitude = -112.0200 }
        };

        await context.Locations.AddRangeAsync(locations);
        await context.SaveChangesAsync();

        var readings = new List<TemperatureReading>
        {
            new() { LocationId = 1, Date = "2026-08-01", Time = "14:00:00", TempF = 112.4, TempC = 44.7, Granularity = 60 },
            new() { LocationId = 1, Date = "2026-08-01", Time = "15:00:00", TempF = 113.8, TempC = 45.4, Granularity = 60 },
            new() { LocationId = 2, Date = "2026-08-01", Time = "14:00:00", TempF = 114.2, TempC = 45.7, Granularity = 60 },
            new() { LocationId = 2, Date = "2026-08-01", Time = "15:00:00", TempF = 115.6, TempC = 46.4, Granularity = 60 },
            new() { LocationId = 3, Date = "2026-08-01", Time = "14:00:00", TempF = 109.1, TempC = 42.8, Granularity = 60 },
            new() { LocationId = 4, Date = "2026-08-01", Time = "14:00:00", TempF = 108.5, TempC = 42.5, Granularity = 60 },
            new() { LocationId = 5, Date = "2026-08-01", Time = "14:00:00", TempF = 102.3, TempC = 39.1, Granularity = 60 },
            new() { LocationId = 6, Date = "2026-08-01", Time = "14:00:00", TempF = 106.8, TempC = 41.6, Granularity = 60 },
            new() { LocationId = 7, Date = "2026-08-01", Time = "14:00:00", TempF = 105.4, TempC = 40.8, Granularity = 60 },
            new() { LocationId = 8, Date = "2026-08-01", Time = "14:00:00", TempF = 100.5, TempC = 38.1, Granularity = 60 }
        };

        var healthData = new List<HealthData>
        {
            new() { LocationId = 1, Source = "CDC PLACES", Indicator = "ASTHMA", Value = 12.8, Year = 2024 },
            new() { LocationId = 1, Source = "CDC PLACES", Indicator = "BPHIGH", Value = 34.2, Year = 2024 },
            new() { LocationId = 2, Source = "CDC PLACES", Indicator = "ASTHMA", Value = 13.5, Year = 2024 },
            new() { LocationId = 2, Source = "CDC PLACES", Indicator = "BPHIGH", Value = 37.1, Year = 2024 },
            new() { LocationId = 3, Source = "CDC PLACES", Indicator = "ASTHMA", Value = 11.2, Year = 2024 },
            new() { LocationId = 4, Source = "CDC PLACES", Indicator = "ASTHMA", Value = 10.9, Year = 2024 },
            new() { LocationId = 5, Source = "CDC PLACES", Indicator = "ASTHMA", Value = 7.8, Year = 2024 },
            new() { LocationId = 6, Source = "CDC PLACES", Indicator = "ASTHMA", Value = 9.6, Year = 2024 },
            new() { LocationId = 7, Source = "CDC PLACES", Indicator = "ASTHMA", Value = 9.1, Year = 2024 },
            new() { LocationId = 8, Source = "CDC PLACES", Indicator = "ASTHMA", Value = 6.4, Year = 2024 }
        };

        var analysisResults = new List<AnalysisResult>
        {
            new() { LocationId = 1, TempAvgF = 113.1, HealthIndicator = "ASTHMA", Correlation = 0.84, PValue = 0.002, Notes = "Severe urban heat island effect with dark pavement and sparse tree canopy. High emergency respiratory incidents." },
            new() { LocationId = 2, TempAvgF = 114.9, HealthIndicator = "ASTHMA", Correlation = 0.89, PValue = 0.001, Notes = "Critical heat vulnerability cluster. High density, low shaded infrastructure, elevated chronic respiratory vulnerability." },
            new() { LocationId = 3, TempAvgF = 109.1, HealthIndicator = "ASTHMA", Correlation = 0.78, PValue = 0.006, Notes = "Significant thermal exposure near mountain base with medium-high asthma rates." },
            new() { LocationId = 4, TempAvgF = 108.5, HealthIndicator = "ASTHMA", Correlation = 0.75, PValue = 0.010, Notes = "Moderate to high heat load along commercial corridors; respiratory distress peaks during afternoon peaks." },
            new() { LocationId = 5, TempAvgF = 102.3, HealthIndicator = "ASTHMA", Correlation = 0.35, PValue = 0.080, Notes = "Substantial urban tree canopy mitigating surface heat. Low vulnerability index." },
            new() { LocationId = 6, TempAvgF = 106.8, HealthIndicator = "ASTHMA", Correlation = 0.62, PValue = 0.025, Notes = "Moderate urban heat profile with park corridors providing partial thermal buffer." },
            new() { LocationId = 7, TempAvgF = 105.4, HealthIndicator = "ASTHMA", Correlation = 0.58, PValue = 0.035, Notes = "Moderate residential heat retention with average health vulnerability." },
            new() { LocationId = 8, TempAvgF = 100.5, HealthIndicator = "ASTHMA", Correlation = 0.28, PValue = 0.120, Notes = "Coolest sub-area with open desert air flows and lowest reported health burden." }
        };

        await context.TemperatureReadings.AddRangeAsync(readings);
        await context.HealthDataRecords.AddRangeAsync(healthData);
        await context.AnalysisResults.AddRangeAsync(analysisResults);
        await context.SaveChangesAsync();
    }
}
