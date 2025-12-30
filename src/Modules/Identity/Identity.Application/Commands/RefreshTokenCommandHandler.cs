using FluentResults;
using Identity.Application.Dtos;
using Identity.Application.Services;
using Identity.Domain.Entities;
using MediatR;
using TheShop.SharedKernel;

namespace Identity.Application.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ISessionService _sessionService;

    private const int ACCESS_TOKEN_EXPIRATION_MINUTES = 360;

    public RefreshTokenCommandHandler(
        IRepository<ApplicationUser> userRepository,
        IJwtService jwtService,
        ISessionService sessionService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _sessionService = sessionService;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var user = await _userRepository.FindOneAsync(
            u => u.RefreshToken == request.RefreshToken, ct);

        if (user is null || !user.IsRefreshTokenValid(request.RefreshToken))
        {
            return Result.Fail<TokenResponse>("Invalid refresh token");
        }

        if (!string.IsNullOrEmpty(request.OldJti))
        {
            await _sessionService.RevokeSessionAsync(request.OldJti, ct);
        }

        var (accessToken, jti) = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        await _sessionService.CreateSessionAsync(
            userId: user.Id,
            email: user.Email,
            name: $"{user.FirstName} {user.LastName}",
            role: user.Role,
            jti: jti,
            expiration: TimeSpan.FromMinutes(ACCESS_TOKEN_EXPIRATION_MINUTES),
            ct: ct);

        user.UpdateRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _userRepository.UpdateAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        return Result.Ok(new TokenResponse(accessToken, newRefreshToken, ACCESS_TOKEN_EXPIRATION_MINUTES * 60));
    }
}

