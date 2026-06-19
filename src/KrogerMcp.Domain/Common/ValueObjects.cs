namespace KrogerMcp.Domain.Common;

public readonly record struct LocationId(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct ProductId(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct CustomerAccessToken(string Value)
{
    public override string ToString() => Value;
}
