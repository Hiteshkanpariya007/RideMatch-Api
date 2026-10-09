using Common.Constants;
using Common.Enums;
using Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Interfaces;
using System.Data;

namespace Services.Implementation
{
    public class RideMatchService(RideMatchContext dbContext,
    IOptions<MatchingOptions> options,
    IRideNotificationService notificationService,
    ILogger<RideMatchService> logger) : IRideMatchService
    {
        private const double EarthRadiusKm = 6371;
        private readonly MatchingOptions matchingOptions = options.Value;

        public async Task MatchAsync(Guid rideId, CancellationToken cancellationToken = default)
        {
            var ride = await dbContext.Rides.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == rideId, cancellationToken);
            if (ride is null || ride.Status is RideStatus.Cancelled or RideStatus.Completed || ride.DriverId.HasValue)
            {
                return;
            }

            await notificationService.NotifyRideEventAsync(rideId, "RideSearching", new { rideId, status = RideStatus.Searching.ToString() }, cancellationToken);
            var staleBefore = DateTime.UtcNow.AddSeconds(-matchingOptions.DriverLocationExpirySeconds);
            var radius = Math.Max(0, matchingOptions.InitialRadiusKm);

            while (radius <= matchingOptions.MaxRadiusKm)
            {
                var candidates = await FindCandidatesAsync(ride.PickupLatitude, ride.PickupLongitude, radius, staleBefore, cancellationToken);
                foreach (var candidate in candidates)
                {
                    if (await TryAssignAsync(rideId, candidate.DriverId, cancellationToken))
                    {
                        logger.LogInformation("Driver {DriverId} assigned to ride {RideId} at {DistanceKm} km", candidate.DriverId, rideId, candidate.DistanceKm);
                        await notificationService.NotifyRideEventAsync(rideId, "DriverAssigned", new { rideId, driverId = candidate.DriverId, distanceKm = candidate.DistanceKm }, cancellationToken);
                        return;
                    }
                }

                radius += Math.Max(1, matchingOptions.RadiusIncrementKm);
            }

            logger.LogInformation("No eligible driver found for ride {RideId}", rideId);
            await notificationService.NotifyRideEventAsync(rideId, "RideSearchFailed", new { rideId, status = "No driver available" }, cancellationToken);
        }

        private async Task<IReadOnlyList<MatchingCandidate>> FindCandidatesAsync(decimal latitude, decimal longitude, double radiusKm, DateTime staleBefore, CancellationToken cancellationToken)
        {
            var latitudeDelta = radiusKm / 111.0;
            var longitudeDelta = radiusKm / (111.0 * Math.Max(0.01, Math.Cos(ToRadians((double)latitude))));
            var minLatitude = (double)latitude - latitudeDelta;
            var maxLatitude = (double)latitude + latitudeDelta;
            var minLongitude = (double)longitude - longitudeDelta;
            var maxLongitude = (double)longitude + longitudeDelta;

            var drivers = await dbContext.Drivers.AsNoTracking()
                .Where(driver => driver.IsOnline && driver.IsAvailable && driver.LastLocationUpdatedAt >= staleBefore
                    && driver.CurrentLatitude >= (decimal)minLatitude && driver.CurrentLatitude <= (decimal)maxLatitude
                    && driver.CurrentLongitude >= (decimal)minLongitude && driver.CurrentLongitude <= (decimal)maxLongitude)
                .Select(driver => new { driver.Id, driver.CurrentLatitude, driver.CurrentLongitude })
                .ToListAsync(cancellationToken);

            return drivers
                .Where(driver => driver.CurrentLatitude.HasValue && driver.CurrentLongitude.HasValue)
                .Select(driver => new MatchingCandidate(driver.Id, DistanceKm((double)latitude, (double)longitude, (double)driver.CurrentLatitude!.Value, (double)driver.CurrentLongitude!.Value)))
                .Where(candidate => candidate.DistanceKm <= radiusKm)
                .OrderBy(candidate => candidate.DistanceKm)
                .Take(matchingOptions.MaxCandidates)
                .ToArray();
        }

        private async Task<bool> TryAssignAsync(Guid rideId, Guid driverId, CancellationToken cancellationToken)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var ride = await dbContext.Rides.SingleOrDefaultAsync(candidate => candidate.Id == rideId, cancellationToken);
            var driver = await dbContext.Drivers.SingleOrDefaultAsync(candidate => candidate.Id == driverId, cancellationToken);
            if (ride is null || driver is null || ride.DriverId.HasValue || ride.Status is RideStatus.Cancelled or RideStatus.Completed || !driver.IsOnline || !driver.IsAvailable)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            ride.DriverId = driver.Id;
            ride.Status = RideStatus.DriverAssigned;
            driver.IsAvailable = false;
            driver.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        private static double DistanceKm(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            var latitudeDelta = ToRadians(latitude2 - latitude1);
            var longitudeDelta = ToRadians(longitude2 - longitude1);
            var a = Math.Pow(Math.Sin(latitudeDelta / 2), 2) + Math.Cos(ToRadians(latitude1)) * Math.Cos(ToRadians(latitude2)) * Math.Pow(Math.Sin(longitudeDelta / 2), 2);
            return EarthRadiusKm * 2 * Math.Asin(Math.Sqrt(a));
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;
    }
}
