using KrogerMcp.Domain.Common;

namespace KrogerMcp.Domain.Locations;

public sealed record StoreLocation(
    string LocationId,
    string? Name,
    string? Chain,
    string? Phone,
    string? StoreNumber,
    string? DivisionNumber,
    Address? Address,
    GeoLocation? GeoLocation,
    IReadOnlyList<Department> Departments);

public sealed record Address(
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? County,
    string? State,
    string? ZipCode);

public sealed record GeoLocation(string? LatLong, decimal? Latitude, decimal? Longitude);

public sealed record StoreHours(string? Timezone, IReadOnlyDictionary<string, string> DailyHours);

public sealed record Department(string DepartmentId, string? Name, string? Phone);

public sealed record Chain(string Name, string? Domain, string? FriendlyBannerName);

public sealed record LocationSearchQuery(
    string? ZipCode,
    string? LatLong,
    string? Latitude,
    string? Longitude,
    LocationId? LocationId,
    int RadiusInMiles,
    int Limit,
    string? Chain,
    string? DepartmentId);

public sealed record LocationSearchResult(IReadOnlyList<StoreLocation> Locations);
