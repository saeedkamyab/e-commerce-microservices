using Identity.Application.Abstractions.Authentication;
using Identity.Application.RefreshToken;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.IntegrationTests.Applications.RefreshToken;

[Collection(IdentityDatabaseCollection.Name)]
public sealed class RefreshTokenTests
{
    private readonly IdentityDatabaseFixture _fixture;

    public RefreshTokenTests(
        IdentityDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_With_Valid_RefreshToken_Should_Rotate_Token()
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

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        var user =
            User.Create(
                Email.Create("refresh@example.com"),
                "hashed-password",
                "Ali",
                "Ahmadi");

        await userRepository.AddAsync(
            user,
            CancellationToken.None);

        var oldRawToken =
            refreshTokenProvider.Generate();

        var oldTokenHash =
            refreshTokenProvider.Hash(
                oldRawToken);

        var oldRefreshToken =
            Identity.Domain.Entities.RefreshToken.Create(
                user.Id,
                oldTokenHash,
                DateTime.UtcNow.AddDays(7));

        await refreshTokenRepository.AddAsync(
            oldRefreshToken,
            CancellationToken.None);

        await dbContext.SaveChangesAsync();

        var handler =
            new RefreshTokenCommandHandler(
                userRepository,
                refreshTokenRepository,
                refreshTokenProvider,
                new FakeAccessTokenProvider(),
                dbContext,
                Options.Create(
                    new RefreshTokenOptions
                    {
                        ExpirationDays = 7
                    }));

        var command =
            new RefreshTokenCommand(
                oldRawToken);

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

        Assert.NotEqual(
            oldRawToken,
            result.RefreshToken);

        await using var assertionContext =
            _fixture.CreateDbContext();

        var tokens =
            await assertionContext.RefreshTokens
                .AsNoTracking()
                .OrderBy(x => x.CreatedOnUtc)
                .ToListAsync();

        Assert.Equal(
            2,
            tokens.Count);

        var persistedOldToken =
            tokens.Single(x =>
                x.TokenHash == oldTokenHash);

        Assert.NotNull(
            persistedOldToken.RevokedOnUtc);

        var newTokenHash =
            refreshTokenProvider.Hash(
                result.RefreshToken);

        var persistedNewToken =
            tokens.Single(x =>
                x.TokenHash == newTokenHash);

        Assert.Null(
            persistedNewToken.RevokedOnUtc);

        Assert.True(
            persistedNewToken.ExpiresOnUtc >
            DateTime.UtcNow);
        Assert.Equal(
    persistedOldToken.FamilyId,
    persistedNewToken.FamilyId);

        Assert.Equal(
            persistedNewToken.Id,
            persistedOldToken.ReplacedByTokenId);
    }

    [Fact]
    public async Task Handle_When_RefreshToken_Is_Already_Revoked_Should_Throw()
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

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        var user =
            User.Create(
                Email.Create("revoked@example.com"),
                "hashed-password",
                "Ali",
                "Ahmadi");

        await userRepository.AddAsync(
            user,
            CancellationToken.None);

        var rawToken =
            refreshTokenProvider.Generate();

        var refreshToken =
            Identity.Domain.Entities.RefreshToken.Create(
                user.Id,
                refreshTokenProvider.Hash(rawToken),
                DateTime.UtcNow.AddDays(7));

        refreshToken.Revoke();

        await refreshTokenRepository.AddAsync(
            refreshToken,
            CancellationToken.None);

        await dbContext.SaveChangesAsync();

        var handler =
            new RefreshTokenCommandHandler(
                userRepository,
                refreshTokenRepository,
                refreshTokenProvider,
                new FakeAccessTokenProvider(),
                dbContext,
                Options.Create(
                    new RefreshTokenOptions
                    {
                        ExpirationDays = 7
                    }));

        // Act
        var act = async () =>
            await handler.Handle(
                new RefreshTokenCommand(rawToken),
                CancellationToken.None);

        // Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                act);

        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);

        var tokenCount =
            await dbContext.RefreshTokens.CountAsync();

        Assert.Equal(
            1,
            tokenCount);
    }

    [Fact]
    public async Task Handle_When_Previous_Token_Is_Reused_Should_Revoke_Entire_Family()
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

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        var user =
            User.Create(
                Email.Create("reuse@example.com"),
                "hashed-password",
                "Ali",
                "Ahmadi");

        await userRepository.AddAsync(
            user,
            CancellationToken.None);

        var rawTokenA =
            refreshTokenProvider.Generate();

        var tokenA =
            Identity.Domain.Entities.RefreshToken.Create(
                user.Id,
                refreshTokenProvider.Hash(rawTokenA),
                DateTime.UtcNow.AddDays(7));

        await refreshTokenRepository.AddAsync(
            tokenA,
            CancellationToken.None);

        await dbContext.SaveChangesAsync();

        var handler =
            new RefreshTokenCommandHandler(
                userRepository,
                refreshTokenRepository,
                refreshTokenProvider,
                new FakeAccessTokenProvider(),
                dbContext,
                Options.Create(
                    new RefreshTokenOptions
                    {
                        ExpirationDays = 7
                    }));

        // A -> B
        var firstResult =
            await handler.Handle(
                new RefreshTokenCommand(rawTokenA),
                CancellationToken.None);

        var rawTokenB =
            firstResult.RefreshToken;

        // Act
        // attacker tries A again
        var act = async () =>
            await handler.Handle(
                new RefreshTokenCommand(rawTokenA),
                CancellationToken.None);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                act);

        // Assert
        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);

        await using var assertionContext =
            _fixture.CreateDbContext();

        var tokens =
            await assertionContext.RefreshTokens
                .AsNoTracking()
                .ToListAsync();

        Assert.Equal(2, tokens.Count);

        Assert.All(
            tokens,
            token =>
                Assert.NotNull(token.RevokedOnUtc));

        var tokenBHash =
            refreshTokenProvider.Hash(rawTokenB);

        var persistedTokenB =
            tokens.Single(x =>
                x.TokenHash == tokenBHash);

        Assert.NotNull(
            persistedTokenB.RevokedOnUtc);
    }

    [Fact]
    public async Task Handle_After_Token_Reuse_Should_Reject_Current_Family_Token()
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

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        var user =
            User.Create(
                Email.Create("compromised@example.com"),
                "hashed-password",
                "Ali",
                "Ahmadi");

        await userRepository.AddAsync(
            user,
            CancellationToken.None);

        var rawTokenA =
            refreshTokenProvider.Generate();

        var tokenA =
            Identity.Domain.Entities.RefreshToken.Create(
                user.Id,
                refreshTokenProvider.Hash(rawTokenA),
                DateTime.UtcNow.AddDays(7));

        await refreshTokenRepository.AddAsync(
            tokenA,
            CancellationToken.None);

        await dbContext.SaveChangesAsync();

        var handler =
            new RefreshTokenCommandHandler(
                userRepository,
                refreshTokenRepository,
                refreshTokenProvider,
                new FakeAccessTokenProvider(),
                dbContext,
                Options.Create(
                    new RefreshTokenOptions
                    {
                        ExpirationDays = 7
                    }));

        
        var firstResult =
            await handler.Handle(
                new RefreshTokenCommand(rawTokenA),
                CancellationToken.None);

        var rawTokenB =
            firstResult.RefreshToken;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(
                new RefreshTokenCommand(rawTokenA),
                CancellationToken.None));

     
        var act = () =>
            handler.Handle(
                new RefreshTokenCommand(rawTokenB),
                CancellationToken.None);

       
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                act);

        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);

        await using var assertionContext =
            _fixture.CreateDbContext();

        var tokens =
            await assertionContext.RefreshTokens
                .AsNoTracking()
                .ToListAsync();

        Assert.Equal(2, tokens.Count);

        Assert.All(
            tokens,
            token =>
                Assert.NotNull(token.RevokedOnUtc));
    }

}
