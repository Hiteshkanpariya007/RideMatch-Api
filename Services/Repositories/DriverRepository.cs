using Data.Context;
using Data.Entities;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Services.Repositories
{
    public class DriverRepository(RideMatchContext dbContext) : IDriverRepository
    {
        public async Task<Driver?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Drivers
                .SingleOrDefaultAsync(d => d.UserId == userId, cancellationToken);
        }

        public async Task<Driver?> GetByIdAsync(Guid driverId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Drivers
                .SingleOrDefaultAsync(d => d.Id == driverId, cancellationToken);
        }

        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<DriverRidePage> GetRidesAsync(Guid driverId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Rides
                .AsNoTracking()
                .Where(ride => ride.DriverId == driverId)
                .OrderByDescending(ride => ride.RequestedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new DriverRidePage(totalCount, items);
        }

        public async Task<Ride?> GetRideAsync(Guid driverId, Guid rideId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Rides
                .AsNoTracking()
                .SingleOrDefaultAsync(ride => ride.Id == rideId && ride.DriverId == driverId, cancellationToken);
        }

        public async Task<Guid?> GetActiveRideIdAsync(Guid driverId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Rides
                .AsNoTracking()
                .Where(ride => ride.DriverId == driverId && ride.Status != RideStatus.Completed && ride.Status != RideStatus.Cancelled)
                .Select(ride => (Guid?)ride.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Ride?> ChangeRideStatusAsync(Guid driverId, Guid rideId, RideStatus targetStatus, CancellationToken cancellationToken = default)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var ride = await dbContext.Rides
                .SingleOrDefaultAsync(item => item.Id == rideId && item.DriverId == driverId, cancellationToken);

            if (ride is null)
            {
                return null;
            }

            var isValid = targetStatus switch
            {
                RideStatus.DriverAssigned => ride.Status is RideStatus.DriverAssigned or RideStatus.Searching,
                RideStatus.DriverArrived => ride.Status == RideStatus.DriverAssigned,
                RideStatus.Started => ride.Status == RideStatus.DriverArrived,
                RideStatus.Completed => ride.Status == RideStatus.Started,
                _ => false
            };

            if (!isValid)
            {
                throw new InvalidOperationException("Invalid ride state transition.");
            }

            var now = DateTime.UtcNow;
            ride.Status = targetStatus;

            if (targetStatus == RideStatus.DriverAssigned)
            {
                ride.AcceptedAt = now;
            }
            else if (targetStatus == RideStatus.Started)
            {
                ride.StartedAt = now;
            }
            else if (targetStatus == RideStatus.Completed)
            {
                ride.CompletedAt = now;
                var driver = await dbContext.Drivers.SingleAsync(item => item.Id == driverId, cancellationToken);
                driver.IsAvailable = true;
                driver.UpdatedAt = now;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ride;
        }
    }
}
