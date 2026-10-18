namespace WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Services;
using ApiContracts;
using Entities;

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
    [HttpPost]
    public async Task<ActionResult<Comment>> CreateComment(
        [FromBody] CreateCommentRequest request)
    {
        var comment = await commentService.CreateCommentAsync(
            request.Body,
            request.UserId,
            request.PostId,
            request.ParentCommentId);

        return StatusCode(
            StatusCodes.Status201Created,
            comment);
    }

    // PUT: api/comments/1
    [HttpPut("{commentId:int}")]
    public async Task<IActionResult> UpdateComment(
        int commentId,
        [FromBody] UpdateCommentRequest request)
    {
        await commentService.UpdateCommentAsync(
            commentId,
            request.Body);

        return NoContent();
    }
}