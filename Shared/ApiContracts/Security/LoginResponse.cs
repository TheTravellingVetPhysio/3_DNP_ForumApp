namespace ApiContracts;

public class LoginResponse
{
    public required string Token { get; set; }
    public int UserId { get; set; }
    public required string Username { get; set; }
    public UserRole Role { get; set; }
}