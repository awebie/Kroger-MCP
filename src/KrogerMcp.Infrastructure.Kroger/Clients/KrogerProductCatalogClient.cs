using KrogerMcp.Application.Abstractions;
using KrogerMcp.Domain.Common;
using KrogerMcp.Domain.Products;
using KrogerMcp.Generated.Products;
using KrogerMcp.Infrastructure.Kroger.Auth;
using KrogerMcp.Infrastructure.Kroger.Mapping;

namespace KrogerMcp.Infrastructure.Kroger.Clients;

public sealed class KrogerProductCatalogClient(KrogerProductsApiClient apiClient, KrogerKiotaAuthenticationProvider auth)
    : IProductCatalogClient
{
    public async Task<Result<ProductSearchResult>> SearchProductsAsync(ProductSearchQuery query, CancellationToken ct)
    {
        var token = await auth.GetProductTokenAsync(ct);
        if (!token.IsSuccess)
        {
            return Result<ProductSearchResult>.Failure(token.Error!);
        }

        var response = await apiClient.SearchProductsAsync(new ProductSearchRequest(
            query.Term,
            query.LocationId.Value,
            query.Brand,
            query.Fulfillment?.ToString().ToLowerInvariant(),
            query.Start,
            query.Limit), token.Value!, ct);

        return response.IsSuccess
            ? Result<ProductSearchResult>.Success(ProductMapper.ToSearchResult(response.Json))
            : Result<ProductSearchResult>.Failure(KrogerErrorMapper.FromStatus(response.StatusCode, response.Body));
    }

    public async Task<Result<Product>> GetProductAsync(ProductId productId, LocationId? locationId, CancellationToken ct)
    {
        var token = await auth.GetProductTokenAsync(ct);
        if (!token.IsSuccess)
        {
            return Result<Product>.Failure(token.Error!);
        }

        var response = await apiClient.GetProductAsync(productId.Value, locationId?.Value, token.Value!, ct);
        if (!response.IsSuccess)
        {
            return Result<Product>.Failure(KrogerErrorMapper.FromStatus(response.StatusCode, response.Body));
        }

        var product = ProductMapper.ToProductPayload(response.Json);
        return product is null
            ? Result<Product>.Failure(new KrogerError(ErrorCategory.UnexpectedUpstreamResponse, "Kroger product response did not include data."))
            : Result<Product>.Success(product);
    }
}
