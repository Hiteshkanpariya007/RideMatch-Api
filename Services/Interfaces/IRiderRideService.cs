using Services.Models;

namespace Services.Interfaces;

public interface IRiderRideService
{
    Task<RideCreationResult> CreateAsync(Guid userId, CreateRideRequest request, string? idempotencyKey, CancellationToken cancellationToken = default);

    Task<PagedResponse<RideResponse>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<RideResponse> GetAsync(Guid userId, Guid rideId, CancellationToken cancellationToken = default);

    Task<RideResponse> CancelAsync(Guid userId, Guid rideId, CancellationToken cancellationToken = default);
}

public sealed record RideCreationResult(RideResponse Ride, bool AlreadyCreated);
