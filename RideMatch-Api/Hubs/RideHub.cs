using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RideMatch_Api.Hubs
{
    [Authorize]
    public sealed class RideHub : Hub
    {
        public Task JoinRideGroup(Guid rideId) => Groups.AddToGroupAsync(Context.ConnectionId, RideGroup(rideId));
        public Task LeaveRideGroup(Guid rideId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, RideGroup(rideId));
        public static string RideGroup(Guid rideId) => $"ride:{rideId}";
    }
}
