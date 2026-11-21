using System;

namespace Entities;

public class Post : Content
{
    public required string Title { get; set; }
    public int SubforumId { get; set; }   // FK -> Subforum.SubforumId

    // Navigation properties
    public Subforum Subforum { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
