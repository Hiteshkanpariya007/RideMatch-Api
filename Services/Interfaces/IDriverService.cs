using Common.Enums;
using Services.Models;

namespace Services.Interfaces;

public interface IDriverService
{
    Task<DriverStatusResponse> GoOnlineAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<DriverLocationUpdateResult> UpdateLocationAsync(Guid userId, LocationRequest request, CancellationToken cancellationToken = default);

    Task<DriverStatusResponse> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResponse<RideResponse>> GetRidesAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<RideResponse> GetRideAsync(Guid userId, Guid rideId, CancellationToken cancellationToken = default);

    Task<RideResponse> ChangeRideStatusAsync(Guid userId, Guid rideId, RideStatus targetStatus, CancellationToken cancellationToken = default);
}

public sealed record DriverLocationUpdateResult(
    DriverStatusResponse Status,
    Guid? ActiveRideId,
    DateTime UpdatedAt);
