using Entities;

namespace Services;

public interface IPasswordHasher
{
    string HashPassword(User user, string password);

    bool VerifyPassword(User user, string hashedPassword, string password);
}