using KrogerMcp.Domain.Common;

namespace KrogerMcp.Domain.Cart;

public sealed record CartItem(string UpcOrProductId, int Quantity);

public sealed record AddToCartCommand(IReadOnlyList<CartItem> Items, CartModality Modality);

public sealed record CartAddResult(bool Success, string Message)
{
    public static CartAddResult Added(int itemCount) => new(true, $"Added {itemCount} item(s) to cart.");
}

public enum CartModality
{
    Pickup,
    Delivery
}
