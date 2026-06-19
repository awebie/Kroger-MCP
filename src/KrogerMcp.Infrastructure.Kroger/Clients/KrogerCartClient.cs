using KrogerMcp.Application.Abstractions;
using KrogerMcp.Domain.Cart;
using KrogerMcp.Domain.Common;
using KrogerMcp.Generated.Cart;
using KrogerMcp.Infrastructure.Kroger.Mapping;

namespace KrogerMcp.Infrastructure.Kroger.Clients;

public sealed class KrogerCartClient(KrogerCartApiClient apiClient) : ICartClient
{
    public async Task<Result<CartAddResult>> AddToCartAsync(AddToCartCommand command, CustomerAccessToken token, CancellationToken ct)
    {
        var response = await apiClient.AddToCartAsync(CartMapper.ToGeneratedRequest(command), token.Value, ct);
        return response.IsSuccess
            ? Result<CartAddResult>.Success(CartAddResult.Added(command.Items.Count))
            : Result<CartAddResult>.Failure(KrogerErrorMapper.FromStatus(response.StatusCode, response.Body));
    }
}
