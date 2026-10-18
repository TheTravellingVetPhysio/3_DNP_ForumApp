namespace WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Services;
using ApiContracts;
using Entities;

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
    [HttpPost]
    public async Task<IActionResult> AddReaction(
        [FromBody] CreateReactionRequest request)
    {
        var reaction = await reactionService.AddReactionAsync(
            request.UserId,
            request.ContentId,
            request.ContentType,
            request.ReactionType);

        return StatusCode(StatusCodes.Status201Created, reaction);
    }

    // PUT: api/reactions
    [HttpPut]
    public async Task<IActionResult> UpdateReaction(
        [FromBody] UpdateReactionRequest request)
    {
        await reactionService.UpdateReactionAsync(
            request.UserId,
            request.ContentId,
            request.ContentType,
            request.ReactionType);

        return NoContent();
    }

    // DELETE: api/reactions
    [HttpDelete]
    public async Task<IActionResult> RemoveReaction(
        [FromBody] DeleteReactionRequest request)
    {
        await reactionService.RemoveReactionAsync(
            request.UserId,
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
}
