using System.Net.Http.Headers;
using System.Text.Json;
using KrogerMcp.Domain.Common;
using KrogerMcp.Infrastructure.Kroger.Options;
using Microsoft.Extensions.Options;

namespace KrogerMcp.Infrastructure.Kroger.Auth;

public interface IKrogerAccessTokenProvider
{
    Task<Result<string>> GetClientAccessTokenAsync(string? scope, CancellationToken ct);
}

public sealed class KrogerClientCredentialsTokenProvider(HttpClient httpClient, IOptions<KrogerApiOptions> apiOptions, IOptions<KrogerOAuthOptions> oauthOptions)
    : IKrogerAccessTokenProvider
{
    private readonly Dictionary<string, CachedToken> _cache = new(StringComparer.Ordinal);

    public async Task<Result<string>> GetClientAccessTokenAsync(string? scope, CancellationToken ct)
    {
        var cacheKey = scope ?? string.Empty;
        if (_cache.TryGetValue(cacheKey, out var cached) && cached.ExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return Result<string>.Success(cached.AccessToken);
        }

        var clientId = First(Environment.GetEnvironmentVariable("KROGER_CLIENT_ID"), oauthOptions.Value.ClientId);
        var clientSecret = First(Environment.GetEnvironmentVariable("KROGER_CLIENT_SECRET"), oauthOptions.Value.ClientSecret);
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return Result<string>.Failure(new KrogerError(ErrorCategory.Configuration, "KROGER_CLIENT_ID and KROGER_CLIENT_SECRET are required."));
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, apiOptions.Value.TokenUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));
        var form = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials")
        };
        if (!string.IsNullOrWhiteSpace(scope))
        {
            form.Add(new KeyValuePair<string, string>("scope", scope));
        }

        request.Content = new FormUrlEncodedContent(form);
        using var response = await httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            return Result<string>.Failure(new KrogerError(ErrorCategory.Unauthorized, "Unable to obtain Kroger client credentials token.", (int)response.StatusCode));
        }

        using var json = JsonDocument.Parse(body);
        var accessToken = json.RootElement.GetProperty("access_token").GetString();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result<string>.Failure(new KrogerError(ErrorCategory.UnexpectedUpstreamResponse, "Kroger token response did not include access_token."));
        }

        var expiresIn = json.RootElement.TryGetProperty("expires_in", out var expiresElement) && expiresElement.TryGetInt32(out var seconds)
            ? seconds
            : 1800;
        _cache[cacheKey] = new CachedToken(accessToken, DateTimeOffset.UtcNow.AddSeconds(expiresIn));
        return Result<string>.Success(accessToken);
    }

    private static string? First(params string?[] values) => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private sealed record CachedToken(string AccessToken, DateTimeOffset ExpiresAtUtc);
}
