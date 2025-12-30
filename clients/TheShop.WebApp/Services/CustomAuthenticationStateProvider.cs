using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace TheShop.WebApp.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string UserDataKey = "user_data";

    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private string? _accessToken;
    private string? _refreshToken;
    private UserDto? _currentUser;
    private int _basketItemCount;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public string? AccessToken => _accessToken;
    public string? RefreshToken => _refreshToken;
    public UserDto? CurrentUser => _currentUser;
    public int BasketItemCount => _basketItemCount;
    public bool IsAdmin => _currentUser?.Role == 1;

    public event Action? OnUserStateChanged;

    public CustomAuthenticationStateProvider(
        ProtectedSessionStorage sessionStorage,
        ILogger<CustomAuthenticationStateProvider> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _sessionStorage = sessionStorage;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var accessTokenResult = await _sessionStorage.GetAsync<string>(AccessTokenKey);
            var refreshTokenResult = await _sessionStorage.GetAsync<string>(RefreshTokenKey);
            var userResult = await _sessionStorage.GetAsync<UserDto>(UserDataKey);

            if (!accessTokenResult.Success || string.IsNullOrEmpty(accessTokenResult.Value))
            {
                return new AuthenticationState(_anonymous);
            }

            _accessToken = accessTokenResult.Value;
            _refreshToken = refreshTokenResult.Value;
            _currentUser = userResult.Value;

            var claims = ParseClaimsFromJwt(_accessToken);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch (InvalidOperationException)
        {
            // During prerendering JS interop (ProtectedSessionStorage) isn't available.
            // Fall back to HttpContext.User populated by ASP.NET Core authentication.
            var httpUser = _httpContextAccessor.HttpContext?.User;
            if (httpUser?.Identity?.IsAuthenticated == true)
            {
                return new AuthenticationState(httpUser);
            }

            return new AuthenticationState(_anonymous);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get authentication state from storage");
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task LoginAsync(string accessToken, string refreshToken, UserDto user)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _currentUser = user;

        await _sessionStorage.SetAsync(AccessTokenKey, accessToken);
        await _sessionStorage.SetAsync(RefreshTokenKey, refreshToken);
        await _sessionStorage.SetAsync(UserDataKey, user);

        var claims = ParseClaimsFromJwt(accessToken);
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        OnUserStateChanged?.Invoke();
    }

    public async Task LoginWithTokenAsync(string accessToken, string refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _currentUser = ParseUserFromJwt(accessToken);

        await _sessionStorage.SetAsync(AccessTokenKey, accessToken);
        await _sessionStorage.SetAsync(RefreshTokenKey, refreshToken);
        if (_currentUser != null)
        {
            await _sessionStorage.SetAsync(UserDataKey, _currentUser);
        }

        var claims = ParseClaimsFromJwt(accessToken);
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        OnUserStateChanged?.Invoke();
    }

    private static UserDto? ParseUserFromJwt(string jwt)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var idClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var emailClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var nameClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (idClaim == null || emailClaim == null)
            {
                return null;
            }

            var fullName = nameClaim?.Value ?? "";
            var nameParts = fullName.Split(' ', 2);
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            var role = roleClaim?.Value switch
            {
                "Admin" => 1,
                "Customer" => 0,
                _ => 0
            };

            return new UserDto
            {
                Id = Guid.Parse(idClaim.Value),
                Email = emailClaim.Value,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task UpdateTokensAsync(string accessToken, string? refreshToken = null)
    {
        _accessToken = accessToken;
        if (refreshToken != null)
        {
            _refreshToken = refreshToken;
            await _sessionStorage.SetAsync(RefreshTokenKey, refreshToken);
        }

        await _sessionStorage.SetAsync(AccessTokenKey, accessToken);

        var claims = ParseClaimsFromJwt(accessToken);
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task LogoutAsync()
    {
        _accessToken = null;
        _refreshToken = null;
        _currentUser = null;
        _basketItemCount = 0;

        try
        {
            await _sessionStorage.DeleteAsync(AccessTokenKey);
            await _sessionStorage.DeleteAsync(RefreshTokenKey);
            await _sessionStorage.DeleteAsync(UserDataKey);
        }
        catch (InvalidOperationException)
        {
            // JS interop not available during prerendering
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        OnUserStateChanged?.Invoke();
    }

    public void SetBasketCount(int count)
    {
        _basketItemCount = count;
        OnUserStateChanged?.Invoke();
    }

    public void ClearTokens()
    {
        _accessToken = null;
        _refreshToken = null;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            claims.AddRange(token.Claims);

            var roleClaim = token.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role);
            if (roleClaim != null && !claims.Any(c => c.Type == ClaimTypes.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
            }
        }
        catch
        {
            // If JWT parsing fails, return empty claims
        }

        return claims;
    }
}

