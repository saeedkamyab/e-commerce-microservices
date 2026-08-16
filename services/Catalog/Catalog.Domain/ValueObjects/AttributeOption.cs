namespace Catalog.Domain.ValueObjects;

public sealed record AttributeOption
{
    public string Value { get; }

    private AttributeOption(string value)
    {
        Value = value;
    }

    public static AttributeOption Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Attribute option cannot be empty.",
                nameof(value));

        value = value.Trim();

        if (value.Length > 100)
            throw new ArgumentException(
                "Attribute option cannot exceed 100 characters.",
                nameof(value));

        return new AttributeOption(value);
    }

    public override string ToString() => Value;
}
