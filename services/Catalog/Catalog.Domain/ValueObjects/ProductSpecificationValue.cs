using Catalog.Domain.Enums;

namespace Catalog.Domain.ValueObjects;

public sealed record ProductSpecificationValue
{
    public AttributeType Type { get; }

    public object Value { get; }

    private ProductSpecificationValue(
        AttributeType type,
        object value)
    {
        Type = type;
        Value = value;
    }

    public static ProductSpecificationValue CreateText(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Text value cannot be empty.",
                nameof(value));

        return new ProductSpecificationValue(
            AttributeType.Text,
            value.Trim());
    }

    public static ProductSpecificationValue CreateNumber(
        int value)
    {
        return new ProductSpecificationValue(
            AttributeType.Number,
            value);
    }

    public static ProductSpecificationValue CreateDecimal(
        decimal value)
    {
        return new ProductSpecificationValue(
            AttributeType.Decimal,
            value);
    }

    public static ProductSpecificationValue CreateBoolean(
        bool value)
    {
        return new ProductSpecificationValue(
            AttributeType.Boolean,
            value);
    }

    public static ProductSpecificationValue CreateDate(
        DateTime value)
    {
        return new ProductSpecificationValue(
            AttributeType.Date,
            value);
    }

    public static ProductSpecificationValue CreateOption(
        AttributeOption value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new ProductSpecificationValue(
            AttributeType.Option,
            value);
    }
}