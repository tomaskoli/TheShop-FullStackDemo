using System.Net.Http.Headers;

namespace TheShop.WebApp.Services;

public class AuthorizationDelegatingHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;
    private string? _pendingIdempotencyKey;

    public AuthorizationDelegatingHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void SetIdempotencyKey(string key)
    {
        _pendingIdempotencyKey = key;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var authProvider = _serviceProvider.GetRequiredService<CustomAuthenticationStateProvider>();

        if (!string.IsNullOrEmpty(authProvider.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authProvider.AccessToken);
        }

        if (_pendingIdempotencyKey != null)
        {
            request.Headers.Add("Idempotency-Key", _pendingIdempotencyKey);
            _pendingIdempotencyKey = null;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
