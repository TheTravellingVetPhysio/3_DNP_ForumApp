
namespace WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Services;
using ApiContracts;

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

    // GET: api/posts/by-user/1
    [HttpGet("by-user/{userId:int}")]
    public async Task<ActionResult<List<PostOverviewDTO>>> GetPostsByUser(
        int userId)
    {
        var posts = await postService.GetPostsOverviewAsync(null, userId, null, null);
        return Ok(posts);
    }

    // GET: api/posts/by-subforum/1
    [HttpGet("by-subforum/{subforumId:int}")]
    public async Task<ActionResult<List<PostOverviewDTO>>> GetPostsBySubforum(
        int subforumId)
    {
        var posts =
            await postService.GetPostsOverviewAsync(null, null, null, subforumId);

        return Ok(posts);
    }

    // POST: api/posts
    [HttpPost]
    public async Task<ActionResult<PostDetailsDTO>> CreatePost(
        [FromBody] CreatePostRequest request)
    {
        var post = await postService.CreatePostAsync(
            request.Title,
            request.Body,
            request.UserId,
            request.SubforumId);

        var result = await postService.GetPostDetailsAsync(post.ContentId);

        return CreatedAtAction(
            nameof(GetPostById),
            new { postId = post.ContentId },
            result);
    }

    // PUT: api/posts/1
     [HttpPut("{postId:int}")]
    public async Task<IActionResult> UpdatePost(
        int postId,
        [FromBody] UpdatePostRequest request)
    {
        await postService.UpdatePostAsync(
            postId,
            request.Title,
            request.Body);

        return NoContent();
    }

    // DELETE: api/posts/1
    [HttpDelete("{postId:int}")]
    public async Task<IActionResult> DeletePost(int postId)
    {
        await postService.DeletePostAsync(postId);
        return NoContent();
    }
}
