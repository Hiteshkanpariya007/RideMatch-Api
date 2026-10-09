using Common.Enums;

namespace Services.Models
{
    public class AuthResponse
    {
        public AuthResponse(Guid id, string name, string email, UserRole role, object token, object expiresAt)
        {
            Id = id;
            Name = name;
            Email = email;
            Role = role;
            Token1 = token;
            ExpiresAt1 = expiresAt;
        }

        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Guid Id { get; }
        public object Token1 { get; }
        public object ExpiresAt1 { get; }
    }
}
