using System.Net;
using System.Text.Json;
using KrogerMcp.Generated.Locations.Kiota;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace KrogerMcp.Generated.Locations;

public sealed class KrogerLocationsApiClient(HttpClient httpClient)
{
    public async Task<GeneratedApiResponse> SearchLocationsAsync(LocationSearchRequest request, string accessToken, CancellationToken ct)
    {
        var client = CreateClient(accessToken);
        try
        {
            var payload = await client.V1.Locations.GetAsync(config =>
            {
                config.QueryParameters.FilterZipCodeNear = request.ZipCode;
                config.QueryParameters.FilterLatLongNear = request.LatLong;
                config.QueryParameters.FilterLatNear = request.Latitude;
                config.QueryParameters.FilterLonNear = request.Longitude;
                config.QueryParameters.FilterLocationId = request.LocationId;
                config.QueryParameters.FilterRadiusInMiles = request.RadiusInMiles;
                config.QueryParameters.FilterLimit = request.Limit;
                config.QueryParameters.FilterChain = request.Chain;
                config.QueryParameters.FilterDepartment = request.DepartmentId;
            }, ct);

            return GeneratedApiResponse.Ok(ToJsonDocument(payload));
        }
        catch (ApiException ex)
        {
            return GeneratedApiResponse.Failed(ex.ResponseStatusCode, ex.Message);
        }
    }

    public async Task<GeneratedApiResponse> GetLocationAsync(string locationId, string accessToken, CancellationToken ct)
    {
        var client = CreateClient(accessToken);
        try
        {
            var payload = await client.V1.Locations[locationId].GetAsync(cancellationToken: ct);
            return GeneratedApiResponse.Ok(ToJsonDocument(payload));
        }
        catch (ApiException ex)
        {
            return GeneratedApiResponse.Failed(ex.ResponseStatusCode, ex.Message);
        }
    }

    private KrogerLocationsKiotaClient CreateClient(string accessToken)
    {
        var adapter = new HttpClientRequestAdapter(new StaticBearerAuthenticationProvider(accessToken), httpClient: httpClient)
        {
            BaseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://api.kroger.com"
        };
        return new KrogerLocationsKiotaClient(adapter);
    }

    private static JsonDocument? ToJsonDocument<T>(T? value)
    {
        return value is null ? null : JsonSerializer.SerializeToDocument(value, JsonOptions);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}

internal sealed class StaticBearerAuthenticationProvider(string accessToken) : IAuthenticationProvider
{
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        request.Headers.TryAdd("Authorization", $"Bearer {accessToken}");
        return Task.CompletedTask;
    }
}

public sealed record LocationSearchRequest(
    string? ZipCode,
    string? LatLong,
    string? Latitude,
    string? Longitude,
    string? LocationId,
    int RadiusInMiles,
    int Limit,
    string? Chain,
    string? DepartmentId);

public sealed record GeneratedApiResponse(HttpStatusCode StatusCode, JsonDocument? Json, string? Body)
{
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;

    public static GeneratedApiResponse Ok(JsonDocument? json) => new(HttpStatusCode.OK, json, null);

    public static GeneratedApiResponse Failed(int statusCode, string? body) => new((HttpStatusCode)statusCode, null, body);
}
