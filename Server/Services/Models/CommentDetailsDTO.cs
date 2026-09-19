namespace Services.Models;

using Entities;

public class CommentDetailsDTO
{
    public required int CommentId { get; set; }
    public required string Body { get; set; }
    public required int UserId { get; set; }
    public required string Username { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public int? ParentCommentId { get; set; }
    public required Dictionary<ReactionType, int> ReactionCounts { get; set; }
}