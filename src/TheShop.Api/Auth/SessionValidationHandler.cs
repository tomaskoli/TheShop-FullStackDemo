using System.IdentityModel.Tokens.Jwt;
using Identity.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace TheShop.Api.Auth;

public class SessionValidationHandler : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var sessionService = context.HttpContext.RequestServices
            .GetRequiredService<ISessionService>();

        var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        if (string.IsNullOrEmpty(jti))
        {
            context.Fail("Token does not contain a valid JTI claim.");
            return;
        }

        var isValid = await sessionService.ValidateSessionAsync(jti);

        if (!isValid)
        {
            context.Fail("Session is invalid or has been revoked.");
            return;
        }

        _ = UpdateActivitySafeAsync(context.HttpContext.RequestServices, sessionService, jti);

        await base.TokenValidated(context);
    }

    private static async Task UpdateActivitySafeAsync(
        IServiceProvider services,
        ISessionService sessionService,
        string jti)
    {
        try
        {
            await sessionService.UpdateActivityAsync(jti);
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown, ignore
        }
        catch (Exception ex)
        {
            var logger = services.GetService<ILogger<SessionValidationHandler>>();
            logger?.LogWarning(ex, "Failed to update session activity for JTI {Jti}", jti);
        }
    }

    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<SessionValidationHandler>>();

        if (context.Exception is SecurityTokenExpiredException)
        {
            logger.LogDebug("JWT token expired for request to {Path}", context.Request.Path);
        }
        else
        {
            logger.LogWarning(
                context.Exception,
                "JWT authentication failed: {Message}",
                context.Exception.Message);
        }

        return base.AuthenticationFailed(context);
    }
}

