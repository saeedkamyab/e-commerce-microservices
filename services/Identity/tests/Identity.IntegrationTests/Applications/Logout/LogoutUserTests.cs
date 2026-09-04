using Identity.Application.Abstractions.Authentication;
using Identity.Application.Users.Logout;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Identity.IntegrationTests.Applications.Logout;

[Collection(IdentityDatabaseCollection.Name)]
public class LogoutUserTests
{

    private readonly IdentityDatabaseFixture _fixture;

    public LogoutUserTests(IdentityDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    [Fact]
    public async Task Handle_With_Valid_RefreshToken_Should_Revoke_Entire_Family()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.RefreshTokens.ExecuteDeleteAsync();
        await dbContext.Users.ExecuteDeleteAsync();

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        var user =
            User.Create(
                Email.Create("logout@example.com"),
                "hashed-password",
                "Ali",
                "Ahmadi");

        dbContext.Users.Add(user);

        var familyId =
            Guid.NewGuid();

        var rawTokenA =
            refreshTokenProvider.Generate();

        var tokenA =
            Identity.Domain.Entities.RefreshToken.Create(
                user.Id,
                familyId,
                refreshTokenProvider.Hash(rawTokenA),
                DateTime.UtcNow.AddDays(7));

        var rawTokenB =
            refreshTokenProvider.Generate();

        var tokenB =
            Identity.Domain.Entities.RefreshToken.Create(
                user.Id,
                familyId,
                refreshTokenProvider.Hash(rawTokenB),
                DateTime.UtcNow.AddDays(7));

        tokenA.ReplaceWith(tokenB.Id);

        dbContext.RefreshTokens.AddRange(
            tokenA,
            tokenB);

        await dbContext.SaveChangesAsync();

        var repository =
            new RefreshTokenRepository(dbContext);

        var handler =
            new LogoutCommandHandler(
                refreshTokenProvider,
                repository,
                dbContext);

        // Act
        await handler.Handle(
            new LogoutCommand(rawTokenB),
            CancellationToken.None);

        // Assert
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
    public async Task Handle_When_RefreshToken_Does_Not_Exist_Should_Do_Nothing()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.RefreshTokens.ExecuteDeleteAsync();

        IRefreshTokenProvider refreshTokenProvider =
            new RefreshTokenProvider();

        var repository =
            new RefreshTokenRepository(dbContext);

        var handler =
            new LogoutCommandHandler(
                refreshTokenProvider,
                repository,
                dbContext);

        var unknownToken =
            refreshTokenProvider.Generate();

        await handler.Handle(
            new LogoutCommand(unknownToken),
            CancellationToken.None);

        Assert.Equal(
            0,
            await dbContext.RefreshTokens.CountAsync());
    }

}
