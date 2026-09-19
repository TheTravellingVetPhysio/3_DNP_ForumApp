namespace Services;

using Entities;
using RepositoryContracts;

public class SubforumService
{
    private readonly ISubforumRepository subforumRepository;
    private readonly UserService userService;
    private readonly PostService postService;

    public SubforumService(
        ISubforumRepository subforumRepository,
        UserService userService,
        PostService postService
    )
    {
        this.subforumRepository = subforumRepository;
        this.userService = userService;
        this.postService = postService;
    }

    // CRUD
    public async Task<Subforum> CreateSubforumAsync(string name, int userId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Navnet på subforummet må ikke være tomt.");
        }

        bool subforumnameTaken = subforumRepository.GetManyAsync()
            .Any(u => u.Subforumname.Equals(subforumname, StringComparison.OrdinalIgnoreCase));

        if (subforumnameTaken)
        {
            throw new InvalidOperationException($"Navnet '{subforumname}' er allerede taget.");
        }

        await userService.EnsureUserExistsAsync(userId);

        Subforum subforum = new Subforum
        {
            Name = subforumname,
            UserId = userId
        };

        return await subforumRepository.AddAsync(subforum);
    }

    public async Task<Subforum> DeleteSubforumAsync(int subforumId)
    {
        Subforum subforum = GetSubforumByIdAsync(subforumId);

        if (postService.GetPostCountBySubforumAsync(subforumId) == 0)
        {
            await subforumRepository.DeleteAsync(subforum);
        }

    }

    // Getters
    public async Task<Subforum> GetSubforumByIdAsync(int subforumId)
    {
        return await subforumRepository.GetSingleAsync(subforumId);
    }

    public async Task EnsureSubforumExistsAsync(int subforumId)
    {
        await subforumRepository.GetSingleAsync(subforumId);
    }
}