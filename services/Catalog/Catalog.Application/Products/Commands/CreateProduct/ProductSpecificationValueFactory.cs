using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Products.Commands.CreateProduct;

internal static class ProductSpecificationValueFactory
{
    public static ProductSpecificationValue Create(
        AttributeType type,
        string value)
    {
        return type switch
        {
            AttributeType.Text =>
                ProductSpecificationValue.CreateText(value),

            AttributeType.Number =>
                ProductSpecificationValue.CreateNumber(
                    ParseInt(value)),

            AttributeType.Decimal =>
                ProductSpecificationValue.CreateDecimal(
                    ParseDecimal(value)),

            AttributeType.Boolean =>
                ProductSpecificationValue.CreateBoolean(
                    ParseBool(value)),

            AttributeType.Date =>
                ProductSpecificationValue.CreateDate(
                    ParseDate(value)),

            _ => throw new InvalidOperationException(
                $"Unsupported attribute type: {type}")
        };
    }

    private static int ParseInt(string value)
    {
        if (!int.TryParse(value, out var result))
            throw new ArgumentException(
                $"'{value}' is not a valid number.");

        return result;
    }

    private static decimal ParseDecimal(string value)
    {
        if (!decimal.TryParse(value, out var result))
            throw new ArgumentException(
                $"'{value}' is not a valid decimal.");

        return result;
    }

    private static bool ParseBool(string value)
    {
        if (!bool.TryParse(value, out var result))
            throw new ArgumentException(
                $"'{value}' is not a valid boolean.");

        return result;
    }

    private static DateTime ParseDate(string value)
    {
        if (!DateTime.TryParse(value, out var result))
            throw new ArgumentException(
                $"'{value}' is not a valid date.");

        return result;
    }
}
