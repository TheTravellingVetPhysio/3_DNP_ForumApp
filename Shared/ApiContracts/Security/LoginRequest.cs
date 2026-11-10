using System.ComponentModel.DataAnnotations;

namespace ApiContracts;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}