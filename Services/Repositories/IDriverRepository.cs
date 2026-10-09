using Data.Entities;
using Common.Enums;
namespace Services.Repositories;

public interface IDriverRepository
{
    Task<Driver?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Driver?> GetByIdAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);

    Task<DriverRidePage> GetRidesAsync(Guid driverId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Ride?> GetRideAsync(Guid driverId, Guid rideId, CancellationToken cancellationToken = default);

    Task<Guid?> GetActiveRideIdAsync(Guid driverId, CancellationToken cancellationToken = default);

    Task<Ride?> ChangeRideStatusAsync(Guid driverId, Guid rideId, RideStatus targetStatus, CancellationToken cancellationToken = default);
}

public sealed record DriverRidePage(int TotalCount, IReadOnlyList<Ride> Items);
