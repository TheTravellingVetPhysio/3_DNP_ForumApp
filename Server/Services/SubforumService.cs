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

        bool nameTaken = subforumRepository.GetManyAsync()
            .Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (nameTaken)
        {
            throw new InvalidOperationException($"Navnet '{name}' er allerede taget.");
        }

        await userService.EnsureUserExistsAsync(userId);

        Subforum subforum = new Subforum
        {
            Name = name,
            UserId = userId
        };

        return await subforumRepository.AddAsync(subforum);
    }

    public async Task DeleteSubforumAsync(int subforumId)
    {
        await subforumRepository.GetSingleAsync(subforumId);

        int postCount = await postService.GetPostCountBySubforumAsync(subforumId) == 0;
        
        if (postCount == 0)
        {
            await subforumRepository.DeleteAsync(subforumId);
        }
        else
        {
            throw new InvalidOperationException("Subforummet kan ikke slettes, da det indeholder indlæg.");
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