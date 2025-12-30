using System.IdentityModel.Tokens.Jwt;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TheShop.WebApp.Services;

public sealed class TokenCookieAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TokenCookieAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(TokenCookieAuthenticationDefaults.AccessTokenCookieName, out var rawToken) ||
            string.IsNullOrWhiteSpace(rawToken))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(rawToken);

            var identity = new System.Security.Claims.ClaimsIdentity(token.Claims, TokenCookieAuthenticationDefaults.SchemeName);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, TokenCookieAuthenticationDefaults.SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            return Task.FromResult(AuthenticateResult.Fail(ex));
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var returnUrl = Uri.EscapeDataString($"{Request.PathBase}{Request.Path}{Request.QueryString}");
        Response.Redirect($"/login?returnUrl={returnUrl}");
        return Task.CompletedTask;
    }
}


