using Data.Context;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Services.Models;

namespace Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly RideMatchContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly TokenService _tokenService;

        public AuthService(
            RideMatchContext dbContext,
            IPasswordHasher<User> passwordHasher,
            TokenService tokenService)
        {
            this._dbContext = dbContext;
            this._passwordHasher = passwordHasher;
            this._tokenService = tokenService;
        }
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await _dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken))
            {
                throw new InvalidOperationException("An account with this email already exists.");
            }

            var now = DateTime.UtcNow;
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Email = email,
                PhoneNumber = request.PhoneNumber.Trim(),
                Role = request.Role,
                CreatedAt = now,
                UpdatedAt = now
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _dbContext.Users.Add(user);

            if (request.Role == Common.Enums.UserRole.Driver)
            {
                _dbContext.Drivers.Add(new Driver
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    VehicleType = request.VehicleType!.Trim(),
                    VehicleNumber = request.VehicleNumber!.Trim(),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                _dbContext.Riders.Add(new Rider { Id = Guid.NewGuid(), UserId = user.Id, CreatedAt = now, UpdatedAt = now });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return CreateResponse(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);
            if (user is null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return CreateResponse(user);
        }

        private AuthResponse CreateResponse(User user)
        {
            var tokenResult = _tokenService.CreateToken(user); 
            return new AuthResponse(user.Id, user.Name, user.Email, user.Role, tokenResult.Token, tokenResult.ExpiresAt);
        }
    }
}

