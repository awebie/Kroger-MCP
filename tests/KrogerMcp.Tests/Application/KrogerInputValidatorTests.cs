using KrogerMcp.Application.Contracts;
using KrogerMcp.Application.Validation;

namespace KrogerMcp.Tests.Application;

public sealed class KrogerInputValidatorTests
{
    private readonly KrogerInputValidator _validator = new();

    [Fact]
    public void SearchProducts_requires_location_id()
    {
        var error = _validator.Validate(new SearchProductsRequest("milk", ""));

        Assert.NotNull(error);
        Assert.Contains("LocationId", error.Message);
    }

    [Fact]
    public void SearchProducts_requires_term_or_brand()
    {
        var error = _validator.Validate(new SearchProductsRequest(null, "01400943"));

        Assert.NotNull(error);
        Assert.Contains("Term or Brand", error.Message);
    }

    [Fact]
    public void LookupLocations_requires_exactly_one_origin()
    {
        var error = _validator.Validate(new LookupLocationsRequest(ZipCode: "45044", LocationId: "01400943"));

        Assert.NotNull(error);
        Assert.Contains("exactly one origin", error.Message);
    }

    [Fact]
    public void AddToCart_requires_items()
    {
        var error = _validator.Validate(new AddToCartRequest([]));

        Assert.NotNull(error);
        Assert.Contains("At least one", error.Message);
    }
}
