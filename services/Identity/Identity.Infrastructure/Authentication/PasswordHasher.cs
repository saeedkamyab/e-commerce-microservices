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


    public bool Verify(
        string password,
        string passwordHash)
    {
        var result =
            _passwordHasher.VerifyHashedPassword(
                null!,
                passwordHash,
                password);

        return result is
            PasswordVerificationResult.Success or
            PasswordVerificationResult.SuccessRehashNeeded;
    }
}
