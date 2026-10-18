namespace Services;

using Entities;
using RepositoryContracts;
using Exceptions;

public class UserService
{
    private readonly IUserRepository userRepository;

    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    // CRUD
    public async Task<User> CreateUserAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Brugernavn må ikke være tomt.");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            throw new ArgumentException("Adgangskode skal være mindst 6 tegn.");
        }

        bool usernameTaken = await userRepository.UsernameExistsAsync(username);

        if (usernameTaken)
        {
            throw new BusinessConflictException($"Brugernavnet '{username}' er allerede taget.");
        }

        User user = new User { Username = username, Password = password };
        return await userRepository.AddAsync(user);
    }

    // Getters
    public Task<List<User>> GetAllUsersAsync(string? username)
        => userRepository.GetUsersAsync(username);

    public Task<User> GetUserByIdAsync(int userId)
        => userRepository.GetSingleAsync(userId);

    public async Task EnsureUserExistsAsync(int userId)
    {
        await userRepository.GetSingleAsync(userId);
    }
}