using System.Net.Http.Headers;

namespace TheShop.WebApp.Services;

public partial class TheShopApiClient
{
    private IServiceProvider? _serviceProvider;
    private ILogger<TheShopApiClient>? _logger;

    partial void Initialize()
    {
        _instanceSettings = new Newtonsoft.Json.JsonSerializerSettings();
        UpdateJsonSerializerSettings(_instanceSettings);

        if (_httpClient.BaseAddress != null)
        {
            BaseUrl = _httpClient.BaseAddress.ToString();
        }
    }

    public void Configure(IServiceProvider serviceProvider, ILogger<TheShopApiClient> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private void ApplyAuthorizationHeader(System.Net.Http.HttpRequestMessage request)
    {
        var authProvider = _serviceProvider?.GetService<CustomAuthenticationStateProvider>();
        if (!string.IsNullOrEmpty(authProvider?.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authProvider.AccessToken);
        }
    }

    partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
    {
        ApplyAuthorizationHeader(request);
    }

    partial void PrepareRequest(
        System.Net.Http.HttpClient client,
        System.Net.Http.HttpRequestMessage request,
        System.Text.StringBuilder urlBuilder)
    {
        ApplyAuthorizationHeader(request);
    }

    public void SetIdempotencyKey(string key)
    {
        var handler = _serviceProvider?.GetService<AuthorizationDelegatingHandler>();
        handler?.SetIdempotencyKey(key);
    }

    public async Task LogoutAndRevokeAsync()
    {
        var authProvider = _serviceProvider?.GetService<CustomAuthenticationStateProvider>();
        if (authProvider?.AccessToken == null)
        {
            if (authProvider != null)
            {
                await authProvider.LogoutAsync();
            }
            return;
        }

        try
        {
            await LogoutAsync(new LogoutRequest
            {
                LogoutAllDevices = false
            });
            _logger?.LogInformation("Logout successful, token revoked");
        }
        catch (ApiException ex)
        {
            _logger?.LogWarning(ex, "Logout API call failed with status {StatusCode}", ex.StatusCode);
        }
        finally
        {
            if (authProvider != null)
            {
                await authProvider.LogoutAsync();
            }
        }
    }

    static partial void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
    {
        settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    }
}
