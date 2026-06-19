using KrogerMcp.Domain.Common;

namespace KrogerMcp.Domain.Products;

public sealed record Product(
    string ProductId,
    string? Upc,
    string? Brand,
    string? Description,
    string? ProductPageUri,
    IReadOnlyList<string> Categories,
    IReadOnlyList<AisleLocation> AisleLocations,
    IReadOnlyList<ProductItem> Items);

public sealed record ProductItem(
    string? ItemId,
    string? Size,
    string? SoldBy,
    ProductPrice? Price,
    InventoryStatus? Inventory,
    IReadOnlyList<FulfillmentType> Fulfillment);

public sealed record ProductPrice(decimal? Regular, decimal? Promo);

public sealed record AisleLocation(string? Description, string? Number, string? ShelfNumber, string? Side);

public enum InventoryStatus
{
    Unknown,
    High,
    Low,
    TemporarilyOutOfStock
}

public enum FulfillmentType
{
    Ais,
    Csp,
    Dth,
    Sth
}

public sealed record ProductSearchQuery(
    string? Term,
    LocationId LocationId,
    string? Brand,
    FulfillmentType? Fulfillment,
    int Limit,
    int Start);

public sealed record ProductSearchResult(IReadOnlyList<Product> Products);
