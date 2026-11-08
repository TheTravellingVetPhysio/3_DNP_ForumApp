using ApiContracts;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService userService;
    private readonly JwtTokenService jwtTokenService;

    public AuthController(
        UserService userService,
        JwtTokenService jwtTokenService)
    {
        this.userService = userService;
        this.jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request)
    {
        var user = await userService.LoginAsync(
            request.Username,
            request.Password);

        string token = jwtTokenService.GenerateToken(user);

        return Ok(new LoginResponse
        {
            Token = token,
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role
        });
    }
}