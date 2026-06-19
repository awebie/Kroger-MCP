using KrogerMcp.Application.Abstractions;
using KrogerMcp.Generated.Cart;
using KrogerMcp.Generated.Locations;
using KrogerMcp.Generated.Products;
using KrogerMcp.Infrastructure.Kroger.Auth;
using KrogerMcp.Infrastructure.Kroger.Clients;
using KrogerMcp.Infrastructure.Kroger.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KrogerMcp.Infrastructure.Kroger.DependencyInjection;

public static class KrogerInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddKrogerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KrogerApiOptions>(configuration.GetSection(KrogerApiOptions.SectionName));
        services.Configure<KrogerOAuthOptions>(configuration.GetSection(KrogerOAuthOptions.SectionName));

        services.AddHttpClient<IKrogerAccessTokenProvider, KrogerClientCredentialsTokenProvider>();
        services.AddSingleton<KrogerKiotaAuthenticationProvider>();
        services.AddHttpClient<ICustomerAuthorizationService, KrogerCustomerAuthorizationService>();

        services.AddHttpClient<KrogerProductsApiClient>((sp, httpClient) => ConfigureBaseAddress(sp, httpClient));
        services.AddHttpClient<KrogerLocationsApiClient>((sp, httpClient) => ConfigureBaseAddress(sp, httpClient));
        services.AddHttpClient<KrogerCartApiClient>((sp, httpClient) => ConfigureBaseAddress(sp, httpClient));

        services.AddTransient<IProductCatalogClient, KrogerProductCatalogClient>();
        services.AddTransient<ILocationClient, KrogerLocationClient>();
        services.AddTransient<ICartClient, KrogerCartClient>();
        return services;
    }

    private static void ConfigureBaseAddress(IServiceProvider sp, HttpClient httpClient)
    {
        var options = sp.GetRequiredService<IOptions<KrogerApiOptions>>().Value;
        httpClient.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
    }
}
