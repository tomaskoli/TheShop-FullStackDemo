using Identity.Domain.Enums;

namespace Identity.Application.Dtos;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);

public record LoginRequest(
    string Email,
    string Password);

public record RefreshTokenRequest(
    string RefreshToken);

public record LogoutRequest(
    bool LogoutAllDevices = false);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    DateTime CreatedAt,
    bool IsActive);

public record SessionDto(
    Guid SessionId,
    string? DeviceInfo,
    string? IpAddress,
    DateTime CreatedAt,
    DateTime LastActivityAt);

public record LogoutResponse(string Message);

