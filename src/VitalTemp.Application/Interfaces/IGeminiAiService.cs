using VitalTemp.Application.DTOs;

namespace VitalTemp.Application.Interfaces;

public interface IGeminiAiService
{
    Task<GeminiRecommendationDto> GenerateNeighborhoodRecommendationsAsync(int locationId, CancellationToken cancellationToken = default);
}
