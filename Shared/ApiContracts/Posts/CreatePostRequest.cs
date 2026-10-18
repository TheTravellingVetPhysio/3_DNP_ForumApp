
namespace ApiContracts;

using System.ComponentModel.DataAnnotations;

public class CreatePostRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    public string Body { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int SubforumId { get; set; }
}
