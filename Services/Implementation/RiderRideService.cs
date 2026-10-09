using Common.Constants;
using Common.Enums;
using Common.Exceptions;
using Data.Entities;
using Services.Interfaces;
using Services.Models;
using Services.Repositories;

namespace Services.Implementation;

public class RiderRideService : IRiderRideService
{
    private readonly IRideRepository _rideRepository;
    private readonly IRedisService _redisService;
    private readonly IRideMatchQueue _matchingQueue;
    private readonly IRideNotificationService _notificationService;

    public RiderRideService(
        IRideRepository rideRepository,
        IRedisService redisService,
        IRideMatchQueue matchingQueue,
        IRideNotificationService notificationService)
    {
        _rideRepository = rideRepository;
        _redisService = redisService;
        _matchingQueue = matchingQueue;
        _notificationService = notificationService;
    }
    public async Task<RideCreationResult> CreateAsync(Guid userId, CreateRideRequest request, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await _redisService.GetAsync<RideResponse>(RedisKeys.Idempotency(userId, idempotencyKey), cancellationToken);
            if (existing is not null)
            {
                return new RideCreationResult(existing, true);
            }
        }

        var riderId = await GetRiderIdAsync(userId, cancellationToken);
        var now = DateTime.UtcNow;
        var ride = new Ride
        {
            Id = Guid.NewGuid(),
            RiderId = riderId,
            PickupLatitude = request.PickupLatitude,
            PickupLongitude = request.PickupLongitude,
            DestinationLatitude = request.DestinationLatitude,
            DestinationLongitude = request.DestinationLongitude,
            Status = RideStatus.Searching,
            RequestedAt = now
        };

        await _rideRepository.AddAsync(ride, cancellationToken);
        var response = ToResponse(ride);
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _redisService.SetAsync(RedisKeys.Idempotency(userId, idempotencyKey), response, TimeSpan.FromHours(24), cancellationToken);
        }

        await _matchingQueue.EnqueueAsync(ride.Id, cancellationToken);
        await _notificationService.NotifyRideEventAsync(ride.Id, "RideSearching", new { rideId = ride.Id, status = ride.Status.ToString() }, cancellationToken);
        return new RideCreationResult(response, false);
    }

    public async Task<PagedResponse<RideResponse>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var riderId = await GetRiderIdAsync(userId, cancellationToken);
        var result = await _rideRepository.GetRiderRidesAsync(riderId, page, pageSize, cancellationToken);
        return new PagedResponse<RideResponse>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            TotalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize),
            Items = result.Items.Select(ToResponse).ToList()
        };
    }

    public async Task<RideResponse> GetAsync(Guid userId, Guid rideId, CancellationToken cancellationToken = default)
    {
        var riderId = await GetRiderIdAsync(userId, cancellationToken);
        var ride = await _rideRepository.GetRiderRideAsync(riderId, rideId, cancellationToken)
            ?? throw new NotFoundException("Ride not found.");
        return ToResponse(ride);
    }

    public async Task<RideResponse> CancelAsync(Guid userId, Guid rideId, CancellationToken cancellationToken = default)
    {
        var riderId = await GetRiderIdAsync(userId, cancellationToken);
        Ride? ride;
        try
        {
            ride = await _rideRepository.CancelRiderRideAsync(riderId, rideId, DateTime.UtcNow, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        if (ride is null)
        {
            throw new NotFoundException("Ride not found.");
        }

        await _notificationService.NotifyRideEventAsync(ride.Id, "RideCancelled", new { rideId = ride.Id, status = ride.Status.ToString() }, cancellationToken);
        return ToResponse(ride);
    }

    private async Task<Guid> GetRiderIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _rideRepository.GetRiderIdByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Rider profile not found.");
    }

    private static RideResponse ToResponse(Ride ride) => new()
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
}


