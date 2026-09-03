namespace Identity.Domain.ValueObjects;

public sealed record Email
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Email cannot be empty.",
                nameof(value));
        }

        value = value.Trim().ToLowerInvariant();

        if (!value.Contains('@'))
        {
            throw new ArgumentException(
                "Email is invalid.",
                nameof(value));
        }

        return new Email(value);
    }

    public override string ToString()
    {
        return Value;
    }
}