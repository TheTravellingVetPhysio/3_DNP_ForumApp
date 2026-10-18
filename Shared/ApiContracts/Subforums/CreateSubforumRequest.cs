namespace ApiContracts;

public class CreateSubforumRequest
{
    public required string Name { get; set; }
    public required int UserId { get; set; }
}