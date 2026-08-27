using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitalTemp.Application.Interfaces;
using VitalTemp.Domain.Entities;
using VitalTemp.Infrastructure.Data;

namespace VitalTemp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly VitalTempDbContext _context;
    private readonly ICsvImportService _csvImportService;
    private readonly IRiskScoreCalculator _riskScoreCalculator;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(
        VitalTempDbContext context,
        ICsvImportService csvImportService,
        IRiskScoreCalculator riskScoreCalculator,
        ILogger<LocationsController> logger)
    {
        _context = context;
        _csvImportService = csvImportService;
        _riskScoreCalculator = riskScoreCalculator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all registered census tracts/locations.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllLocations(CancellationToken ct = default)
    {
        var locations = await _context.Locations.AsNoTracking().ToListAsync(ct);
        return Ok(locations);
    }

    /// <summary>
    /// Imports Phoenix Census Tracts CSV file.
    /// </summary>
    [HttpPost("import-csv")]
    public async Task<IActionResult> ImportLocationsCsv(IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Please upload a valid non-empty CSV file." });
        }

        using var stream = file.OpenReadStream();
        var result = await _csvImportService.ImportLocationsCsvAsync(stream, ct);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        await _riskScoreCalculator.RecalculateAllAnalysisResultsAsync("ALL", ct);
        return Ok(result);
    }

    /// <summary>
    /// Imports CDC PLACES Health Metrics CSV file.
    /// </summary>
    [HttpPost("import-health-csv")]
    public async Task<IActionResult> ImportHealthDataCsv(IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Please upload a valid CDC PLACES CSV file." });
        }

        using var stream = file.OpenReadStream();
        var result = await _csvImportService.ImportHealthDataCsvAsync(stream, ct);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        await _riskScoreCalculator.RecalculateAllAnalysisResultsAsync("ALL", ct);
        return Ok(result);
    }

    /// <summary>
    /// Imports Hamza's unified FortyGuard + CDC PLACES Heat-Health Risk CSV.
    /// </summary>
    [HttpPost("import-hamza-risk-csv")]
    public async Task<IActionResult> ImportHamzaRiskCsv(IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Please upload a valid phoenix_heat_health_risk.csv file." });
        }

        using var stream = file.OpenReadStream();
        var result = await _csvImportService.ImportHamzaHeatHealthRiskCsvAsync(stream, ct);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Automatically seeds the 100 true Phoenix tracts from local phoenix_heat_health_risk.csv file.
    /// </summary>
    [HttpPost("seed-phoenix-risk-data")]
    public async Task<IActionResult> SeedPhoenixRiskData(CancellationToken ct = default)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "phoenix_heat_health_risk.csv");
        if (!System.IO.File.Exists(filePath))
        {
            filePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "phoenix_heat_health_risk.csv");
        }

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { message = "Could not locate phoenix_heat_health_risk.csv in project root." });
        }

        using var stream = System.IO.File.OpenRead(filePath);
        var result = await _csvImportService.ImportHamzaHeatHealthRiskCsvAsync(stream, ct);
        return Ok(result);
    }
}
