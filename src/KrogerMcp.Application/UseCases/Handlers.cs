using KrogerMcp.Application.Abstractions;
using KrogerMcp.Application.Contracts;
using KrogerMcp.Application.Validation;
using KrogerMcp.Domain.Cart;
using KrogerMcp.Domain.Common;
using KrogerMcp.Domain.Locations;
using KrogerMcp.Domain.Products;

namespace KrogerMcp.Application.UseCases;

public sealed class SearchProductsHandler(IProductCatalogClient client, KrogerInputValidator validator)
{
    public async Task<Result<SearchProductsResponse>> HandleAsync(SearchProductsRequest request, CancellationToken ct)
    {
        var validationError = validator.Validate(request);
        if (validationError is not null)
        {
            return Result<SearchProductsResponse>.Failure(validationError);
        }

        var query = new ProductSearchQuery(request.Term, new LocationId(request.LocationId), request.Brand, request.Fulfillment, request.Limit, request.Start);
        var result = await client.SearchProductsAsync(query, ct);
        return result.IsSuccess
            ? Result<SearchProductsResponse>.Success(new SearchProductsResponse(result.Value!.Products))
            : Result<SearchProductsResponse>.Failure(result.Error!);
    }
}

public sealed class GetProductHandler(IProductCatalogClient client, KrogerInputValidator validator)
{
    public async Task<Result<ProductDetailsResponse>> HandleAsync(GetProductRequest request, CancellationToken ct)
    {
        var validationError = validator.Validate(request);
        if (validationError is not null)
        {
            return Result<ProductDetailsResponse>.Failure(validationError);
        }

        LocationId? locationId = string.IsNullOrWhiteSpace(request.LocationId) ? null : new LocationId(request.LocationId);
        var result = await client.GetProductAsync(new ProductId(request.ProductId), locationId, ct);
        return result.IsSuccess
            ? Result<ProductDetailsResponse>.Success(new ProductDetailsResponse(result.Value!))
            : Result<ProductDetailsResponse>.Failure(result.Error!);
    }
}

public sealed class LookupLocationsHandler(ILocationClient client, KrogerInputValidator validator)
{
    public async Task<Result<LookupLocationsResponse>> HandleAsync(LookupLocationsRequest request, CancellationToken ct)
    {
        var validationError = validator.Validate(request);
        if (validationError is not null)
        {
            return Result<LookupLocationsResponse>.Failure(validationError);
        }

        LocationId? locationId = string.IsNullOrWhiteSpace(request.LocationId) ? null : new LocationId(request.LocationId);
        var query = new LocationSearchQuery(
            request.ZipCode,
            request.LatLong,
            request.Latitude,
            request.Longitude,
            locationId,
            request.RadiusInMiles,
            request.Limit,
            request.Chain,
            request.DepartmentId);
        var result = await client.SearchLocationsAsync(query, ct);
        return result.IsSuccess
            ? Result<LookupLocationsResponse>.Success(new LookupLocationsResponse(result.Value!.Locations))
            : Result<LookupLocationsResponse>.Failure(result.Error!);
    }
}

public sealed class GetLocationHandler(ILocationClient client, KrogerInputValidator validator)
{
    public async Task<Result<LocationDetailsResponse>> HandleAsync(GetLocationRequest request, CancellationToken ct)
    {
        var validationError = validator.Validate(request);
        if (validationError is not null)
        {
            return Result<LocationDetailsResponse>.Failure(validationError);
        }

        var result = await client.GetLocationAsync(new LocationId(request.LocationId), ct);
        return result.IsSuccess
            ? Result<LocationDetailsResponse>.Success(new LocationDetailsResponse(result.Value!))
            : Result<LocationDetailsResponse>.Failure(result.Error!);
    }
}

public sealed class AddToCartHandler(ICartClient cartClient, ICustomerAuthorizationService authorizationService, KrogerInputValidator validator)
{
    public async Task<Result<AddToCartResponse>> HandleAsync(AddToCartRequest request, CancellationToken ct)
    {
        var validationError = validator.Validate(request);
        if (validationError is not null)
        {
            return Result<AddToCartResponse>.Failure(validationError);
        }

        var token = await authorizationService.GetCustomerCartTokenAsync(ct);
        if (!token.IsSuccess)
        {
            return Result<AddToCartResponse>.Failure(token.Error!);
        }

        var command = new AddToCartCommand(
            request.Items.Select(item => new CartItem(item.UpcOrProductId, item.Quantity)).ToArray(),
            request.Modality);
        var result = await cartClient.AddToCartAsync(command, token.Value!, ct);
        return result.IsSuccess
            ? Result<AddToCartResponse>.Success(new AddToCartResponse(result.Value!.Success, result.Value.Message))
            : Result<AddToCartResponse>.Failure(result.Error!);
    }
}
