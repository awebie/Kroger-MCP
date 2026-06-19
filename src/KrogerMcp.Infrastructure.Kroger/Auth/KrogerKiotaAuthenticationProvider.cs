using KrogerMcp.Domain.Common;

namespace KrogerMcp.Infrastructure.Kroger.Auth;

public sealed class KrogerKiotaAuthenticationProvider(IKrogerAccessTokenProvider tokenProvider)
{
    public Task<Result<string>> GetProductTokenAsync(CancellationToken ct) => tokenProvider.GetClientAccessTokenAsync("product.compact", ct);

    public Task<Result<string>> GetLocationTokenAsync(CancellationToken ct) => tokenProvider.GetClientAccessTokenAsync(null, ct);
}
