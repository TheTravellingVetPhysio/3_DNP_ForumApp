using System;

namespace Entities;

public class Content
{
    public int ContentId { get; set; }
    public required string Body { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public int UserId { get; set; }   // FK -> User.UserId
}
