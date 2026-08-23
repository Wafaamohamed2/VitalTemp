using Microsoft.AspNetCore.Mvc;
using VitalTemp.Application.DTOs;
using VitalTemp.Application.Interfaces;

namespace VitalTemp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NeighborhoodsController : ControllerBase
{
    private readonly INeighborhoodService _neighborhoodService;
    private readonly ILogger<NeighborhoodsController> _logger;

    public NeighborhoodsController(INeighborhoodService neighborhoodService, ILogger<NeighborhoodsController> logger)
    {
        _neighborhoodService = neighborhoodService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all Phoenix census tracts with their calculated heat and health risk scores.
    /// Fast MVP Contract Endpoint: GET /api/neighborhoods/risk-scores?indicator=ALL
    /// </summary>
    [HttpGet("risk-scores")]
    [ProducesResponseType(typeof(IEnumerable<NeighborhoodRiskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRiskScores([FromQuery] string indicator = "ALL")
    {
        _logger.LogInformation("Fetching risk scores for Phoenix neighborhoods (Indicator: {Indicator})", indicator);
        var results = await _neighborhoodService.GetRiskScoresAsync(indicator);
        return Ok(results);
    }

    /// <summary>
    /// Gets detailed historical and correlation data for a specific census tract.
    /// Endpoint: GET /api/neighborhoods/{id}?indicator=ALL
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(NeighborhoodDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNeighborhoodDetails(int id, [FromQuery] string indicator = "ALL")
    {
        var result = await _neighborhoodService.GetNeighborhoodDetailsAsync(id, indicator);
        if (result == null)
        {
            return NotFound(new { message = $"Neighborhood with ID {id} not found." });
        }
        return Ok(result);
    }

    /// <summary>
    /// Gets summary KPI metrics for the overall dashboard header.
    /// Endpoint: GET /api/neighborhoods/summary?indicator=ALL
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardSummary([FromQuery] string indicator = "ALL")
    {
        var summary = await _neighborhoodService.GetDashboardSummaryAsync(indicator);
        return Ok(summary);
    }
}
