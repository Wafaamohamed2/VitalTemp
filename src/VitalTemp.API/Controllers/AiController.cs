using Microsoft.AspNetCore.Mvc;
using VitalTemp.Application.DTOs;
using VitalTemp.Application.Interfaces;

namespace VitalTemp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IGeminiAiService _geminiAiService;
    private readonly ILogger<AiController> _logger;

    public AiController(IGeminiAiService geminiAiService, ILogger<AiController> logger)
    {
        _geminiAiService = geminiAiService;
        _logger = logger;
    }

    /// <summary>
    /// Generates tailored tactical heat mitigation and public health recommendations using Google Gemini AI.
    /// Endpoint: POST /api/ai/recommendations/{locationId}
    /// </summary>
    [HttpPost("recommendations/{locationId:int}")]
    [ProducesResponseType(typeof(GeminiRecommendationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateRecommendations(int locationId, [FromQuery] string indicator = "ALL", CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Triggering Gemini AI recommendation generation for Location ID {LocationId} (Indicator: {Indicator})", locationId, indicator);
            var result = await _geminiAiService.GenerateNeighborhoodRecommendationsAsync(locationId, indicator, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Census tract with ID {locationId} not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Gemini recommendations for Location ID {LocationId}", locationId);
            return StatusCode(500, new { message = "An error occurred while generating AI recommendations.", error = ex.Message });
        }
    }
}
