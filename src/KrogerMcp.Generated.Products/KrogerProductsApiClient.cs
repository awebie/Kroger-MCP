using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using KrogerMcp.Generated.Products.Kiota;
using KrogerMcp.Generated.Products.Kiota.V1.Products;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace KrogerMcp.Generated.Products;

public sealed class KrogerProductsApiClient(HttpClient httpClient)
{
    public async Task<GeneratedApiResponse> SearchProductsAsync(ProductSearchRequest request, string accessToken, CancellationToken ct)
    {
        var client = CreateClient(accessToken);
        try
        {
            var payload = await client.V1.Products.GetAsync(config =>
            {
                config.QueryParameters.FilterTerm = request.Term;
                config.QueryParameters.FilterLocationId = request.LocationId;
                config.QueryParameters.FilterBrand = request.Brand;
                config.QueryParameters.FilterFulfillmentAsGetFilterFulfillmentQueryParameterType = MapFulfillment(request.Fulfillment);
                config.QueryParameters.FilterStart = request.Start;
                config.QueryParameters.FilterLimit = request.Limit;
            }, ct);

            return GeneratedApiResponse.Ok(ToJsonDocument(payload));
        }
        catch (ApiException ex)
        {
            return GeneratedApiResponse.Failed(ex.ResponseStatusCode, ex.Message);
        }
    }

    public async Task<GeneratedApiResponse> GetProductAsync(string productId, string? locationId, string accessToken, CancellationToken ct)
    {
        var client = CreateClient(accessToken);
        try
        {
            var payload = await client.V1.Products[productId].GetAsync(config =>
            {
                config.QueryParameters.FilterLocationId = locationId;
            }, ct);

            return GeneratedApiResponse.Ok(ToJsonDocument(payload));
        }
        catch (ApiException ex)
        {
            return GeneratedApiResponse.Failed(ex.ResponseStatusCode, ex.Message);
        }
    }

    private KrogerProductsKiotaClient CreateClient(string accessToken)
    {
        var adapter = new HttpClientRequestAdapter(new StaticBearerAuthenticationProvider(accessToken), httpClient: httpClient)
        {
            BaseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://api.kroger.com"
        };
        return new KrogerProductsKiotaClient(adapter);
    }

    private static JsonDocument? ToJsonDocument<T>(T? value)
    {
        return value is null ? null : JsonSerializer.SerializeToDocument(value, JsonOptions);
    }

    private static GetFilterFulfillmentQueryParameterType? MapFulfillment(string? fulfillment)
    {
        return fulfillment switch
        {
            "ais" => GetFilterFulfillmentQueryParameterType.Ais,
            "csp" => GetFilterFulfillmentQueryParameterType.Csp,
            "dth" => GetFilterFulfillmentQueryParameterType.Dth,
            "sth" => GetFilterFulfillmentQueryParameterType.Sth,
            _ => null
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

internal sealed class StaticBearerAuthenticationProvider(string accessToken) : IAuthenticationProvider
{
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        request.Headers.TryAdd("Authorization", $"Bearer {accessToken}");
        return Task.CompletedTask;
    }
}

public sealed record ProductSearchRequest(string? Term, string LocationId, string? Brand, string? Fulfillment, int Start, int Limit);

public sealed record GeneratedApiResponse(HttpStatusCode StatusCode, JsonDocument? Json, string? Body)
{
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;

    public static GeneratedApiResponse Ok(JsonDocument? json) => new(HttpStatusCode.OK, json, null);

    public static GeneratedApiResponse Failed(int statusCode, string? body) => new((HttpStatusCode)statusCode, null, body);
}
