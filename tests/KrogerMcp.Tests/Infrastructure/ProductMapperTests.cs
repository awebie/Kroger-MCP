using System.Text.Json;
using KrogerMcp.Infrastructure.Kroger.Mapping;

namespace KrogerMcp.Tests.Infrastructure;

public sealed class ProductMapperTests
{
    [Fact]
    public void ToSearchResult_maps_core_product_fields()
    {
        using var document = JsonDocument.Parse("""
        {
          "data": [
            {
              "productId": "0001111041700",
              "upc": "0001111041700",
              "brand": "Kroger",
              "description": "Kroger Milk",
              "categories": ["Dairy"],
              "items": [
                {
                  "itemId": "0001111041700",
                  "size": "1 gal",
                  "price": { "regular": 3.49, "promo": 2.99 },
                  "inventory": { "stockLevel": "HIGH" }
                }
              ]
            }
          ]
        }
        """);

        var result = ProductMapper.ToSearchResult(document);

        Assert.Single(result.Products);
        Assert.Equal("Kroger Milk", result.Products[0].Description);
        Assert.Equal(3.49m, result.Products[0].Items[0].Price!.Regular);
    }

    [Fact]
    public void ToSearchResult_does_not_throw_for_numeric_scalar_values()
    {
        using var document = JsonDocument.Parse("""
        {
          "data": [
            {
              "productId": "0001111041700",
              "items": [
                {
                  "itemId": 1111041700,
                  "inventory": { "stockLevel": 0 }
                }
              ]
            }
          ]
        }
        """);

        var result = ProductMapper.ToSearchResult(document);

        Assert.Equal("1111041700", result.Products[0].Items[0].ItemId);
        Assert.Equal(KrogerMcp.Domain.Products.InventoryStatus.Unknown, result.Products[0].Items[0].Inventory);
    }
}
