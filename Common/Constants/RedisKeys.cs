namespace Common.Constants
{
    public static class RedisKeys
    {
        public static string DriverLocation(Guid driverId) => $"driver:location:{driverId}";
        public static string DriverAvailability(Guid driverId) => $"driver:availability:{driverId}";
        public static string Idempotency(Guid userId, string key) => $"idempotency:{userId}:{key}";
        public static string RideLock(Guid rideId) => $"lock:ride:{rideId}";
    }
}

