namespace WebAPI.Controllers;

using System.Security.Claims;
using ApiContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using WebAPI.Mappers;

[ApiController]
[Route("api/subforums")]
public class SubforumController : ControllerBase
{
    private readonly SubforumService subforumService;

    public SubforumController(SubforumService subforumService)
    {
        this.subforumService = subforumService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SubforumDTO>>> GetAllSubforums()
    {
        return Ok(await subforumService.GetSubforumOverviewAsync());
    }

    [HttpGet("{subforumId:int}")]
    public async Task<ActionResult<SubforumDTO>> GetSubforumById(
        int subforumId)
    {
        var subforum =
            await subforumService.GetSubforumByIdAsync(subforumId);

        return Ok(SubforumMapper.ToDTO(subforum));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateSubforum(
        [FromBody] CreateSubforumRequest request)
    {
        int userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var subforum = await subforumService.CreateSubforumAsync(
            request.Name,
            userId);

        var result = SubforumMapper.ToDTO(subforum);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{subforumId:int}")]
    public async Task<IActionResult> DeleteSubforum(int subforumId)
    {
        await subforumService.DeleteSubforumAsync(subforumId);

        return NoContent();
    }
}
