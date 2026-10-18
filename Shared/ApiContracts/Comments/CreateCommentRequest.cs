namespace ApiContracts;

using System.ComponentModel.DataAnnotations;

public class CreateCommentRequest
{
    [Required]
    public string Body { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int PostId { get; set; }

    public int? ParentCommentId { get; set; }
}