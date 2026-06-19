using KrogerMcp.Application.DependencyInjection;
using KrogerMcp.Host.DependencyInjection;
using KrogerMcp.Infrastructure.Kroger.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
    .AddKrogerApplication()
    .AddKrogerInfrastructure(builder.Configuration)
    .AddKrogerMcpHost();

await builder.Build().RunAsync();
