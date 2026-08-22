using VitalTemp.Application.DTOs;

namespace VitalTemp.Application.Interfaces;

public interface INeighborhoodService
{
    Task<IEnumerable<NeighborhoodRiskDto>> GetRiskScoresAsync(string indicator = "ALL");
    Task<NeighborhoodDetailDto?> GetNeighborhoodDetailsAsync(int locationId, string indicator = "ALL");
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(string indicator = "ALL");
}
