using ApiContracts;

namespace BlazorApp.Services;

public interface IUserService
{

    Task<List<UserDTO>> GetAllUsersAsync(string? username = null);

    Task<UserDTO> GetUserByIdAsync(int userId);

    Task<UserDTO> CreateUserAsync(CreateUserRequest request);

}
