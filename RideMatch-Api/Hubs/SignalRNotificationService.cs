using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;

namespace RideMatch_Api.Hubs
{
    public sealed class SignalRNotificationService(IHubContext<RideHub> hubContext) : IRideNotificationService
    {
        public async Task NotifyRideEventAsync(Guid rideId, string eventName, object payload, CancellationToken cancellationToken = default)
        {
            await hubContext.Clients.Group(RideHub.RideGroup(rideId)).SendAsync(eventName, payload, cancellationToken);
        }
    }
}
