using Common.Constants;
using Common.Enums;
using Data.Entities;
using Common.Exceptions;
using Services.Interfaces;
using Services.Models;
using Services.Repositories;

namespace Services.Implementation;

public class DriverService : IDriverService
{
    private readonly IDriverRepository _driverRepository;
    private readonly IRedisService _redisService;
    private readonly IRideNotificationService _rideNotificationService;

    public DriverService(
        IDriverRepository driverRepository,
        IRedisService redisService,
        IRideNotificationService rideNotificationService)
    {
        this._driverRepository = driverRepository;
        this._redisService = redisService;
        this._rideNotificationService = rideNotificationService;
    }
    public async Task<DriverStatusResponse> GoOnlineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var driver = await GetDriverAsync(userId, cancellationToken);
        driver.IsOnline = true;
        driver.IsAvailable = true;
        driver.UpdatedAt = DateTime.UtcNow;
        await _driverRepository.SaveAsync(cancellationToken);
        await _redisService.SetAsync(RedisKeys.DriverAvailability(driver.Id), true, TimeSpan.FromMinutes(10), cancellationToken);
        return ToStatusResponse(driver);
    }

    public async Task<DriverLocationUpdateResult> UpdateLocationAsync(Guid userId, LocationRequest request, CancellationToken cancellationToken = default)
    {
        var driver = await GetDriverAsync(userId, cancellationToken);
        var now = DateTime.UtcNow;
        driver.CurrentLatitude = request.Latitude;
        driver.CurrentLongitude = request.Longitude;
        driver.LastLocationUpdatedAt = now;
        driver.UpdatedAt = now;
        await _driverRepository.SaveAsync(cancellationToken);
        await _redisService.SetAsync(RedisKeys.DriverLocation(driver.Id), new DriverLocationCacheItem(request.Latitude, request.Longitude, now), TimeSpan.FromMinutes(2), cancellationToken);

        var activeRideId = await _driverRepository.GetActiveRideIdAsync(driver.Id, cancellationToken);
        if (activeRideId.HasValue)
        {
            await _rideNotificationService.NotifyRideEventAsync(activeRideId.Value, "DriverLocationUpdated", new
            {
                driverId = driver.Id,
                request.Latitude,
                request.Longitude,
                updatedAt = now
            }, cancellationToken);
        }

        return new DriverLocationUpdateResult(ToStatusResponse(driver), activeRideId, now);
    }

    public async Task<DriverStatusResponse> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return ToStatusResponse(await GetDriverAsync(userId, cancellationToken));
    }

    public async Task<PagedResponse<RideResponse>> GetRidesAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var driver = await GetDriverAsync(userId, cancellationToken);
        var result = await _driverRepository.GetRidesAsync(driver.Id, page, pageSize, cancellationToken);
        var items = result.Items.Select(ToRideResponse).ToList();
        return new PagedResponse<RideResponse>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            TotalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize),
            Items = items
        };
    }

    public async Task<RideResponse> GetRideAsync(Guid userId, Guid rideId, CancellationToken cancellationToken = default)
    {
        var driver = await GetDriverAsync(userId, cancellationToken);
        var ride = await _driverRepository.GetRideAsync(driver.Id, rideId, cancellationToken)
            ?? throw new NotFoundException("Ride not found.");
        return ToRideResponse(ride);
    }

    public async Task<RideResponse> ChangeRideStatusAsync(Guid userId, Guid rideId, RideStatus targetStatus, CancellationToken cancellationToken = default)
    {
        var driver = await GetDriverAsync(userId, cancellationToken);
        Ride? ride;
        try
        {
            ride = await _driverRepository.ChangeRideStatusAsync(driver.Id, rideId, targetStatus, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        if (ride is null)
        {
            throw new NotFoundException("Ride not found.");
        }
        await _rideNotificationService.NotifyRideEventAsync(ride.Id, GetEventName(targetStatus), new
        {
            rideId = ride.Id,
            status = targetStatus.ToString()
        }, cancellationToken);
        return ToRideResponse(ride);
    }

    private async Task<Data.Entities.Driver> GetDriverAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _driverRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Driver profile not found.");
    }

    private static DriverStatusResponse ToStatusResponse(Data.Entities.Driver driver) => new()
    {
        DriverId = driver.Id,
        IsOnline = driver.IsOnline,
        IsAvailable = driver.IsAvailable,
        CurrentLatitude = driver.CurrentLatitude,
        CurrentLongitude = driver.CurrentLongitude,
        LastLocationUpdatedAt = driver.LastLocationUpdatedAt
    };

    private static RideResponse ToRideResponse(Data.Entities.Ride ride) => new()
    {
        Id = ride.Id,
        RiderId = ride.RiderId,
        DriverId = ride.DriverId,
        Status = ride.Status.ToString(),
        PickupLatitude = ride.PickupLatitude,
        PickupLongitude = ride.PickupLongitude,
        DestinationLatitude = ride.DestinationLatitude,
        DestinationLongitude = ride.DestinationLongitude,
        RequestedAt = ride.RequestedAt,
        AcceptedAt = ride.AcceptedAt,
        StartedAt = ride.StartedAt,
        CompletedAt = ride.CompletedAt,
        CancelledAt = ride.CancelledAt,
        Fare = ride.Fare
    };

    private static string GetEventName(RideStatus status) => status switch
    {
        RideStatus.DriverAssigned => "RideAccepted",
        RideStatus.DriverArrived => "DriverArrived",
        RideStatus.Started => "RideStarted",
        RideStatus.Completed => "RideCompleted",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported driver ride status.")
    };
}



