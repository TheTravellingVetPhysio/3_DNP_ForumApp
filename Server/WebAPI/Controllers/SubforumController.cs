namespace WebAPI.Controllers;

using ApiContracts;
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
        var subforums = await subforumService.GetAllSubforumsAsync();

        return Ok(SubforumMapper.ToDTOList(subforums));
    }

    [HttpGet("{subforumId:int}")]
    public async Task<ActionResult<SubforumDTO>> GetSubforumById(
        int subforumId)
    {
        var subforum =
            await subforumService.GetSubforumByIdAsync(subforumId);

        return Ok(SubforumMapper.ToDTO(subforum));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubforum(
        [FromBody] CreateSubforumRequest request)
    {
        var subforum = await subforumService.CreateSubforumAsync(
            request.Name,
            request.UserId);

        var result = SubforumMapper.ToDTO(subforum);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpDelete("{subforumId:int}")]
    public async Task<IActionResult> DeleteSubforum(int subforumId)
    {
        await subforumService.DeleteSubforumAsync(subforumId);

        return NoContent();
    }
}
