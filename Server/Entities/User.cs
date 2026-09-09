using System;

namespace Entities;

public class User
{
    public int UserId { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}
