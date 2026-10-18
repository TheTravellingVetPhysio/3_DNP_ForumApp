
namespace WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Services;
using Mappers;
using ApiContracts;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService userService;

    public UserController(UserService userService)
    {
        this.userService = userService;
    }

    // GET: api/users?username=ann
    [HttpGet]
    public async Task<ActionResult<List<UserDTO>>> GetAllUsers(
        [FromQuery] string? username)
    {
        var users = await userService.GetAllUsersAsync(username);

        var result = users.Select(UserMapper.ToDTO).ToList();

        return Ok(result);
    }

    // GET: api/users/1
    [HttpGet("{userId:int}")]
    public async Task<ActionResult<UserDTO>> GetUserById(int userId)
    {
        var user = await userService.GetUserByIdAsync(userId);

        return Ok(UserMapper.ToDTO(user));
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<UserDTO>> CreateUser(
        [FromBody] CreateUserRequest request)
    {
        var user = await userService.CreateUserAsync(
            request.Username,
            request.Password);

        UserDTO result = UserMapper.ToDTO(user);

        return CreatedAtAction(
            nameof(GetUserById),
            new { userId = user.UserId },
            result);
    }
}
