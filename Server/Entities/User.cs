namespace Entities;

using ApiContracts;

public class User
{
    public int UserId { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.User;

}
