namespace Services.Models;

public class CommentDetailsDTO
{
    public required int CommentId { get; set; }
    public required string Body { get; set; }
    public required int UserId { get; set; }
    public required string Username { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public int? ParentCommentId { get; set; }
}