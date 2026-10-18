namespace ApiContracts;

public class PostOverviewDTO
{
    public required int PostId { get; set; }
    public required string Title { get; set; }
    public required int UserId { get; set; }
    public required string Username { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset LastActivityAt { get; set; }
}