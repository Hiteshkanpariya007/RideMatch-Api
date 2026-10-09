using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Services.Implementation
{
    public class RideMatchWorker(
    RideMatchQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<RideMatchWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var rideId in queue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var matchingService = scope.ServiceProvider.GetRequiredService<RideMatchService>();
                    await matchingService.MatchAsync(rideId, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Ride matching failed for ride {RideId}", rideId);
                }
            }
        }
    }
}
