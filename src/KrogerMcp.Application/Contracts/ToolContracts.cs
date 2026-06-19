using KrogerMcp.Domain.Cart;
using KrogerMcp.Domain.Locations;
using KrogerMcp.Domain.Products;

namespace KrogerMcp.Application.Contracts;

public sealed record SearchProductsRequest(
    string? Term,
    string LocationId,
    string? Brand = null,
    FulfillmentType? Fulfillment = null,
    int Limit = 10,
    int Start = 1);

public sealed record SearchProductsResponse(IReadOnlyList<Product> Products);

public sealed record GetProductRequest(string ProductId, string? LocationId = null);

public sealed record ProductDetailsResponse(Product Product);

public sealed record LookupLocationsRequest(
    string? ZipCode = null,
    string? LatLong = null,
    string? Latitude = null,
    string? Longitude = null,
    string? LocationId = null,
    int RadiusInMiles = 10,
    int Limit = 10,
    string? Chain = null,
    string? DepartmentId = null);

public sealed record LookupLocationsResponse(IReadOnlyList<StoreLocation> Locations);

public sealed record GetLocationRequest(string LocationId);

public sealed record LocationDetailsResponse(StoreLocation Location);

public sealed record AddToCartRequest(IReadOnlyList<AddToCartItemRequest> Items, CartModality Modality = CartModality.Pickup);

public sealed record AddToCartItemRequest(string UpcOrProductId, int Quantity);

public sealed record AddToCartResponse(bool Success, string Message);
