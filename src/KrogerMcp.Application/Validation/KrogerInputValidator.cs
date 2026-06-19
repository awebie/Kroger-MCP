using KrogerMcp.Application.Contracts;
using KrogerMcp.Domain.Common;

namespace KrogerMcp.Application.Validation;

public sealed class KrogerInputValidator
{
    public KrogerError? Validate(SearchProductsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LocationId) || request.LocationId.Length != 8)
        {
            return KrogerError.Validation("LocationId must be exactly 8 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Term) && string.IsNullOrWhiteSpace(request.Brand))
        {
            return KrogerError.Validation("Product search requires Term or Brand.");
        }

        if (!string.IsNullOrWhiteSpace(request.Term) && request.Term.Trim().Length < 3)
        {
            return KrogerError.Validation("Term must be at least 3 characters when provided.");
        }

        if (request.Limit is < 1 or > 50)
        {
            return KrogerError.Validation("Product search Limit must be between 1 and 50.");
        }

        if (request.Start is < 1 or > 250)
        {
            return KrogerError.Validation("Product search Start must be between 1 and 250.");
        }

        return null;
    }

    public KrogerError? Validate(GetProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductId))
        {
            return KrogerError.Validation("ProductId is required.");
        }

        if (!string.IsNullOrWhiteSpace(request.LocationId) && request.LocationId.Length != 8)
        {
            return KrogerError.Validation("LocationId must be exactly 8 characters when provided.");
        }

        return null;
    }

    public KrogerError? Validate(LookupLocationsRequest request)
    {
        var originCount = CountProvided(request.ZipCode, request.LatLong, request.LocationId)
            + (IsProvided(request.Latitude) || IsProvided(request.Longitude) ? 1 : 0);

        if (originCount != 1)
        {
            return KrogerError.Validation("Location search requires exactly one origin: ZipCode, LatLong, Latitude plus Longitude, or LocationId.");
        }

        if (IsProvided(request.Latitude) != IsProvided(request.Longitude))
        {
            return KrogerError.Validation("Latitude and Longitude must be provided together.");
        }

        if (!string.IsNullOrWhiteSpace(request.LocationId) && request.LocationId.Length != 8)
        {
            return KrogerError.Validation("LocationId must be exactly 8 characters.");
        }

        if (!string.IsNullOrWhiteSpace(request.DepartmentId) && request.DepartmentId.Length != 2)
        {
            return KrogerError.Validation("DepartmentId must be exactly 2 characters.");
        }

        if (request.RadiusInMiles is < 1 or > 100)
        {
            return KrogerError.Validation("RadiusInMiles must be between 1 and 100.");
        }

        if (request.Limit is < 1 or > 200)
        {
            return KrogerError.Validation("Location Limit must be between 1 and 200.");
        }

        return null;
    }

    public KrogerError? Validate(GetLocationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LocationId) || request.LocationId.Length != 8)
        {
            return KrogerError.Validation("LocationId must be exactly 8 characters.");
        }

        return null;
    }

    public KrogerError? Validate(AddToCartRequest request)
    {
        if (request.Items.Count == 0)
        {
            return KrogerError.Validation("At least one cart item is required.");
        }

        foreach (var item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.UpcOrProductId))
            {
                return KrogerError.Validation("Each cart item requires UpcOrProductId.");
            }

            if (item.Quantity < 1)
            {
                return KrogerError.Validation("Each cart item quantity must be greater than zero.");
            }
        }

        return null;
    }

    private static int CountProvided(params string?[] values) => values.Count(IsProvided);

    private static bool IsProvided(string? value) => !string.IsNullOrWhiteSpace(value);
}
