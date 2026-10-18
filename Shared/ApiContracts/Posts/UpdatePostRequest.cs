
namespace ApiContracts;

using System.ComponentModel.DataAnnotations;

public class UpdatePostRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    public string Body { get; set; } = "";
}
