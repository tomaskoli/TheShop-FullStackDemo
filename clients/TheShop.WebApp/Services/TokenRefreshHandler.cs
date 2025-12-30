using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TheShop.WebApp.Services;

public class TokenRefreshHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenRefreshHandler> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public TokenRefreshHandler(IServiceProvider serviceProvider, ILogger<TokenRefreshHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private CustomAuthenticationStateProvider GetAuthProvider()
    {
        return _serviceProvider.GetRequiredService<CustomAuthenticationStateProvider>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        if (IsRefreshEndpoint(request.RequestUri))
        {
            return response;
        }

        var authProvider = GetAuthProvider();
        if (string.IsNullOrEmpty(authProvider.RefreshToken))
        {
            _logger.LogDebug("Received 401 but no refresh token available");
            return response;
        }

        var refreshed = await TryRefreshTokenAsync(authProvider, cancellationToken);
        if (!refreshed)
        {
            return response;
        }

        response.Dispose();

        var retryRequest = await CloneRequestAsync(request);
        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authProvider.AccessToken);

        _logger.LogDebug("Retrying request after token refresh");
        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private static bool IsRefreshEndpoint(Uri? uri)
    {
        return uri?.AbsolutePath.Contains("/api/auth/refresh", StringComparison.OrdinalIgnoreCase) == true;
    }

    private async Task<bool> TryRefreshTokenAsync(CustomAuthenticationStateProvider authProvider, CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (string.IsNullOrEmpty(authProvider.RefreshToken))
            {
                return false;
            }

            _logger.LogInformation("Attempting to refresh access token");

            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { refreshToken = authProvider.RefreshToken }, JsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };

            var response = await base.SendAsync(refreshRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token refresh failed with status {StatusCode}", response.StatusCode);
                authProvider.ClearTokens();
                await authProvider.LogoutAsync();
                return false;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<TokenRefreshResponse>(content, JsonOptions);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogWarning("Token refresh returned invalid response");
                authProvider.ClearTokens();
                return false;
            }

            await authProvider.UpdateTokensAsync(tokenResponse.AccessToken, tokenResponse.RefreshToken);
            _logger.LogInformation("Token refreshed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during token refresh");
            authProvider.ClearTokens();
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        return clone;
    }

    private sealed class TokenRefreshResponse
    {
        public string AccessToken { get; set; } = "";
        public string? RefreshToken { get; set; }
    }
}
