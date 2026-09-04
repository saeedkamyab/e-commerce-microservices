using Identity.Application.Abstractions.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace Identity.IntegrationTests.Infrastructure;

public sealed class FakeRefreshTokenProvider
    : IRefreshTokenProvider
{
    public string Generate()
    {
        return Guid.NewGuid().ToString("N");
    }

    public string Hash(string token)
    {
        var bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}
