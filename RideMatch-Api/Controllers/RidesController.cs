using Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Models;
using System.Security.Claims;

namespace RideMatch_Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Rider")]
    [Route("api/v1/riders/rides")]
    public class RidesController : BaseController
    {
        private readonly IRiderRideService _rideService;

        public RidesController(ILoggerManager logger, IRiderRideService rideService) : base(logger)
        {
            _rideService = rideService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRideRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
        {
            var result = await _rideService.CreateAsync(GetUserId(), request, idempotencyKey, cancellationToken);
            if (result.AlreadyCreated)
            {
                return Ok(new { success = true, message = "Ride already created.", data = result.Ride });
            }

            return CreatedAtAction(nameof(Get), new { rideId = result.Ride.Id }, new
            {
                success = true,
                message = "Ride created successfully.",
                data = result.Ride
            });
        }

        [HttpGet]
        public async Task<IActionResult> History([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            return Ok(await _rideService.GetHistoryAsync(GetUserId(), page, pageSize, cancellationToken));
        }

        [HttpGet("{rideId:guid}")]
        public async Task<IActionResult> Get(Guid rideId, CancellationToken cancellationToken)
        {
            return Ok(await _rideService.GetAsync(GetUserId(), rideId, cancellationToken));
        }

        [HttpPost("{rideId:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid rideId, CancellationToken cancellationToken)
        {
            return Ok(await _rideService.CancelAsync(GetUserId(), rideId, cancellationToken));
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


