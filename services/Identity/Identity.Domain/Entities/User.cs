using Identity.Domain.ValueObjects;

namespace Identity.Domain.Entities;

public sealed class User
{

    public Guid Id { get; private set; }

    public Email Email { get; private set; } = null!;

    public string? PasswordHash { get; private set; }

    public string FirstName { get; private set; } = null!;

    public string LastName { get; private set; } = null!;

    public DateTime CreatedOnUtc { get; private set; }

    private User()
    {
    }

    private User(
        Guid id,
        Email email,
        string? passwordHash,
        string firstName,
        string lastName,
        DateTime createdOnUtc)
    {

        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        CreatedOnUtc = createdOnUtc;
    }



    public static User CreateLocal(
       Email email,
       string passwordHash,
       string firstName,
       string lastName)
    {
        ValidateCommonFields(
            email,
            firstName,
            lastName);

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException(
                "Password hash cannot be empty.",
                nameof(passwordHash));
        }

        return new User(
            Guid.NewGuid(),
            email,
            passwordHash,
            firstName,
            lastName,
            DateTime.UtcNow);
    }

    public static User CreateExternal(
        Email email,
        string firstName,
        string lastName)
    {
        ValidateCommonFields(
            email,
            firstName,
            lastName);

        return new User(
            Guid.NewGuid(),
            email,
            null,
            firstName,
            lastName,
            DateTime.UtcNow);
    }

    private static void ValidateCommonFields(
        Email email,
        string firstName,
        string lastName)
    {
        if (email is null)
        {
            throw new ArgumentNullException(
                nameof(email));
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException(
                "First name cannot be empty.",
                nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException(
                "Last name cannot be empty.",
                nameof(lastName));
        }
    }

}
