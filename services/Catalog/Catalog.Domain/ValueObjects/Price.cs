namespace Catalog.Domain.ValueObjects;

public sealed record Price
{
    public decimal Amount { get; }
    public string Currency { get; }
    public Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    public static Price Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException(
                "Price cannot be negative.",
                nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException(
                "Currency is required.",
                nameof(currency));

        return new Price(amount, currency.Trim().ToUpperInvariant());
    }
}
