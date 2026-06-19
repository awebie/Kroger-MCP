using System.ComponentModel;
using KrogerMcp.Application.Contracts;
using KrogerMcp.Application.UseCases;
using ModelContextProtocol.Server;

namespace KrogerMcp.Host.McpTools;

[McpServerToolType]
public sealed class ProductTools(SearchProductsHandler searchProductsHandler, GetProductHandler getProductHandler)
{
    [McpServerTool(Name = "search_products")]
    [Description("Search Kroger products filtered to a location.")]
    public async Task<SearchProductsResponse> SearchProducts(SearchProductsRequest request, CancellationToken cancellationToken)
    {
        return McpToolResultMapper.Unwrap(await searchProductsHandler.HandleAsync(request, cancellationToken));
    }

    [McpServerTool(Name = "get_product")]
    [Description("Get Kroger product details by product id, optionally scoped to a location.")]
    public async Task<ProductDetailsResponse> GetProduct(GetProductRequest request, CancellationToken cancellationToken)
    {
        return McpToolResultMapper.Unwrap(await getProductHandler.HandleAsync(request, cancellationToken));
    }
}
