using Identity.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.Authentication;

internal sealed class PasswordHasher
    : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher =
        new();

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(
            null!,
            password);
    }
}
