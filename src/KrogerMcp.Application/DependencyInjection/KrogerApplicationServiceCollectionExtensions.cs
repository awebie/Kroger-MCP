using KrogerMcp.Application.UseCases;
using KrogerMcp.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace KrogerMcp.Application.DependencyInjection;

public static class KrogerApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddKrogerApplication(this IServiceCollection services)
    {
        services.AddSingleton<KrogerInputValidator>();
        services.AddTransient<SearchProductsHandler>();
        services.AddTransient<GetProductHandler>();
        services.AddTransient<LookupLocationsHandler>();
        services.AddTransient<GetLocationHandler>();
        services.AddTransient<AddToCartHandler>();
        return services;
    }
}
