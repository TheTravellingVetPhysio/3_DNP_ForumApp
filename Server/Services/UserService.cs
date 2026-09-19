namespace Services;

using Entities;
using RepositoryContracts;

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

        bool usernameTaken = userRepository.GetManyAsync()
            .Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (usernameTaken)
        {
            throw new InvalidOperationException($"Brugernavnet '{username}' er allerede taget.");
        }

        User user = new User { Username = username, Password = password };
        return await userRepository.AddAsync(user);
    }
}