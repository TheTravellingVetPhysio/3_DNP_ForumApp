namespace ApiContracts;

using Entities;

public class DeleteReactionRequest
{
    public required int UserId { get; set; }
    public required int ContentId { get; set; }
    public required ContentType ContentType { get; set; }
}
