using KrogerMcp.Domain.Common;

namespace KrogerMcp.Infrastructure.Kroger.Auth;

internal interface IKrogerClientCredentialsTokenProvider
{
    Task<Result<string>> GetTokenAsync(string? scope, CancellationToken ct);
}
