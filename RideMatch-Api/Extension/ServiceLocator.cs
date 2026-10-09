using Microsoft.Extensions.Localization;

namespace RideMatch_Api.Extension
{
    public static class ServiceLocator
    {
        private static IHttpContextAccessor _httpContextAccessor;
        private static IStringLocalizer<Resource> _localizer;

        public static void Initialize(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public static T GetService<T>() => _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<T>();

        public class Resource
        {
        }
    }
}
