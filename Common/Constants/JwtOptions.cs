namespace Common.Constants
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = "RideMatch";
        public string Audience { get; set; } = "RideMatchClient";
        public string SecretKey { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
    }
}
