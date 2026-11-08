using Entities;
using Microsoft.AspNetCore.Identity;

namespace Services;

public class AspNetPasswordHasher : IPasswordHasher
{
    private readonly IPasswordHasher<User> passwordHasher = new PasswordHasher<User>();

    public string HashPassword(User user, string password)
    {
        return passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string hashedPassword, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, hashedPassword, password);

        return result != PasswordVerificationResult.Failed;
    }
}