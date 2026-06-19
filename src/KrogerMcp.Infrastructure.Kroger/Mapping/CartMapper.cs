using KrogerMcp.Domain.Cart;
using KrogerMcp.Generated.Cart;

namespace KrogerMcp.Infrastructure.Kroger.Mapping;

public static class CartMapper
{
    public static CartAddRequest ToGeneratedRequest(AddToCartCommand command)
    {
        var modality = command.Modality == CartModality.Delivery ? "DELIVERY" : "PICKUP";
        return new CartAddRequest(command.Items
            .Select(item => new CartAddItemRequest(item.Quantity, item.UpcOrProductId, modality))
            .ToArray());
    }
}
