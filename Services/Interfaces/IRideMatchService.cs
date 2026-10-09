namespace Services.Interfaces;

public interface IRideMatchService
{
    Task MatchAsync(Guid rideId, CancellationToken cancellationToken = default);
}
