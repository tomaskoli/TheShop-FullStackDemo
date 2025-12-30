using Identity.Application.Models;

namespace Identity.Application.Services;

public interface ISessionService
{
    Task<UserSession> CreateSessionAsync(
        Guid userId,
        string email,
        string name,
        Domain.Enums.UserRole role,
        string jti,
        TimeSpan expiration,
        string? deviceInfo = null,
        string? ipAddress = null,
        CancellationToken ct = default);

    Task<UserSession?> GetSessionAsync(Guid sessionId, CancellationToken ct = default);

    Task<UserSession?> GetSessionByJtiAsync(string jti, CancellationToken ct = default);

    Task<bool> ValidateSessionAsync(string jti, CancellationToken ct = default);

    Task UpdateActivityAsync(string jti, CancellationToken ct = default);

    Task RevokeSessionAsync(string jti, CancellationToken ct = default);

    Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct = default);

    Task<IEnumerable<UserSession>> GetUserSessionsAsync(Guid userId, CancellationToken ct = default);

    Task<int> CountActiveSessionsAsync(CancellationToken ct = default);
}

