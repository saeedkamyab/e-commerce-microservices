using Identity.Application.Abstractions.Authentication;
using Identity.Application.Common.Exceptions;
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
            User.CreateLocal(
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
            User.CreateLocal(
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
            await Assert.ThrowsAsync<UnauthorizedException>(
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
            User.CreateLocal(
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
            await Assert.ThrowsAsync<UnauthorizedException>(
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
            User.CreateLocal(
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

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(
                new RefreshTokenCommand(rawTokenA),
                CancellationToken.None));

     
        var act = () =>
            handler.Handle(
                new RefreshTokenCommand(rawTokenB),
                CancellationToken.None);

       
        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
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


    [Fact]
    public async Task Concurrent_Update_Of_Same_RefreshToken_Should_Fail()
    {
        // Arrange
        await using var setupContext =
            _fixture.CreateDbContext();

        await setupContext.RefreshTokens.ExecuteDeleteAsync();
        await setupContext.Users.ExecuteDeleteAsync();

        var user =
            User.CreateLocal(
                Email.Create("concurrency@example.com"),
                "hashed-password",
                "Ali",
                "Ahmadi");

        setupContext.Users.Add(user);

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        var rawToken =
            refreshTokenProvider.Generate();

        var refreshToken =
            Identity.Domain.Entities.RefreshToken.Create(
                user.Id,
                refreshTokenProvider.Hash(rawToken),
                DateTime.UtcNow.AddDays(7));

        setupContext.RefreshTokens.Add(refreshToken);

        await setupContext.SaveChangesAsync();

        // Two independent requests / DbContexts
        await using var context1 =
            _fixture.CreateDbContext();

        await using var context2 =
            _fixture.CreateDbContext();

        var token1 =
            await context1.RefreshTokens
                .SingleAsync(x =>
                    x.Id == refreshToken.Id);

        var token2 =
            await context2.RefreshTokens
                .SingleAsync(x =>
                    x.Id == refreshToken.Id);

        token1.ReplaceWith(Guid.NewGuid());

        token2.ReplaceWith(Guid.NewGuid());

     
        await context1.SaveChangesAsync();

        // Act
        var act = () =>
            context2.SaveChangesAsync();

        // Assert
        await Assert.ThrowsAsync<ConcurrencyException>(
            act);
    }


    [Fact]
    public async Task Handle_When_RefreshToken_Is_Updated_Concurrently_Should_Reject_Request()
    {
        // Arrange
        await using var setupContext =
            _fixture.CreateDbContext();

        await setupContext.RefreshTokens.ExecuteDeleteAsync();
        await setupContext.Users.ExecuteDeleteAsync();

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        var user =
            User.CreateLocal(
                Email.Create("refresh-race@example.com"),
                "hashed-password",
                "Ali",
                "Ahmadi");

        setupContext.Users.Add(user);

        var rawToken =
            refreshTokenProvider.Generate();

        var refreshToken =
            Identity.Domain.Entities.RefreshToken.Create(
                user.Id,
                refreshTokenProvider.Hash(rawToken),
                DateTime.UtcNow.AddDays(7));

        setupContext.RefreshTokens.Add(refreshToken);

        await setupContext.SaveChangesAsync();

        await using var context1 =
            _fixture.CreateDbContext();

        await using var context2 =
            _fixture.CreateDbContext();

        var userRepository1 =
            new UserRepository(context1);

        var refreshTokenRepository1 =
            new RefreshTokenRepository(context1);

        var userRepository2 =
            new UserRepository(context2);

        var refreshTokenRepository2 =
            new RefreshTokenRepository(context2);

        var handler1 =
            new RefreshTokenCommandHandler(
                userRepository1,
                refreshTokenRepository1,
                refreshTokenProvider,
                new FakeAccessTokenProvider(),
                context1,
                Options.Create(
                    new RefreshTokenOptions
                    {
                        ExpirationDays = 7
                    }));

        var handler2 =
            new RefreshTokenCommandHandler(
                userRepository2,
                refreshTokenRepository2,
                refreshTokenProvider,
                new FakeAccessTokenProvider(),
                context2,
                Options.Create(
                    new RefreshTokenOptions
                    {
                        ExpirationDays = 7
                    }));

        // Both contexts must load the same old version first
        await refreshTokenRepository1.GetByTokenHashAsync(
            refreshTokenProvider.Hash(rawToken),
            CancellationToken.None);

        await refreshTokenRepository2.GetByTokenHashAsync(
            refreshTokenProvider.Hash(rawToken),
            CancellationToken.None);

        // First request wins
        await handler1.Handle(
            new RefreshTokenCommand(rawToken),
            CancellationToken.None);

        // Act
        var act = () =>
            handler2.Handle(
                new RefreshTokenCommand(rawToken),
                CancellationToken.None);

        // Assert
        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                act);

        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);
    }
}
