using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;

namespace Identity.UnitTests.Domain.Entities;

public sealed class UserTests
{
    [Fact]
    public void Create_With_Valid_Data_Should_Create_User()
    {
        // Arrange
        var email =
            Email.Create("test@example.com");

        // Act
        var user =
            User.Create(
                email,
                "hashed-password",
                "Ali",
                "Ahmadi");

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            user.Id);

        Assert.Equal(
            email,
            user.Email);

        Assert.Equal(
            "hashed-password",
            user.PasswordHash);

        Assert.Equal(
            "Ali",
            user.FirstName);

        Assert.Equal(
            "Ahmadi",
            user.LastName);
    }

    [Fact]
    public void Create_Should_Trim_FirstName_And_LastName()
    {
        // Arrange
        var email =
            Email.Create("test@example.com");

        // Act
        var user =
            User.Create(
                email,
                "hashed-password",
                "  Ali  ",
                "  Ahmadi  ");

        // Assert
        Assert.Equal(
            "Ali",
            user.FirstName);

        Assert.Equal(
            "Ahmadi",
            user.LastName);
    }

    [Fact]
    public void Create_With_Empty_PasswordHash_Should_Throw()
    {
        // Arrange
        var email =
            Email.Create("test@example.com");

        // Act
        var act = () =>
            User.Create(
                email,
                "",
                "Ali",
                "Ahmadi");

        // Assert
        Assert.Throws<ArgumentException>(
            act);
    }

    [Fact]
    public void Create_With_Empty_FirstName_Should_Throw()
    {
        var email =
            Email.Create("test@example.com");

        var act = () =>
            User.Create(
                email,
                "hashed-password",
                "",
                "Ahmadi");

        Assert.Throws<ArgumentException>(
            act);
    }

    [Fact]
    public void Create_With_Empty_LastName_Should_Throw()
    {
        var email =
            Email.Create("test@example.com");

        var act = () =>
            User.Create(
                email,
                "hashed-password",
                "Ali",
                "");

        Assert.Throws<ArgumentException>(
            act);
    }
}
