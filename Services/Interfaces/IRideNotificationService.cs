namespace Services.Interfaces;

public interface IRideNotificationService
{
    Task NotifyRideEventAsync(Guid rideId, string eventName, object payload, CancellationToken cancellationToken = default);
}
