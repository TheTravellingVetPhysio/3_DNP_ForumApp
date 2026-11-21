using System;

namespace Entities;

public class Subforum
{
    public int SubforumId { get; set; }
    public required string Name { get; set; }
    public required int UserId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Post> Posts { get; set; } = new List<Post>();

}
