using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Models;

namespace RideMatch_Api.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        
        public AuthController(ILoggerManager logger, IAuthService authService) : base(logger)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            return Created("api/v1/auth/register", response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _authService.LoginAsync(request, cancellationToken));
        }
    }
}
