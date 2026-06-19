using KrogerMcp.Host.McpTools;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace KrogerMcp.Host.DependencyInjection;

public static class HostServiceCollectionExtensions
{
    public static IServiceCollection AddKrogerMcpHost(this IServiceCollection services)
    {
        services.AddTransient<ProductTools>();
        services.AddTransient<LocationTools>();
        services.AddTransient<CartTools>();

        services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        return services;
    }
}
