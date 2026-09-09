using System;

namespace Entities;

public class Subforum
{
    public int SubforumId { get; set; }
    public required string Name { get; set; }
    public required int UserId { get; set; }
}
