using System.IdentityModel.Tokens.Jwt;
using Identity.Application.Commands;
using Identity.Application.Dtos;
using Identity.Application.Queries;
using Identity.Application.Services;
using MediatR;
using TheShop.Api.Auth;

namespace TheShop.Api.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Identity");

        group.MapPost("/register", Register)
            .AllowAnonymous()
            .RequireRateLimiting("Auth")
            .Produces<UserDto>(StatusCodes.Status201Created);

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .RequireRateLimiting("Login")
            .Produces<TokenResponse>();

        group.MapGet("/me", GetCurrentUser)
            .RequireAuthorization()
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/refresh", RefreshToken)
            .AllowAnonymous()
            .RequireRateLimiting("Auth")
            .Produces<TokenResponse>();

        group.MapPost("/logout", Logout)
            .RequireAuthorization()
            .Produces<LogoutResponse>();

        group.MapGet("/sessions", GetActiveSessions)
            .RequireAuthorization()
            .Produces<IEnumerable<SessionDto>>();
        group.MapGet("/statistics", GetUserStatistics)
            .RequireAuthorization(Policies.AdminOnly)
            .Produces<UserStatisticsDto>();
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Created($"/api/auth/me", result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        HttpContext httpContext,
        ISender mediator,
        CancellationToken ct)
    {
        var deviceInfo = httpContext.Request.Headers.UserAgent.ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var command = new LoginCommand(
            request.Email,
            request.Password,
            deviceInfo,
            ipAddress);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> GetCurrentUser(
        ICurrentUser currentUser,
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetUserQuery(currentUser.Id);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> RefreshToken(
        RefreshTokenRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> Logout(
        LogoutRequest? request,
        HttpContext httpContext,
        ISender mediator,
        CancellationToken ct)
    {
        var jti = httpContext.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        if (string.IsNullOrEmpty(jti))
        {
            return Results.BadRequest("Invalid token");
        }

        var command = new LogoutCommand(jti, request?.LogoutAllDevices ?? false);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(new LogoutResponse("Logged out successfully"))
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> GetActiveSessions(
        ICurrentUser currentUser,
        ISessionService sessionService,
        CancellationToken ct)
    {
        var sessions = await sessionService.GetUserSessionsAsync(currentUser.Id, ct);

        var sessionDtos = sessions.Select(s => new SessionDto(
            s.SessionId,
            s.DeviceInfo,
            s.IpAddress,
            s.CreatedAt,
            s.LastActivityAt));

        return Results.Ok(sessionDtos);
    }

    private static async Task<IResult> GetUserStatistics(
        DateTime? fromDate,
        DateTime? toDate,
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetUserStatisticsQuery(fromDate, toDate);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }
}
