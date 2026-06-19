using KrogerMcp.Application.Abstractions;
using KrogerMcp.Domain.Common;
using KrogerMcp.Domain.Locations;
using KrogerMcp.Generated.Locations;
using KrogerMcp.Infrastructure.Kroger.Auth;
using KrogerMcp.Infrastructure.Kroger.Mapping;

namespace KrogerMcp.Infrastructure.Kroger.Clients;

public sealed class KrogerLocationClient(KrogerLocationsApiClient apiClient, KrogerKiotaAuthenticationProvider auth)
    : ILocationClient
{
    public async Task<Result<LocationSearchResult>> SearchLocationsAsync(LocationSearchQuery query, CancellationToken ct)
    {
        var token = await auth.GetLocationTokenAsync(ct);
        if (!token.IsSuccess)
        {
            return Result<LocationSearchResult>.Failure(token.Error!);
        }

        var response = await apiClient.SearchLocationsAsync(new LocationSearchRequest(
            query.ZipCode,
            query.LatLong,
            query.Latitude,
            query.Longitude,
            query.LocationId?.Value,
            query.RadiusInMiles,
            query.Limit,
            query.Chain,
            query.DepartmentId), token.Value!, ct);

        return response.IsSuccess
            ? Result<LocationSearchResult>.Success(LocationMapper.ToSearchResult(response.Json))
            : Result<LocationSearchResult>.Failure(KrogerErrorMapper.FromStatus(response.StatusCode, response.Body));
    }

    public async Task<Result<StoreLocation>> GetLocationAsync(LocationId locationId, CancellationToken ct)
    {
        var token = await auth.GetLocationTokenAsync(ct);
        if (!token.IsSuccess)
        {
            return Result<StoreLocation>.Failure(token.Error!);
        }

        var response = await apiClient.GetLocationAsync(locationId.Value, token.Value!, ct);
        if (!response.IsSuccess)
        {
            return Result<StoreLocation>.Failure(KrogerErrorMapper.FromStatus(response.StatusCode, response.Body));
        }

        var location = LocationMapper.ToLocationPayload(response.Json);
        return location is null
            ? Result<StoreLocation>.Failure(new KrogerError(ErrorCategory.UnexpectedUpstreamResponse, "Kroger location response did not include data."))
            : Result<StoreLocation>.Success(location);
    }
}
