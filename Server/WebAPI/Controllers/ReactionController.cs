namespace WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Services;
using ApiContracts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/reactions")]
public class ReactionController : ControllerBase
{
    private readonly ReactionService reactionService;

    public ReactionController(ReactionService reactionService)
    {
        this.reactionService = reactionService;
    }

    // POST: api/reactions
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddReaction(
        [FromBody] CreateReactionRequest request)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await reactionService.AddReactionAsync(
            userId,
            request.ContentId,
            request.ContentType,
            request.ReactionType);

        return StatusCode(StatusCodes.Status201Created);
    }

    // PUT: api/reactions
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateReaction(
        [FromBody] UpdateReactionRequest request)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await reactionService.UpdateReactionAsync(
            userId,
            request.ContentId,
            request.ContentType,
            request.ReactionType);

        return NoContent();
    }

    // DELETE: api/reactions
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RemoveReaction(
        [FromBody] DeleteReactionRequest request)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
        await reactionService.RemoveReactionAsync(
            userId,
            request.ContentId,
            request.ContentType);

        return NoContent();
    }


    // GET: api/reactions/counts?contentId=1&contentType=Post
    [HttpGet("counts")]
    public async Task<ActionResult<Dictionary<ReactionType, int>>>
        GetReactionCounts(
            [FromQuery] int contentId,
            [FromQuery] ContentType contentType)
    {
        var counts = await reactionService.GetReactionCountsAsync(
            contentId,
            contentType);

        return Ok(counts);
    }

    // GET: api/reactions/user?userId=1&contentId=1&contentType=Post
    [HttpGet("user")]
    public async Task<ActionResult<ReactionType?>> GetUserReaction(
        [FromQuery] int contentId,
        [FromQuery] ContentType contentType)
    {

        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var reaction = await reactionService.GetUserReactionAsync(
            userId,
            contentId,
            contentType);

        return new JsonResult(reaction?.ReactionType)
        {
            StatusCode = StatusCodes.Status200OK
        };
    }
}
