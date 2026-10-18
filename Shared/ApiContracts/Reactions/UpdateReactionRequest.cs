namespace ApiContracts;

using Entities;

public class UpdateReactionRequest
{
    public required int UserId { get; set; }
    public required int ContentId { get; set; }
    public required ContentType ContentType { get; set; }
    public required ReactionType ReactionType { get; set; }
}