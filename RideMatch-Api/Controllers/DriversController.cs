using Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Services.Interfaces;
using Services.Models;
using System.Security.Claims;

namespace RideMatch_Api.Controllers
{
    [Authorize(Roles = "Driver")]
    [Route("api/v1/drivers")]
    public class DriversController : BaseController
    {
        private readonly IDriverService _driverService;

        public DriversController(ILoggerManager logger, IDriverService driverService) : base(logger)
        {
            _driverService = driverService;
        }

        [HttpPost("online")]
        public async Task<IActionResult> GoOnline(CancellationToken cancellationToken)
        {
            var status = await _driverService.GoOnlineAsync(GetUserId(), cancellationToken);
            return Ok(new { success = true, message = "Driver is now online.", data = status });
        }

        [HttpPost("location")]
        [EnableRateLimiting("driver-location")]
        public async Task<IActionResult> UpdateLocation(LocationRequest request, CancellationToken cancellationToken)
        {
            var result = await _driverService.UpdateLocationAsync(GetUserId(), request, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Driver location updated.",
                data = new
                {
                    driverId = result.Status.DriverId,
                    request.Latitude,
                    request.Longitude,
                    updatedAt = result.UpdatedAt
                }
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            return Ok(await _driverService.GetStatusAsync(GetUserId(), cancellationToken));
        }

        [HttpGet("status")]
        public Task<IActionResult> Status(CancellationToken cancellationToken) => Me(cancellationToken);

        [HttpGet("rides")]
        public async Task<IActionResult> Rides([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            return Ok(await _driverService.GetRidesAsync(GetUserId(), page, pageSize, cancellationToken));
        }

        [HttpGet("rides/{rideId:guid}")]
        public async Task<IActionResult> GetRide(Guid rideId, CancellationToken cancellationToken)
        {
            return Ok(await _driverService.GetRideAsync(GetUserId(), rideId, cancellationToken));
        }

        [HttpPost("rides/{rideId:guid}/accept")]
        public Task<IActionResult> Accept(Guid rideId, CancellationToken cancellationToken) =>
            ChangeStatus(rideId, RideStatus.DriverAssigned, cancellationToken);

        [HttpPost("rides/{rideId:guid}/arrived")]
        public Task<IActionResult> Arrived(Guid rideId, CancellationToken cancellationToken) =>
            ChangeStatus(rideId, RideStatus.DriverArrived, cancellationToken);

        [HttpPost("rides/{rideId:guid}/start")]
        public Task<IActionResult> Start(Guid rideId, CancellationToken cancellationToken) =>
            ChangeStatus(rideId, RideStatus.Started, cancellationToken);

        [HttpPost("rides/{rideId:guid}/complete")]
        public Task<IActionResult> Complete(Guid rideId, CancellationToken cancellationToken) =>
            ChangeStatus(rideId, RideStatus.Completed, cancellationToken);

        private async Task<IActionResult> ChangeStatus(Guid rideId, RideStatus targetStatus, CancellationToken cancellationToken)
        {
            var ride = await _driverService.ChangeRideStatusAsync(GetUserId(), rideId, targetStatus, cancellationToken);
            return Ok(ride);
        }

        private Guid GetUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException("Authenticated user identifier is missing or invalid.");
            }

            return userId;
        }
    }
}
