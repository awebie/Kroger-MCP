using System.Text.Json;
using KrogerMcp.Domain.Products;

namespace KrogerMcp.Infrastructure.Kroger.Mapping;

public static class ProductMapper
{
    public static ProductSearchResult ToSearchResult(JsonDocument? document)
    {
        if (document is null || !document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return new ProductSearchResult([]);
        }

        return new ProductSearchResult(data.EnumerateArray().Select(ToProduct).ToArray());
    }

    public static Product? ToProductPayload(JsonDocument? document)
    {
        if (document is null || !document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return ToProduct(data);
    }

    private static Product ToProduct(JsonElement element)
    {
        var items = element.GetArrayOrEmpty("items").Select(ToProductItem).ToArray();
        var aisles = element.GetArrayOrEmpty("aisleLocations")
            .Select(aisle => new AisleLocation(
                aisle.GetStringOrNull("description"),
                aisle.GetStringOrNull("number"),
                aisle.GetStringOrNull("shelfNumber"),
                aisle.GetStringOrNull("side")))
            .ToArray();
        var categories = element.GetArrayOrEmpty("categories")
            .Select(category => category.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();

        return new Product(
            element.GetStringOrNull("productId") ?? element.GetStringOrNull("upc") ?? string.Empty,
            element.GetStringOrNull("upc"),
            element.GetStringOrNull("brand"),
            element.GetStringOrNull("description"),
            element.GetStringOrNull("productPageURI"),
            categories,
            aisles,
            items);
    }

    private static ProductItem ToProductItem(JsonElement item)
    {
        var priceElement = item.GetObjectOrNull("price");
        var inventoryElement = item.GetObjectOrNull("inventory");
        var fulfillmentElement = item.GetObjectOrNull("fulfillment");

        return new ProductItem(
            item.GetStringOrNull("itemId"),
            item.GetStringOrNull("size"),
            item.GetStringOrNull("soldBy"),
            priceElement is null ? null : new ProductPrice(priceElement.Value.GetDecimalOrNull("regular"), priceElement.Value.GetDecimalOrNull("promo")),
            inventoryElement is null ? null : MapInventory(inventoryElement.Value.GetStringOrNull("stockLevel")),
            MapFulfillment(fulfillmentElement));
    }

    private static InventoryStatus MapInventory(string? stockLevel)
    {
        return stockLevel switch
        {
            "HIGH" => InventoryStatus.High,
            "LOW" => InventoryStatus.Low,
            "TEMPORARILY_OUT_OF_STOCK" => InventoryStatus.TemporarilyOutOfStock,
            _ => InventoryStatus.Unknown
        };
    }

    private static IReadOnlyList<FulfillmentType> MapFulfillment(JsonElement? fulfillment)
    {
        if (fulfillment is null)
        {
            return [];
        }

        var values = new List<FulfillmentType>();
        AddIfTrue(fulfillment.Value, "ais", FulfillmentType.Ais, values);
        AddIfTrue(fulfillment.Value, "csp", FulfillmentType.Csp, values);
        AddIfTrue(fulfillment.Value, "dth", FulfillmentType.Dth, values);
        AddIfTrue(fulfillment.Value, "sth", FulfillmentType.Sth, values);
        return values;
    }

    private static void AddIfTrue(JsonElement element, string propertyName, FulfillmentType fulfillmentType, ICollection<FulfillmentType> values)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False && property.GetBoolean())
        {
            values.Add(fulfillmentType);
        }
    }
}
