namespace ApiContracts.Comments;

using System.ComponentModel.DataAnnotations;

public class UpdateCommentRequest
{
    [Required]
    public string Body { get; set; } = "";
}