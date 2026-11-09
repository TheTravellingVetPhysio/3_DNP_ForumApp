namespace ApiContracts;

public class UpdateReactionRequest
{
    public required int ContentId { get; set; }
    public required ContentType ContentType { get; set; }
    public required ReactionType ReactionType { get; set; }
}