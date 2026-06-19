using System.Reflection;
using KrogerMcp.Host.McpTools;

namespace KrogerMcp.Tests.Host;

public sealed class McpToolRegistrationTests
{
    [Fact]
    public void Tool_methods_are_present()
    {
        var methods = new[]
        {
            typeof(ProductTools).GetMethod("SearchProducts"),
            typeof(ProductTools).GetMethod("GetProduct"),
            typeof(LocationTools).GetMethod("LookupLocations"),
            typeof(LocationTools).GetMethod("GetLocation"),
            typeof(CartTools).GetMethod("AddToCart")
        };

        Assert.All(methods, method => Assert.NotNull(method));
        Assert.All(methods, method => Assert.True(method!.GetCustomAttributes().Any()));
    }
}
