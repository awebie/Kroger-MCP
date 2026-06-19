# Kroger MCP Server Detailed Spec

## Summary

Build a greenfield .NET 10 local stdio MCP server for Kroger product search, product details, store/location lookup, and cart additions. Use Onion Architecture, dependency injection, generated-client isolation, and stable MCP root actions that do not expose generated types.

## Public MCP Tool Contracts

- `search_products(SearchProductsRequest request) -> SearchProductsResponse`
- `get_product(GetProductRequest request) -> ProductDetailsResponse`
- `lookup_locations(LookupLocationsRequest request) -> LookupLocationsResponse`
- `get_location(GetLocationRequest request) -> LocationDetailsResponse`
- `add_to_cart(AddToCartRequest request) -> AddToCartResponse`

## Solution Structure

- `src/KrogerMcp.Host`: stdio MCP entrypoint, DI, and tool adapters.
- `src/KrogerMcp.Application`: use cases, stable contracts, validation, and application interfaces.
- `src/KrogerMcp.Domain`: domain records, value objects, result/error model.
- `src/KrogerMcp.Infrastructure.Kroger`: Kroger auth, generated-client wrappers, mapping, options.
- `src/KrogerMcp.Generated.*`: generated-client boundary projects for Products, Cart, and Locations.
- `tests/KrogerMcp.Tests`: validation, handler, mapping, and host smoke tests.

## Application Interfaces

```csharp
public interface IProductCatalogClient {
    Task<Result<ProductSearchResult>> SearchProductsAsync(ProductSearchQuery query, CancellationToken ct);
    Task<Result<Product>> GetProductAsync(ProductId productId, LocationId? locationId, CancellationToken ct);
}

public interface ILocationClient {
    Task<Result<LocationSearchResult>> SearchLocationsAsync(LocationSearchQuery query, CancellationToken ct);
    Task<Result<StoreLocation>> GetLocationAsync(LocationId locationId, CancellationToken ct);
}

public interface ICartClient {
    Task<Result<CartAddResult>> AddToCartAsync(AddToCartCommand command, CustomerAccessToken token, CancellationToken ct);
}

public interface ICustomerAuthorizationService {
    Task<Result<CustomerAccessToken>> GetCustomerCartTokenAsync(CancellationToken ct);
}
```

## Mermaid Architecture

```mermaid
classDiagram
    class ProductTools
    class LocationTools
    class CartTools
    class SearchProductsHandler
    class LookupLocationsHandler
    class AddToCartHandler
    class IProductCatalogClient
    class ILocationClient
    class ICartClient
    class ICustomerAuthorizationService
    class KrogerProductCatalogClient
    class KrogerLocationClient
    class KrogerCartClient
    class KrogerProductsApiClient
    class KrogerLocationsApiClient
    class KrogerCartApiClient

    ProductTools --> SearchProductsHandler
    LocationTools --> LookupLocationsHandler
    CartTools --> AddToCartHandler
    SearchProductsHandler --> IProductCatalogClient
    LookupLocationsHandler --> ILocationClient
    AddToCartHandler --> ICartClient
    AddToCartHandler --> ICustomerAuthorizationService
    IProductCatalogClient <|.. KrogerProductCatalogClient
    ILocationClient <|.. KrogerLocationClient
    ICartClient <|.. KrogerCartClient
    KrogerProductCatalogClient --> KrogerProductsApiClient
    KrogerLocationClient --> KrogerLocationsApiClient
    KrogerCartClient --> KrogerCartApiClient
```

## Mermaid Product Search Flow

```mermaid
sequenceDiagram
    participant Host as MCP Host
    participant Tool as ProductTools
    participant Handler as SearchProductsHandler
    participant Client as IProductCatalogClient
    participant Generated as KrogerProductsApiClient
    participant Kroger as Kroger Products API

    Host->>Tool: search_products(request)
    Tool->>Handler: HandleAsync(request)
    Handler->>Handler: Validate term, locationId, limit, start
    Handler->>Client: SearchProductsAsync(query)
    Client->>Generated: GET /v1/products with filters
    Generated->>Kroger: Authorized request
    Kroger-->>Generated: products.productsPayloadModel
    Generated-->>Client: JSON payload
    Client-->>Handler: Result<ProductSearchResult>
    Handler-->>Tool: SearchProductsResponse
    Tool-->>Host: MCP tool result
```

## Mermaid Cart Auth Flow

```mermaid
sequenceDiagram
    participant Host as MCP Host
    participant Tool as CartTools
    participant Handler as AddToCartHandler
    participant Auth as ICustomerAuthorizationService
    participant Cart as ICartClient
    participant Kroger as Kroger Cart API

    Host->>Tool: add_to_cart(request)
    Tool->>Handler: HandleAsync(request)
    Handler->>Handler: Validate items and modality
    Handler->>Auth: GetCustomerCartTokenAsync()
    Auth-->>Handler: CustomerAccessToken
    Handler->>Cart: AddToCartAsync(command, token)
    Cart->>Kroger: PUT /v1/cart/add
    Kroger-->>Cart: 204 No Content
    Cart-->>Handler: CartAddResult.Success
    Handler-->>Host: AddToCartResponse
```

## Validation Rules

- `LocationId`: exactly 8 characters.
- `DepartmentId`: exactly 2 characters.
- Product search `Term`: minimum 3 characters when present.
- Product search `Limit`: 1 to 50.
- Product search `Start`: 1 to 250.
- Location `Limit`: 1 to 200.
- Location `RadiusInMiles`: 1 to 100.
- Product search must include at least one initial search value: `Term` or `Brand` in MCP v1.
- Location search must use exactly one search origin: zip, latLong, lat/lon, or locationId.
- `AddToCartRequest.Items`: non-empty; each quantity must be greater than zero.

## Configuration

```json
{
  "Kroger": {
    "BaseUrl": "https://api.kroger.com",
    "AuthorizationUrl": "https://api.kroger.com/v1/connect/oauth2/authorize",
    "TokenUrl": "https://api.kroger.com/v1/connect/oauth2/token",
    "DefaultLocationId": null,
    "OAuthRedirectUri": "http://127.0.0.1:53682/callback"
  }
}
```

Environment variables: `KROGER_CLIENT_ID`, `KROGER_CLIENT_SECRET`, optional `KROGER_DEFAULT_LOCATION_ID`, optional `KROGER_CUSTOMER_ACCESS_TOKEN`.

## Generation Contract

- Store canonical specs under `openapi/kroger-cart.openapi.json`, `openapi/kroger-products.openapi.json`, and `openapi/kroger-locations.openapi.json`.
- Treat `openapi (2).json` and `openapi (3).json` as duplicate Products specs; keep one.
- Keep stable boundary names: `KrogerProductsApiClient`, `KrogerCartApiClient`, `KrogerLocationsApiClient`.
- Generated projects must not reference Application or Domain.
- Infrastructure may reference Generated projects; Application and Domain may not.
