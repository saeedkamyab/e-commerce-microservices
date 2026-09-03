using Identity.Application.Abstractions.Authentication;
using Identity.Application.Users.Register;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Identity.IntegrationTests.Applications.Register;

[Collection(IdentityDatabaseCollection.Name)]
public sealed class RegisterUserTests
{
    private readonly IdentityDatabaseFixture _fixture;

    public RegisterUserTests(
        IdentityDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_Should_Persist_User_With_Hashed_Password()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.Users.ExecuteDeleteAsync();

        var repository =
            new UserRepository(dbContext);

        IPasswordHasher passwordHasher =
            new PasswordHasher();

        var handler =
            new RegisterUserCommandHandler(
                repository,
                dbContext,
                passwordHasher);

        var command =
            new RegisterUserCommand(
                $"user-{Guid.NewGuid()}@example.com",
                "StrongPassword123!",
                "Ali",
                "Ahmadi");

        // Act
        var userId =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        await using var assertionContext =
            _fixture.CreateDbContext();

        var user =
            await assertionContext.Users
                .AsNoTracking()
                .SingleAsync(x => x.Id == userId);

        Assert.Equal(
            command.Email.ToLowerInvariant(),
            user.Email.Value);

        Assert.Equal(
            command.FirstName,
            user.FirstName);

        Assert.Equal(
            command.LastName,
            user.LastName);

        Assert.NotEqual(
            command.Password,
            user.PasswordHash);

        Assert.False(
            string.IsNullOrWhiteSpace(
                user.PasswordHash));
    }

    [Fact]
    public async Task Handle_When_Email_Already_Exists_Should_Throw()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.Users.ExecuteDeleteAsync();

        var repository =
            new UserRepository(dbContext);

        IPasswordHasher passwordHasher =
            new PasswordHasher();

        var handler =
            new RegisterUserCommandHandler(
                repository,
                dbContext,
                passwordHasher);

        var email =
            $"duplicate-{Guid.NewGuid()}@example.com";

        var firstCommand =
            new RegisterUserCommand(
                email,
                "StrongPassword123!",
                "Ali",
                "Ahmadi");

        var secondCommand =
            new RegisterUserCommand(
                email.ToUpperInvariant(),
                "AnotherPassword123!",
                "Reza",
                "Karimi");

        await handler.Handle(
            firstCommand,
            CancellationToken.None);

        // Act
        var act = async () =>
            await handler.Handle(
                secondCommand,
                CancellationToken.None);

        // Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                act);

        Assert.Equal(
            "A user with this email already exists.",
            exception.Message);

        await using var assertionContext =
            _fixture.CreateDbContext();

        var usersCount =
            await assertionContext.Users
                .CountAsync();

        Assert.Equal(
            1,
            usersCount);
    }
}
