using Microsoft.EntityFrameworkCore;
using VitalTemp.Application.Interfaces;
using VitalTemp.Infrastructure.Data;
using VitalTemp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers
builder.Services.AddControllers();

// 2. Configure CORS for Frontend React Vite (localhost:5173, localhost:3000, etc.)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3. Configure EF Core with SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=vitaltemp.db";
builder.Services.AddDbContext<VitalTempDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// 4. Register In-Memory Caching & HTTP Clients for FortyGuard and Gemini AI
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IFortyGuardClient, FortyGuardClient>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); 
}); builder.Services.AddHttpClient<IGeminiAiService, GeminiAiService>();

// 5. Register Application & Infrastructure Services
builder.Services.AddScoped<INeighborhoodService, NeighborhoodService>();
builder.Services.AddScoped<ICsvImportService, CsvImportService>();
builder.Services.AddScoped<IRiskScoreCalculator, RiskScoreCalculator>();

// 6. Configure Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 7. Automatic Database Migration & Seeding for Fast MVP
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<VitalTempDbContext>();
        await DbInitializer.SeedAsync(context);
        app.Logger.LogInformation("Database initialized and seeded successfully with Phoenix Census Tracts.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// 8. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// 9. Serve Static Files for React Single Page Application (SPA)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();
