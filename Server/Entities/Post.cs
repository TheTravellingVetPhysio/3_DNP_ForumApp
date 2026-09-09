using System;

namespace Entities;

public class Post : Content
{
    public required string Title { get; set; }
    public required int SubforumId { get; set; }   // FK -> Subforum.SubforumId
}
