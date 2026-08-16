namespace Catalog.Domain.ValueObjects;

public sealed record CategoryName
{
    public string Value { get; }

    private CategoryName(string value)
    {
        Value = value;
    }

    public static CategoryName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Category name cannot be empty.",
                nameof(value));

        value = value.Trim();

        if (value.Length > 100)
            throw new ArgumentException(
                "Category name cannot exceed 100 characters.",
                nameof(value));

        return new CategoryName(value);
    }

    public override string ToString() => Value;
}
