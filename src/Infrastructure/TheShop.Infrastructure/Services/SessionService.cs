using System.Text.Json;
using Identity.Application.Models;
using Identity.Application.Services;
using Identity.Domain.Enums;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace TheShop.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<SessionService> _logger;
    
    private const string SESSION_PREFIX = "session:";
    private const string JTI_PREFIX = "session:jti:";
    private const string USER_SESSIONS_PREFIX = "user:sessions:";

    public SessionService(IConnectionMultiplexer redis, ILogger<SessionService> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<UserSession> CreateSessionAsync(
        Guid userId,
        string email,
        string name,
        UserRole role,
        string jti,
        TimeSpan expiration,
        string? deviceInfo = null,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        var session = new UserSession
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            Name = name,
            Role = role,
            Jti = jti,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(expiration),
            IsRevoked = false
        };

        var sessionKey = GetSessionKey(session.SessionId);
        var jtiKey = GetJtiKey(jti);
        var userSessionsKey = GetUserSessionsKey(userId);

        var json = JsonSerializer.Serialize(session);

        await _database.StringSetAsync(sessionKey, json, expiration);
        await _database.StringSetAsync(jtiKey, session.SessionId.ToString(), expiration);
        await _database.SetAddAsync(userSessionsKey, session.SessionId.ToString());
        await _database.KeyExpireAsync(userSessionsKey, TimeSpan.FromDays(30));

        return session;
    }

    public async Task<UserSession?> GetSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var key = GetSessionKey(sessionId);
        var data = await _database.StringGetAsync(key);

        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<UserSession>(data.ToString());
    }

    public async Task<UserSession?> GetSessionByJtiAsync(string jti, CancellationToken ct = default)
    {
        var jtiKey = GetJtiKey(jti);
        var sessionIdValue = await _database.StringGetAsync(jtiKey);

        if (sessionIdValue.IsNullOrEmpty)
        {
            return null;
        }

        if (!Guid.TryParse(sessionIdValue.ToString(), out var sessionId))
        {
            return null;
        }

        return await GetSessionAsync(sessionId, ct);
    }

    public async Task<bool> ValidateSessionAsync(string jti, CancellationToken ct = default)
    {
        var session = await GetSessionByJtiAsync(jti, ct);

        if (session is null)
        {
            _logger.LogDebug("Session not found for JTI: {Jti}", jti);
            return false;
        }

        if (session.IsRevoked)
        {
            _logger.LogWarning("Attempted to use revoked session. JTI: {Jti}", jti);
            return false;
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogDebug("Session expired for JTI: {Jti}", jti);
            return false;
        }

        return true;
    }

    public async Task UpdateActivityAsync(string jti, CancellationToken ct = default)
    {
        var session = await GetSessionByJtiAsync(jti, ct);

        if (session is null)
        {
            return;
        }

        session.LastActivityAt = DateTime.UtcNow;

        var sessionKey = GetSessionKey(session.SessionId);
        var remainingTtl = session.ExpiresAt - DateTime.UtcNow;

        if (remainingTtl > TimeSpan.Zero)
        {
            var json = JsonSerializer.Serialize(session);
            await _database.StringSetAsync(sessionKey, json, remainingTtl);
        }
    }

    public async Task RevokeSessionAsync(string jti, CancellationToken ct = default)
    {
        var session = await GetSessionByJtiAsync(jti, ct);

        if (session is null)
        {
            return;
        }

        session.IsRevoked = true;

        var sessionKey = GetSessionKey(session.SessionId);
        var jtiKey = GetJtiKey(jti);

        var json = JsonSerializer.Serialize(session);
        await _database.StringSetAsync(sessionKey, json, TimeSpan.FromHours(1));
        await _database.KeyDeleteAsync(jtiKey);

        var userSessionsKey = GetUserSessionsKey(session.UserId);
        await _database.SetRemoveAsync(userSessionsKey, session.SessionId.ToString());

        _logger.LogInformation(
            "Session revoked for user {UserId}, SessionId: {SessionId}, JTI: {Jti}",
            session.UserId, session.SessionId, jti);
    }

    public async Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        var sessions = await GetUserSessionsAsync(userId, ct);
        
        foreach (var session in sessions)
        {
            await RevokeSessionAsync(session.Jti, ct);
        }

        _logger.LogInformation(
            "All sessions revoked for user {UserId}",
            userId);
    }

    public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        var userSessionsKey = GetUserSessionsKey(userId);
        var sessionIds = await _database.SetMembersAsync(userSessionsKey);

        var sessions = new List<UserSession>();

        foreach (var sessionIdValue in sessionIds)
        {
            if (Guid.TryParse(sessionIdValue.ToString(), out var sessionId))
            {
                var session = await GetSessionAsync(sessionId, ct);
                if (session is not null && !session.IsRevoked)
                {
                    sessions.Add(session);
                }
            }
        }

        return sessions;
    }

    public async Task<int> CountActiveSessionsAsync(CancellationToken ct = default)
    {
        var endpoints = _redis.GetEndPoints();
        if (endpoints.Length == 0)
        {
            return 0;
        }

        var server = _redis.GetServer(endpoints[0]);
        var count = 0;
        var keys = new List<RedisKey>();

        await foreach (var key in server.KeysAsync(pattern: $"{SESSION_PREFIX}*", pageSize: 1000))
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            var keyString = key.ToString();
            if (keyString.Contains(JTI_PREFIX))
            {
                continue;
            }

            keys.Add(key);
        }

        if (keys.Count == 0)
        {
            return 0;
        }

        var values = await _database.StringGetAsync(keys.ToArray());

        foreach (var data in values)
        {
            if (data.IsNullOrEmpty)
            {
                continue;
            }

            try
            {
                var session = JsonSerializer.Deserialize<UserSession>(data.ToString());
                if (session is not null && !session.IsRevoked && session.ExpiresAt > DateTime.UtcNow)
                {
                    count++;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize session data during count");
            }
        }

        return count;
    }

    private static string GetSessionKey(Guid sessionId) => $"{SESSION_PREFIX}{sessionId}";
    private static string GetJtiKey(string jti) => $"{JTI_PREFIX}{jti}";
    private static string GetUserSessionsKey(Guid userId) => $"{USER_SESSIONS_PREFIX}{userId}";
}

