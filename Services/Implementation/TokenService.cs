using Common.Constants;
using Data.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Implementation
{
    public class TokenService(IOptions<JwtOptions> options)
    {
        public (string Token, DateTime ExpiresAt) CreateToken(User user)
        {
            var jwtOptions = options.Value;
            if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) || Encoding.UTF8.GetByteCount(jwtOptions.SecretKey) < 32)
            {
                throw new InvalidOperationException("Jwt:SecretKey must contain at least 32 characters.");
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
            var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(jwtOptions.Issuer, jwtOptions.Audience, claims, expires: expiresAt, signingCredentials: credentials);
            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
