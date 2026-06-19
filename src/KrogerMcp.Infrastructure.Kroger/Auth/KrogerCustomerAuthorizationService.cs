using KrogerMcp.Application.Abstractions;
using KrogerMcp.Domain.Common;
using KrogerMcp.Infrastructure.Kroger.Options;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KrogerMcp.Infrastructure.Kroger.Auth;

public sealed class KrogerCustomerAuthorizationService(HttpClient httpClient, IOptions<KrogerOAuthOptions> oauthOptions, IOptions<KrogerApiOptions> apiOptions)
    : ICustomerAuthorizationService
{
    public async Task<Result<CustomerAccessToken>> GetCustomerCartTokenAsync(CancellationToken ct)
    {
        var token = First(Environment.GetEnvironmentVariable("KROGER_CUSTOMER_ACCESS_TOKEN"), oauthOptions.Value.CustomerAccessToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            return Result<CustomerAccessToken>.Success(new CustomerAccessToken(token));
        }

        var clientId = First(Environment.GetEnvironmentVariable("KROGER_CLIENT_ID"), oauthOptions.Value.ClientId);
        var clientSecret = First(Environment.GetEnvironmentVariable("KROGER_CLIENT_SECRET"), oauthOptions.Value.ClientSecret);
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return Result<CustomerAccessToken>.Failure(new KrogerError(ErrorCategory.Configuration, "KROGER_CLIENT_ID and KROGER_CLIENT_SECRET are required for cart OAuth."));
        }

        var state = Guid.NewGuid().ToString("N");
        var authorizeUrl = BuildAuthorizeUrl(clientId, state);
        var redirectUri = new Uri(apiOptions.Value.OAuthRedirectUri);
        using var listener = new HttpListener();
        listener.Prefixes.Add($"{redirectUri.Scheme}://{redirectUri.Host}:{redirectUri.Port}/");

        try
        {
            listener.Start();
            OpenBrowser(authorizeUrl);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromMinutes(5));
            var context = await listener.GetContextAsync().WaitAsync(timeoutCts.Token);
            var code = context.Request.QueryString["code"];
            var returnedState = context.Request.QueryString["state"];
            await WriteBrowserResponseAsync(context.Response, string.IsNullOrWhiteSpace(code) ? "Kroger authorization failed." : "Kroger authorization complete. You can return to your MCP client.", timeoutCts.Token);

            if (string.IsNullOrWhiteSpace(code) || !string.Equals(state, returnedState, StringComparison.Ordinal))
            {
                return Result<CustomerAccessToken>.Failure(new KrogerError(ErrorCategory.Unauthorized, "Kroger cart OAuth did not return a valid authorization code."));
            }

            return await ExchangeCodeAsync(code, clientId, clientSecret, ct);
        }
        catch (OperationCanceledException)
        {
            return Result<CustomerAccessToken>.Failure(new KrogerError(ErrorCategory.Unauthorized, "Timed out waiting for Kroger cart OAuth callback."));
        }
        catch (Exception ex)
        {
            return Result<CustomerAccessToken>.Failure(new KrogerError(ErrorCategory.Unauthorized, $"Unable to complete Kroger cart OAuth: {ex.Message}"));
        }
        finally
        {
            if (listener.IsListening)
            {
                listener.Stop();
            }
        }
    }

    private string BuildAuthorizeUrl(string clientId, string state)
    {
        var query = string.Join("&", new[]
        {
            $"response_type=code",
            $"client_id={Uri.EscapeDataString(clientId)}",
            $"redirect_uri={Uri.EscapeDataString(apiOptions.Value.OAuthRedirectUri)}",
            $"scope={Uri.EscapeDataString("cart.basic:write")}",
            $"state={Uri.EscapeDataString(state)}"
        });
        return $"{apiOptions.Value.AuthorizationUrl}?{query}";
    }

    private async Task<Result<CustomerAccessToken>> ExchangeCodeAsync(string code, string clientId, string clientSecret, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, apiOptions.Value.TokenUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = apiOptions.Value.OAuthRedirectUri
        });

        using var response = await httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            return Result<CustomerAccessToken>.Failure(new KrogerError(ErrorCategory.Unauthorized, $"Kroger cart token exchange failed: {body}", (int)response.StatusCode));
        }

        using var json = JsonDocument.Parse(body);
        var accessToken = json.RootElement.TryGetProperty("access_token", out var tokenElement) ? tokenElement.GetString() : null;
        return string.IsNullOrWhiteSpace(accessToken)
            ? Result<CustomerAccessToken>.Failure(new KrogerError(ErrorCategory.UnexpectedUpstreamResponse, "Kroger cart token exchange did not include access_token."))
            : Result<CustomerAccessToken>.Success(new CustomerAccessToken(accessToken));
    }

    private static void OpenBrowser(string authorizeUrl)
    {
        Process.Start(new ProcessStartInfo(authorizeUrl)
        {
            UseShellExecute = true
        });
    }

    private static async Task WriteBrowserResponseAsync(HttpListenerResponse response, string message, CancellationToken ct)
    {
        var buffer = Encoding.UTF8.GetBytes($"<html><body>{WebUtility.HtmlEncode(message)}</body></html>");
        response.ContentType = "text/html";
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, ct);
        response.Close();
    }

    private static string? First(params string?[] values) => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}
