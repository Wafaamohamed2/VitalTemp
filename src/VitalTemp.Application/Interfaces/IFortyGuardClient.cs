using VitalTemp.Application.DTOs;

namespace VitalTemp.Application.Interfaces;

public interface IFortyGuardClient
{
    Task<FortyGuardHeatmapResponse> GetPhoenixHeatmapAsync(FortyGuardHeatmapRequest request, CancellationToken cancellationToken = default);
    Task<int> SyncTemperaturesToLocationsAsync(CancellationToken cancellationToken = default);
}
