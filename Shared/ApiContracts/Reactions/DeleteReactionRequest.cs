namespace ApiContracts;

public class DeleteReactionRequest
{
    public required int ContentId { get; set; }
    public required ContentType ContentType { get; set; }
}
