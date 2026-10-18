namespace ApiContracts;

using System.ComponentModel.DataAnnotations;

public class CreateUserRequest
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = "";
}