using FluentResults;
using Identity.Application.Services;
using Identity.Domain.Entities;
using MediatR;
using TheShop.SharedKernel;

namespace Identity.Application.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly ISessionService _sessionService;
    private readonly IRepository<ApplicationUser> _userRepository;

    public LogoutCommandHandler(
        ISessionService sessionService,
        IRepository<ApplicationUser> userRepository)
    {
        _sessionService = sessionService;
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        if (request.LogoutAllDevices)
        {
            var session = await _sessionService.GetSessionByJtiAsync(request.Jti, ct);
            
            if (session is null)
            {
                return Result.Fail("Session not found");
            }

            await _sessionService.RevokeAllUserSessionsAsync(session.UserId, ct);

            var user = await _userRepository.FindOneAsync(u => u.Id == session.UserId, ct);
            
            if (user is not null)
            {
                user.RevokeRefreshToken();
                await _userRepository.UpdateAsync(user, ct);
                await _userRepository.SaveChangesAsync(ct);
            }
        }
        else
        {
            await _sessionService.RevokeSessionAsync(request.Jti, ct);
        }

        return Result.Ok();
    }
}

