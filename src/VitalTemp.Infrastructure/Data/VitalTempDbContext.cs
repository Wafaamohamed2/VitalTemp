using Microsoft.EntityFrameworkCore;
using VitalTemp.Domain.Entities;

namespace VitalTemp.Infrastructure.Data;

public class VitalTempDbContext : DbContext
{
    public VitalTempDbContext(DbContextOptions<VitalTempDbContext> options) : base(options)
    {
    }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<TemperatureReading> TemperatureReadings => Set<TemperatureReading>();
    public DbSet<HealthData> HealthDataRecords => Set<HealthData>();
    public DbSet<AnalysisResult> AnalysisResults => Set<AnalysisResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map Table Names to match the agreed schema
        modelBuilder.Entity<Location>().ToTable("locations");
        modelBuilder.Entity<TemperatureReading>().ToTable("temperature_readings");
        modelBuilder.Entity<HealthData>().ToTable("health_data");
        modelBuilder.Entity<AnalysisResult>().ToTable("analysis_results");

        // Locations Configuration
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
        });

        // Temperature Readings Configuration
        modelBuilder.Entity<TemperatureReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.TempF).HasColumnName("temp_f");
            entity.Property(e => e.TempC).HasColumnName("temp_c");
            entity.Property(e => e.Granularity).HasColumnName("granularity");

            entity.HasOne(e => e.Location)
                  .WithMany(l => l.TemperatureReadings)
                  .HasForeignKey(e => e.LocationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.LocationId);
        });

        // Health Data Configuration
        modelBuilder.Entity<HealthData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Source).HasColumnName("source");
            entity.Property(e => e.Indicator).HasColumnName("indicator");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(e => e.Location)
                  .WithMany(l => l.HealthDataRecords)
                  .HasForeignKey(e => e.LocationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.LocationId);
        });

        // Analysis Results Configuration
        modelBuilder.Entity<AnalysisResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.TempAvgF).HasColumnName("temp_avg_f");
            entity.Property(e => e.HealthIndicator).HasColumnName("health_indicator");
            entity.Property(e => e.Correlation).HasColumnName("correlation");
            entity.Property(e => e.PValue).HasColumnName("p_value");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(e => e.Location)
                  .WithMany(l => l.AnalysisResults)
                  .HasForeignKey(e => e.LocationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.LocationId);
        });
    }
}
