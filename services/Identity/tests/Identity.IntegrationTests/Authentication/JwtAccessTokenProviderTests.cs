using Identity.Application.Abstractions.Authentication;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace Identity.IntegrationTests.Authentication;

public sealed class JwtAccessTokenProviderTests
{
    [Fact]
    public void Create_Should_Create_Jwt_With_User_Claims()
    {
        // Arrange
        var options =
            Options.Create(
                new JwtOptions
                {
                    Issuer = "identity-service",
                    Audience = "ecommerce-api",
                    SecretKey =
                        "THIS_IS_A_TEST_SECRET_KEY_12345678901234567890",
                    ExpirationMinutes = 60
                });

        IAccessTokenProvider provider =
            new JwtAccessTokenProvider(options);

        var user =
            User.CreateLocal(
                Email.Create("jwt@example.com"),
                "hashed-password",
                "Ali",
                "Ahmadi");

        // Act
        var token =
            provider.Create(user);

        // Assert
        Assert.False(
            string.IsNullOrWhiteSpace(token));

        var jwt =
            new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

        Assert.Equal(
            "identity-service",
            jwt.Issuer);

        Assert.Contains(
            "ecommerce-api",
            jwt.Audiences);

        Assert.Equal(
            user.Id.ToString(),
            jwt.Subject);

        Assert.Contains(
            jwt.Claims,
            x =>
                x.Type ==
                JwtRegisteredClaimNames.Email &&
                x.Value ==
                user.Email.Value);
    }
}
