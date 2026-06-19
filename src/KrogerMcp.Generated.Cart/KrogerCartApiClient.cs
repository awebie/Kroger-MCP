using System.Net;
using KrogerMcp.Generated.Cart.Kiota;
using KrogerMcp.Generated.Cart.Kiota.Models.Cart;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace KrogerMcp.Generated.Cart;

public sealed class KrogerCartApiClient(HttpClient httpClient)
{
    public async Task<GeneratedApiResponse> AddToCartAsync(CartAddRequest request, string accessToken, CancellationToken ct)
    {
        var client = CreateClient(accessToken);
        try
        {
            await client.V1.Cart.Add.PutAsync(new CartItemRequestModel
            {
                Items = request.Items.Select(item => new CartItemModel
                {
                    Quantity = item.Quantity,
                    Upc = item.Upc,
                    Modality = item.Modality == "DELIVERY" ? CartItemModel_modality.DELIVERY : CartItemModel_modality.PICKUP
                }).ToList()
            }, cancellationToken: ct);

            return GeneratedApiResponse.Ok();
        }
        catch (ApiException ex)
        {
            return GeneratedApiResponse.Failed(ex.ResponseStatusCode, ex.Message);
        }
    }

    private KrogerCartKiotaClient CreateClient(string accessToken)
    {
        var adapter = new HttpClientRequestAdapter(new StaticBearerAuthenticationProvider(accessToken), httpClient: httpClient)
        {
            BaseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://api.kroger.com"
        };
        return new KrogerCartKiotaClient(adapter);
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

public sealed record CartAddRequest(IReadOnlyList<CartAddItemRequest> Items);

public sealed record CartAddItemRequest(int Quantity, string Upc, string Modality);

public sealed record GeneratedApiResponse(HttpStatusCode StatusCode, string? Body)
{
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;

    public static GeneratedApiResponse Ok() => new(HttpStatusCode.NoContent, null);

    public static GeneratedApiResponse Failed(int statusCode, string? body) => new((HttpStatusCode)statusCode, body);
}
