using Data.Entities;

namespace Services.Repositories;

public interface IRideRepository
{
    Task<Ride?> GetByIdAsync(Guid rideId, CancellationToken cancellationToken = default);

    Task AddAsync(Ride ride, CancellationToken cancellationToken = default);

    Task<Guid?> GetRiderIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<(int TotalCount, IReadOnlyList<Ride> Items)> GetRiderRidesAsync(Guid riderId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Ride?> GetRiderRideAsync(Guid riderId, Guid rideId, CancellationToken cancellationToken = default);

    Task<Ride?> CancelRiderRideAsync(Guid riderId, Guid rideId, DateTime cancelledAt, CancellationToken cancellationToken = default);
}
