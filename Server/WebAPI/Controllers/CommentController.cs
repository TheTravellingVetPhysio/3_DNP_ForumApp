namespace WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Services;
using ApiContracts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/comments")]
public class CommentController : ControllerBase
{
    private readonly CommentService commentService;

    public CommentController(CommentService commentService)
    {
        this.commentService = commentService;
    }

    // GET: api/comments?postId=1&userId=2&username=anna
    [HttpGet]
    public async Task<ActionResult<List<CommentDetailsDTO>>> GetComments(
        [FromQuery] int? postId,
        [FromQuery] int? userId,
        [FromQuery] string? username)
    {
        var comments = await commentService.GetCommentDetailsAsync(
            postId,
            userId,
            username);

        return Ok(comments);
    }

    // POST: api/comments
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateComment(
        [FromBody] CreateCommentRequest request)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        await commentService.CreateCommentAsync(
            request.Body,
            userId,
            request.PostId,
            request.ParentCommentId);

        return StatusCode(
            StatusCodes.Status201Created);
    }

    // PUT: api/comments/1
    [Authorize]
    [HttpPut("{commentId:int}")]
    public async Task<IActionResult> UpdateComment(
        int commentId,
        [FromBody] UpdateCommentRequest request)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        bool isAdmin = User.IsInRole("Admin");
        
        await commentService.UpdateCommentAsync(
            commentId,
            userId,
            isAdmin,
            request.Body);

        return NoContent();
    }
}