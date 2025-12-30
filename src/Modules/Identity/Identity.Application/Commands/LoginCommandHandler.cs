using FluentResults;
using Identity.Application.Dtos;
using Identity.Application.Services;
using Identity.Domain.Entities;
using MediatR;
using TheShop.SharedKernel;

namespace Identity.Application.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ISessionService _sessionService;

    private const int ACCESS_TOKEN_EXPIRATION_MINUTES = 360;

    public LoginCommandHandler(
        IRepository<ApplicationUser> userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ISessionService sessionService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _sessionService = sessionService;
    }

    public async Task<Result<TokenResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.FindOneAsync(u => u.Email == request.Email, ct);

        if (user is null)
        {
            return Result.Fail<TokenResponse>("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return Result.Fail<TokenResponse>("Account is deactivated");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Fail<TokenResponse>("Invalid email or password");
        }

        var (accessToken, jti) = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await _sessionService.CreateSessionAsync(
            userId: user.Id,
            email: user.Email,
            name: $"{user.FirstName} {user.LastName}",
            role: user.Role,
            jti: jti,
            expiration: TimeSpan.FromMinutes(ACCESS_TOKEN_EXPIRATION_MINUTES),
            deviceInfo: request.DeviceInfo,
            ipAddress: request.IpAddress,
            ct: ct);

        user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _userRepository.UpdateAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        return Result.Ok(new TokenResponse(accessToken, refreshToken, ACCESS_TOKEN_EXPIRATION_MINUTES * 60));
    }
}

