using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Persistence;
using Identity.Application.Users.Login;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.IntegrationTests.Applications.Login;

[Collection(IdentityDatabaseCollection.Name)]
public class LoginUserTests
{

    private readonly IdentityDatabaseFixture _fixture;

    public LoginUserTests(IdentityDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_With_Valid_Credentials_Should_Return_Tokens_And_Persist_RefreshToken()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.RefreshTokens.ExecuteDeleteAsync();
        await dbContext.Users.ExecuteDeleteAsync();

        var userRepository =
            new UserRepository(dbContext);

        var refreshTokenRepository =
            new RefreshTokenRepository(dbContext);

        IPasswordHasher passwordHasher =
            new PasswordHasher();

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        const string password =
            "StrongPassword123!";

        var user =
            User.Create(
                Email.Create("login@example.com"),
                passwordHasher.Hash(password),
                "Ali",
                "Ahmadi");

        await userRepository.AddAsync(
            user,
            CancellationToken.None);

        await dbContext.SaveChangesAsync();

        var options =
            Options.Create(
                new RefreshTokenOptions
                {
                    ExpirationDays = 7
                });

        var handler =
            new LoginUserCommandHandler(
                userRepository,
                passwordHasher,
                new FakeAccessTokenProvider(),
                refreshTokenProvider,
                refreshTokenRepository,
                dbContext,
                options);

        var command =
            new LoginUserCommand(
                "LOGIN@EXAMPLE.COM",
                password);

        // Act
        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.Equal(
            user.Id,
            result.UserId);

        Assert.Equal(
            $"fake-token-{user.Id}",
            result.AccessToken);

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.RefreshToken));

        await using var assertionContext =
            _fixture.CreateDbContext();

        var persistedRefreshToken =
            await assertionContext.RefreshTokens
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal(
            user.Id,
            persistedRefreshToken.UserId);

        Assert.NotEqual(
            result.RefreshToken,
            persistedRefreshToken.TokenHash);

        Assert.Equal(
            refreshTokenProvider.Hash(
                result.RefreshToken),
            persistedRefreshToken.TokenHash);

        Assert.Null(
            persistedRefreshToken.RevokedOnUtc);

        Assert.True(
            persistedRefreshToken.ExpiresOnUtc >
            DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_With_Invalid_Password_Should_Throw()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.Users.ExecuteDeleteAsync();

        var repository =
            new UserRepository(dbContext);

        IPasswordHasher passwordHasher =
            new PasswordHasher();
        var refreshTokenRepository =
         new RefreshTokenRepository(dbContext);

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        var user =
            User.Create(
                Email.Create("login@example.com"),
                passwordHasher.Hash("CorrectPassword123!"),
                "Ali",
                "Ahmadi");

        await repository.AddAsync(
            user,
            CancellationToken.None);

        await dbContext.SaveChangesAsync();

        var options =
           Options.Create(
               new RefreshTokenOptions
               {
                   ExpirationDays = 7
               });


        var handler =
             new LoginUserCommandHandler(
                 repository,
                 passwordHasher,
                 new FakeAccessTokenProvider(),
                 refreshTokenProvider,
                 refreshTokenRepository,
                 dbContext,
                 options);

        var command =
            new LoginUserCommand(
                "login@example.com",
                "WrongPassword123!");

        // Act
        var act = async () =>
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                act);

        Assert.Equal(
            "Invalid email or password.",
            exception.Message);

        Assert.Empty(
    await dbContext.RefreshTokens
        .AsNoTracking()
        .ToListAsync());
    }

    [Fact]
    public async Task Handle_When_User_Does_Not_Exist_Should_Throw()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.Users.ExecuteDeleteAsync();

        var repository =
            new UserRepository(dbContext);
      
        var refreshTokenRepository =
    new RefreshTokenRepository(dbContext);

        IPasswordHasher passwordHasher =
            new PasswordHasher();

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        var options =
           Options.Create(
               new RefreshTokenOptions
               {
                   ExpirationDays = 7
               });

        var handler =
              new LoginUserCommandHandler(
                  repository,
                  passwordHasher,
                  new FakeAccessTokenProvider(),
                  refreshTokenProvider,
                  refreshTokenRepository,
                  dbContext,
                  options);

        var command =
            new LoginUserCommand(
                "missing@example.com",
                "SomePassword123!");

        var act = async () =>
            await handler.Handle(
                command,
                CancellationToken.None);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                act);

        Assert.Equal(
            "Invalid email or password.",
            exception.Message);

        Assert.Empty(
    await dbContext.RefreshTokens
        .AsNoTracking()
        .ToListAsync());
    }
}
