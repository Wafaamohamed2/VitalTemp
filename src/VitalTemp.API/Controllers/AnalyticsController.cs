using Microsoft.AspNetCore.Mvc;
using VitalTemp.Application.DTOs;
using VitalTemp.Application.Interfaces;

namespace VitalTemp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IFortyGuardClient _fortyGuardClient;
    private readonly IRiskScoreCalculator _riskScoreCalculator;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IFortyGuardClient fortyGuardClient,
        IRiskScoreCalculator riskScoreCalculator,
        ILogger<AnalyticsController> logger)
    {
        _fortyGuardClient = fortyGuardClient;
        _riskScoreCalculator = riskScoreCalculator;
        _logger = logger;
    }

    /// <summary>
    /// Fetches Phoenix thermal heatmap from FortyGuard API with Memory Caching layer.
    /// </summary>
    [HttpGet("heatmap")]
    [ProducesResponseType(typeof(FortyGuardHeatmapResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHeatmap([FromQuery] string date = "2023-08-18", CancellationToken ct = default)
    {
        var request = new FortyGuardHeatmapRequest { City = "Phoenix", Date = date };
        var heatmap = await _fortyGuardClient.GetPhoenixHeatmapAsync(request, ct);
        return Ok(heatmap);
    }

    /// <summary>
    /// Syncs thermal readings from FortyGuard into the SQLite database.
    /// </summary>
    [HttpPost("sync-temperatures")]
    public async Task<IActionResult> SyncTemperatures([FromQuery] string indicator = "ALL", CancellationToken ct = default)
    {
        int count = await _fortyGuardClient.SyncTemperaturesToLocationsAsync(ct);
        await _riskScoreCalculator.RecalculateAllAnalysisResultsAsync(indicator, ct);
        return Ok(new { message = $"Successfully synced {count} temperature readings and recalculated risk models.", count });
    }

    /// <summary>
    /// Triggers statistical correlation recalculation across all census tracts.
    /// </summary>
    [HttpPost("recalculate")]
    public async Task<IActionResult> Recalculate([FromQuery] string indicator = "ALL", CancellationToken ct = default)
    {
        int updated = await _riskScoreCalculator.RecalculateAllAnalysisResultsAsync(indicator, ct);
        return Ok(new { message = $"Recalculated correlation and risk indices for {updated} tracts.", updated });
    }
}
