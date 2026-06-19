using System.Text.Json;
using KrogerMcp.Domain.Locations;

namespace KrogerMcp.Infrastructure.Kroger.Mapping;

public static class LocationMapper
{
    public static LocationSearchResult ToSearchResult(JsonDocument? document)
    {
        if (document is null || !document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return new LocationSearchResult([]);
        }

        return new LocationSearchResult(data.EnumerateArray().Select(ToLocation).ToArray());
    }

    public static StoreLocation? ToLocationPayload(JsonDocument? document)
    {
        if (document is null || !document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return ToLocation(data);
    }

    private static StoreLocation ToLocation(JsonElement element)
    {
        var address = element.GetObjectOrNull("address");
        var geo = element.GetObjectOrNull("geolocation");
        var departments = element.GetArrayOrEmpty("departments")
            .Select(department => new Department(
                department.GetStringOrNull("departmentId") ?? string.Empty,
                department.GetStringOrNull("name"),
                department.GetStringOrNull("phone")))
            .Where(department => !string.IsNullOrWhiteSpace(department.DepartmentId))
            .ToArray();

        return new StoreLocation(
            element.GetStringOrNull("locationId") ?? string.Empty,
            element.GetStringOrNull("name"),
            element.GetStringOrNull("chain"),
            element.GetStringOrNull("phone"),
            element.GetStringOrNull("storeNumber"),
            element.GetStringOrNull("divisionNumber"),
            address is null ? null : new Address(
                address.Value.GetStringOrNull("addressLine1"),
                address.Value.GetStringOrNull("addressLine2"),
                address.Value.GetStringOrNull("city"),
                address.Value.GetStringOrNull("county"),
                address.Value.GetStringOrNull("state"),
                address.Value.GetStringOrNull("zipCode")),
            geo is null ? null : new GeoLocation(
                geo.Value.GetStringOrNull("latLng"),
                geo.Value.GetDecimalOrNull("latitude"),
                geo.Value.GetDecimalOrNull("longitude")),
            departments);
    }
}
