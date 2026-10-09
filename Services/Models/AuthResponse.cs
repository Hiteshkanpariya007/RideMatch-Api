using Common.Enums;

namespace Services.Models;

public sealed class AuthResponse
{
    public AuthResponse(Guid userId, string name, string email, UserRole role, string token, DateTime expiresAt)
    {
        UserId = userId;
        Name = name;
        Email = email;
        Role = role;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public Guid UserId { get; }
    public string Name { get; }
    public string Email { get; }
    public UserRole Role { get; }
    public string Token { get; }
    public DateTime ExpiresAt { get; }
}
