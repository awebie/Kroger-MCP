using System.Text.Json;
using KrogerMcp.Infrastructure.Kroger.Mapping;

namespace KrogerMcp.Tests.Infrastructure;

public sealed class LocationMapperTests
{
    [Fact]
    public void ToSearchResult_maps_location_id_and_address()
    {
        using var document = JsonDocument.Parse("""
        {
          "data": [
            {
              "locationId": "01400943",
              "name": "Kroger",
              "chain": "KROGER",
              "address": { "city": "Cincinnati", "state": "OH", "zipCode": "45202" },
              "geolocation": { "latLng": "39.1,-84.5", "latitude": 39.1, "longitude": -84.5 }
            }
          ]
        }
        """);

        var result = LocationMapper.ToSearchResult(document);

        Assert.Single(result.Locations);
        Assert.Equal("01400943", result.Locations[0].LocationId);
        Assert.Equal("Cincinnati", result.Locations[0].Address!.City);
    }
}
