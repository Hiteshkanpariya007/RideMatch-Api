using Common.Enums;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Services.Repositories
{
    public class RideRepository(RideMatchContext dbContext) : IRideRepository
    {
        public async Task<Ride?> GetByIdAsync(Guid rideId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Rides.SingleOrDefaultAsync(ride => ride.Id == rideId, cancellationToken);
        }

        public async Task AddAsync(Ride ride, CancellationToken cancellationToken = default)
        {
            await dbContext.Rides.AddAsync(ride, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Guid?> GetRiderIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Riders.AsNoTracking()
                .Where(rider => rider.UserId == userId)
                .Select(rider => (Guid?)rider.Id)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<(int TotalCount, IReadOnlyList<Ride> Items)> GetRiderRidesAsync(Guid riderId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Rides.AsNoTracking()
                .Where(ride => ride.RiderId == riderId)
                .OrderByDescending(ride => ride.RequestedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return (totalCount, items);
        }

        public async Task<Ride?> GetRiderRideAsync(Guid riderId, Guid rideId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Rides.AsNoTracking()
                .SingleOrDefaultAsync(ride => ride.Id == rideId && ride.RiderId == riderId, cancellationToken);
        }

        public async Task<Ride?> CancelRiderRideAsync(Guid riderId, Guid rideId, DateTime cancelledAt, CancellationToken cancellationToken = default)
        {
            var ride = await dbContext.Rides
                .SingleOrDefaultAsync(item => item.Id == rideId && item.RiderId == riderId, cancellationToken);
            if (ride is null)
            {
                return null;
            }

            if (ride.Status is not (RideStatus.Requested or RideStatus.Searching or RideStatus.DriverAssigned))
            {
                throw new InvalidOperationException("Ride cannot be cancelled in its current state.");
            }

            if (ride.Status == RideStatus.DriverAssigned && ride.DriverId.HasValue)
            {
                var driver = await dbContext.Drivers.SingleOrDefaultAsync(item => item.Id == ride.DriverId.Value, cancellationToken);
                if (driver is not null)
                {
                    driver.IsAvailable = true;
                    driver.UpdatedAt = cancelledAt;
                }
            }

            ride.Status = RideStatus.Cancelled;
            ride.CancelledAt = cancelledAt;
            await dbContext.SaveChangesAsync(cancellationToken);
            return ride;
        }
    }
}

