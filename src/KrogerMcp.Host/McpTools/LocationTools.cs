using System.ComponentModel;
using KrogerMcp.Application.Contracts;
using KrogerMcp.Application.UseCases;
using ModelContextProtocol.Server;

namespace KrogerMcp.Host.McpTools;

[McpServerToolType]
public sealed class LocationTools(LookupLocationsHandler lookupLocationsHandler, GetLocationHandler getLocationHandler)
{
    [McpServerTool(Name = "lookup_locations")]
    [Description("Lookup Kroger stores by zip code, coordinates, or location id.")]
    public async Task<LookupLocationsResponse> LookupLocations(LookupLocationsRequest request, CancellationToken cancellationToken)
    {
        return McpToolResultMapper.Unwrap(await lookupLocationsHandler.HandleAsync(request, cancellationToken));
    }

    [McpServerTool(Name = "get_location")]
    [Description("Get Kroger location details by location id.")]
    public async Task<LocationDetailsResponse> GetLocation(GetLocationRequest request, CancellationToken cancellationToken)
    {
        return McpToolResultMapper.Unwrap(await getLocationHandler.HandleAsync(request, cancellationToken));
    }
}
