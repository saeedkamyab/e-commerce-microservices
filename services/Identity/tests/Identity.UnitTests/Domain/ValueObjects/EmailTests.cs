using Identity.Domain.ValueObjects;

namespace Identity.UnitTests.Domain.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Create_With_Valid_Email_Should_Create_Email()
    {
        // Act
        var email =
            Email.Create("Test@Example.com");

        // Assert
        Assert.Equal(
            "test@example.com",
            email.Value);
    }

    [Fact]
    public void Create_Should_Trim_Email()
    {
        // Act
        var email =
            Email.Create("  test@example.com  ");

        // Assert
        Assert.Equal(
            "test@example.com",
            email.Value);
    }

    [Fact]
    public void Create_With_Empty_Email_Should_Throw()
    {
        // Act
        var act = () =>
            Email.Create("");

        // Assert
        Assert.Throws<ArgumentException>(
            act);
    }

    [Fact]
    public void Create_With_Invalid_Email_Should_Throw()
    {
        // Act
        var act = () =>
            Email.Create("invalid-email");

        // Assert
        Assert.Throws<ArgumentException>(
            act);
    }
}
