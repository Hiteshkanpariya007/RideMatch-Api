namespace Services.Interfaces;

public interface IRideMatchQueue
{
    ValueTask EnqueueAsync(Guid rideId, CancellationToken cancellationToken = default);
}
