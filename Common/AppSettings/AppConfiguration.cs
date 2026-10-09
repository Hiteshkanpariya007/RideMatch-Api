namespace Common.AppSettings
{
    public class AppConfiguration
    {
        public string CurrentEnv { get; set; } = string.Empty;

        public string JWTKey { get; set; } = string.Empty;

        public string JWTAudience { get; set; } = string.Empty;

        public int JWTExpiresIn { get; set; } = Constants.Common.TokenExpiryMinutes;
    }
}
