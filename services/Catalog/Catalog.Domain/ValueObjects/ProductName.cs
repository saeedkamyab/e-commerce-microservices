namespace Catalog.Domain.ValueObjects;

public sealed record ProductName
{
    public string Value {  get;}
    public ProductName(string value)
    {
        Value = value;
    }
    public static ProductName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product name cannot be empty.", nameof(value));

        value = value.Trim();

        if (value.Length > 200)
            throw new ArgumentException(
                "Product name cannot exceed 200 characters.",
                nameof(value));

        return new ProductName(value);
    }

    public override string ToString() => Value;
}
