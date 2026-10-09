using Services.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Services.Implementation
{
    public class RedisService(IConnectionMultiplexer connectionMultiplexer) : IRedisService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly IDatabase database = connectionMultiplexer.GetDatabase();

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var value = await database.StringGetAsync(key);
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value.ToString(), SerializerOptions);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var serialized = JsonSerializer.Serialize(value, SerializerOptions);
            await database.StringSetAsync(key, serialized, expiry);
        }

        public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var serialized = JsonSerializer.Serialize(value, SerializerOptions);
            return await database.StringSetAsync(key, serialized, expiry, When.NotExists);
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await database.KeyExistsAsync(key);
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await database.KeyDeleteAsync(key);
        }
    }
}
