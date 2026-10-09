using Services.Interfaces;
using System.Text.Json;

namespace Services.Implementation;

/// <summary>
/// Process-local Redis substitute for a single-instance Development run.
/// Production and multi-instance deployments should use RedisService.
/// </summary>
public sealed class InMemoryRedisService : IRedisService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly object gate = new();
    private readonly Dictionary<string, CacheEntry> entries = new(StringComparer.Ordinal);

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string? value;
        lock (gate)
        {
            if (!TryGetEntry(key, out var entry)) return Task.FromResult<T?>(default);
            value = entry.Value;
        }
        return Task.FromResult(JsonSerializer.Deserialize<T>(value, SerializerOptions));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var serialized = JsonSerializer.Serialize(value, SerializerOptions);
        DateTimeOffset? expiresAt = expiry is null ? null : DateTimeOffset.UtcNow.Add(expiry.Value);
        lock (gate) entries[key] = new CacheEntry(serialized, expiresAt);
        return Task.CompletedTask;
    }

    public Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var serialized = JsonSerializer.Serialize(value, SerializerOptions);
        lock (gate)
        {
            if (TryGetEntry(key, out _)) return Task.FromResult(false);
            entries[key] = new CacheEntry(serialized, DateTimeOffset.UtcNow.Add(expiry));
            return Task.FromResult(true);
        }
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (gate) return Task.FromResult(TryGetEntry(key, out _));
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (gate) entries.Remove(key);
        return Task.CompletedTask;
    }

    private bool TryGetEntry(string key, out CacheEntry entry)
    {
        if (entries.TryGetValue(key, out entry!))
        {
            if (entry.ExpiresAt is null || entry.ExpiresAt > DateTimeOffset.UtcNow) return true;
            entries.Remove(key);
        }
        entry = null!;
        return false;
    }

    private sealed record CacheEntry(string Value, DateTimeOffset? ExpiresAt);
}

