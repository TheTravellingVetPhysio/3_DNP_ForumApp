namespace ApiContracts;

public class SubforumDTO
{
    public required int SubforumId { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
}