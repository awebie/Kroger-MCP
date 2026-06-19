using KrogerMcp.Application.Abstractions;
using KrogerMcp.Application.Contracts;
using KrogerMcp.Application.UseCases;
using KrogerMcp.Application.Validation;
using KrogerMcp.Domain.Common;
using KrogerMcp.Domain.Products;

namespace KrogerMcp.Tests.Application;

public sealed class SearchProductsHandlerTests
{
    [Fact]
    public async Task HandleAsync_returns_products_from_client()
    {
        var product = new Product("0001111041700", "0001111041700", "Kroger", "Milk", null, [], [], []);
        var handler = new SearchProductsHandler(new FakeProductCatalogClient(product), new KrogerInputValidator());

        var result = await handler.HandleAsync(new SearchProductsRequest("milk", "01400943"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Products);
        Assert.Equal("Milk", result.Value.Products[0].Description);
    }

    private sealed class FakeProductCatalogClient(Product product) : IProductCatalogClient
    {
        public Task<Result<ProductSearchResult>> SearchProductsAsync(ProductSearchQuery query, CancellationToken ct)
        {
            return Task.FromResult(Result<ProductSearchResult>.Success(new ProductSearchResult([product])));
        }

        public Task<Result<Product>> GetProductAsync(ProductId productId, LocationId? locationId, CancellationToken ct)
        {
            return Task.FromResult(Result<Product>.Success(product));
        }
    }
}
