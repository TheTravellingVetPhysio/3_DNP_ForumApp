
namespace WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Services;
using ApiContracts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly PostService postService;

    public PostController(PostService postService)
    {
        this.postService = postService;
    }

    // GET: api/posts/1
    [HttpGet("{postId:int}")]
    public async Task<ActionResult<PostDetailsDTO>> GetPostById(int postId)
    {
        var post = await postService.GetPostDetailsAsync(postId);
        return Ok(post);
    }

    // GET: api/posts?title=java&userId=1&username=ann&subforumId=2
    [HttpGet]
    public async Task<ActionResult<List<PostOverviewDTO>>> GetPosts(
        [FromQuery] string? title,
        [FromQuery] int? userId,
        [FromQuery] string? username,
        [FromQuery] int? subforumId)
    {
        var posts = await postService.GetPostsOverviewAsync(
            title,
            userId,
            username,
            subforumId);

        return Ok(posts);
    }

    // POST: api/posts
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostDetailsDTO>> CreatePost(
        [FromBody] CreatePostRequest request)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var post = await postService.CreatePostAsync(
            request.Title,
            request.Body,
            userId,
            request.SubforumId);

        var result = await postService.GetPostDetailsAsync(post.ContentId);

        return CreatedAtAction(
            nameof(GetPostById),
            new { postId = post.ContentId },
            result);
    }

    // PUT: api/posts/1
    [Authorize]
    [HttpPut("{postId:int}")]
    public async Task<IActionResult> UpdatePost(
        int postId,
        [FromBody] UpdatePostRequest request)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        bool isAdmin = User.IsInRole("Admin");
        
        await postService.UpdatePostAsync(
            postId,
            request.Title,
            request.Body,
            userId,
            isAdmin);

        return NoContent();
    }

    // DELETE: api/posts/1
    [Authorize]
    [HttpDelete("{postId:int}")]
    public async Task<IActionResult> DeletePost(int postId)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        bool isAdmin = User.IsInRole("Admin");

        await postService.DeletePostAsync(postId, userId, isAdmin);
        return NoContent();
    }
}
