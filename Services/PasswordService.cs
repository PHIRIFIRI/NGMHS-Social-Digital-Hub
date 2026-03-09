using Microsoft.AspNetCore.Identity;
using NGMHS.Models;

namespace NGMHS.Services;

public class PasswordService
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public PasswordService(IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    // Hash passwords with ASP.NET Core's PBKDF2 implementation.
    public string HashPassword(User user, string plainPassword)
    {
        return _passwordHasher.HashPassword(user, plainPassword);
    }

    public bool VerifyPassword(User user, string plainPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, plainPassword);
        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
