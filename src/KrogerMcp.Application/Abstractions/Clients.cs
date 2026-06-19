using KrogerMcp.Domain.Cart;
using KrogerMcp.Domain.Common;
using KrogerMcp.Domain.Locations;
using KrogerMcp.Domain.Products;

namespace KrogerMcp.Application.Abstractions;

public interface IProductCatalogClient
{
    Task<Result<ProductSearchResult>> SearchProductsAsync(ProductSearchQuery query, CancellationToken ct);

    Task<Result<Product>> GetProductAsync(ProductId productId, LocationId? locationId, CancellationToken ct);
}

public interface ILocationClient
{
    Task<Result<LocationSearchResult>> SearchLocationsAsync(LocationSearchQuery query, CancellationToken ct);

    Task<Result<StoreLocation>> GetLocationAsync(LocationId locationId, CancellationToken ct);
}

public interface ICartClient
{
    Task<Result<CartAddResult>> AddToCartAsync(AddToCartCommand command, CustomerAccessToken token, CancellationToken ct);
}

public interface ICustomerAuthorizationService
{
    Task<Result<CustomerAccessToken>> GetCustomerCartTokenAsync(CancellationToken ct);
}
