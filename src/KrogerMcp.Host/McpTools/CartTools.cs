using System.ComponentModel;
using KrogerMcp.Application.Contracts;
using KrogerMcp.Application.UseCases;
using ModelContextProtocol.Server;

namespace KrogerMcp.Host.McpTools;

[McpServerToolType]
public sealed class CartTools(AddToCartHandler addToCartHandler)
{
    [McpServerTool(Name = "add_to_cart")]
    [Description("Add one or more Kroger UPCs or product ids to the authenticated customer's cart.")]
    public async Task<AddToCartResponse> AddToCart(AddToCartRequest request, CancellationToken cancellationToken)
    {
        return McpToolResultMapper.Unwrap(await addToCartHandler.HandleAsync(request, cancellationToken));
    }
}
