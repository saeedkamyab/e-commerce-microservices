using Identity.Application.Abstractions.Authentication;
using Identity.Application.Users.ExternalLogin;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.IntegrationTests.Applications.ExternalLogin;

[Collection(IdentityDatabaseCollection.Name)]
public class GoogleLoginTests
{
    private readonly IdentityDatabaseFixture _fixture;

    public GoogleLoginTests(
        IdentityDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_First_External_Login_Should_Create_User_Identity_And_RefreshToken()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await ClearDatabaseAsync(dbContext);

        var externalProvider =
            new FakeExternalIdentityProvider
            {
                Result =
                    new ExternalIdentityResult(
                        "Google",
                        "google-123",
                        "google.user@example.com",
                        "Google",
                        "User")
            };

        var handler =
            CreateHandler(
                dbContext,
                externalProvider);

        // Act
        var result =
            await handler.Handle(
                new ExternalLoginCommand(
                    "fake-google-id-token"),
                CancellationToken.None);

        // Assert
        var users =
            await dbContext.Users
                .AsNoTracking()
                .ToListAsync();

        Assert.Single(users);

        var user = users.Single();

        Assert.Equal(
            user.Id,
            result.UserId);

        Assert.Equal(
            "google.user@example.com",
            user.Email.Value);

        Assert.Null(
            user.PasswordHash);

        var identities =
            await dbContext.ExternalIdentities
                .AsNoTracking()
                .ToListAsync();

        Assert.Single(identities);

        var identity =
            identities.Single();

        Assert.Equal(
            user.Id,
            identity.UserId);

        Assert.Equal(
            "Google",
            identity.Provider);

        Assert.Equal(
            "google-123",
            identity.ProviderUserId);

        var refreshTokens =
            await dbContext.RefreshTokens
                .AsNoTracking()
                .ToListAsync();

        Assert.Single(refreshTokens);

        Assert.Equal(
            user.Id,
            refreshTokens.Single().UserId);

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.AccessToken));

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.RefreshToken));
    }

    [Fact]
    public async Task Handle_Existing_External_Identity_Should_Use_Same_User()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await ClearDatabaseAsync(dbContext);

        var externalProvider =
            new FakeExternalIdentityProvider
            {
                Result =
                    new ExternalIdentityResult(
                        "Google",
                        "google-456",
                        "same.user@example.com",
                        "Same",
                        "User")
            };

        var handler =
            CreateHandler(
                dbContext,
                externalProvider);

        var firstResult =
            await handler.Handle(
                new ExternalLoginCommand(
                    "token-1"),
                CancellationToken.None);

        // Act
        var secondResult =
            await handler.Handle(
                new ExternalLoginCommand(
                    "token-2"),
                CancellationToken.None);

        // Assert
        Assert.Equal(
            firstResult.UserId,
            secondResult.UserId);

        Assert.Equal(
            1,
            await dbContext.Users.CountAsync());

        Assert.Equal(
            1,
            await dbContext.ExternalIdentities
                .CountAsync());

        Assert.Equal(
            2,
            await dbContext.RefreshTokens
                .CountAsync());
    }

    [Fact]
    public async Task Handle_When_Local_User_With_Same_Email_Exists_Should_Link_External_Identity()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await ClearDatabaseAsync(dbContext);

        var localUser =
            User.CreateLocal(
                Email.Create(
                    "existing@example.com"),
                "some-valid-password-hash",
                "Ali",
                "Ahmadi");

        dbContext.Users.Add(
            localUser);

        await dbContext.SaveChangesAsync();

        var externalProvider =
            new FakeExternalIdentityProvider
            {
                Result =
                    new ExternalIdentityResult(
                        "Google",
                        "google-existing-789",
                        "EXISTING@EXAMPLE.COM",
                        "Ali",
                        "Ahmadi")
            };

        var handler =
            CreateHandler(
                dbContext,
                externalProvider);

        // Act
        var result =
            await handler.Handle(
                new ExternalLoginCommand(
                    "google-token"),
                CancellationToken.None);

        // Assert
        Assert.Equal(
            localUser.Id,
            result.UserId);

        Assert.Equal(
            1,
            await dbContext.Users.CountAsync());

        var externalIdentity =
            await dbContext.ExternalIdentities
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal(
            localUser.Id,
            externalIdentity.UserId);

        Assert.Equal(
            "Google",
            externalIdentity.Provider);

        Assert.Equal(
            "google-existing-789",
            externalIdentity.ProviderUserId);

        var persistedUser =
            await dbContext.Users
                .AsNoTracking()
                .SingleAsync();

        Assert.NotNull(
            persistedUser.PasswordHash);

        Assert.Equal(
            1,
            await dbContext.RefreshTokens
                .CountAsync());
    }

    private static ExternalLoginCommandHandler CreateHandler(
        IdentityDbContext dbContext,
        IExternalIdentityProvider externalIdentityProvider)
    {
        var externalIdentityRepository =
            new ExternalIdentityRepository(
                dbContext);

        var userRepository =
            new UserRepository(
                dbContext);

        var refreshTokenRepository =
            new RefreshTokenRepository(
                dbContext);

        var accessTokenProvider =
            new FakeAccessTokenProvider();

        var refreshTokenProvider =
            new FakeRefreshTokenProvider();

        var refreshTokenOptions =
            Options.Create(
                new RefreshTokenOptions
                {
                    ExpirationDays = 7
                });

        return new ExternalLoginCommandHandler(
            externalIdentityProvider,
            externalIdentityRepository,
            userRepository,
            accessTokenProvider,
            refreshTokenProvider,
            refreshTokenRepository,
            dbContext,
            refreshTokenOptions);
    }

    private static async Task ClearDatabaseAsync(
        IdentityDbContext dbContext)
    {
        await dbContext.RefreshTokens
            .ExecuteDeleteAsync();

        await dbContext.ExternalIdentities
            .ExecuteDeleteAsync();

        await dbContext.Users
            .ExecuteDeleteAsync();
    }
}