namespace Services;

using Entities;
using RepositoryContracts;
using Exceptions;

public class SubforumService
{
    private readonly ISubforumRepository subforumRepository;
    private readonly UserService userService;
    private readonly IPostRepository postRepository;

    public SubforumService(
        ISubforumRepository subforumRepository,
        UserService userService,
        IPostRepository postRepository
    )
    {
        this.subforumRepository = subforumRepository;
        this.userService = userService;
        this.postRepository = postRepository;
    }

    // CRUD
    public async Task<Subforum> CreateSubforumAsync(string name, int userId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Navnet på subforummet må ikke være tomt.");
        }

        bool nameTaken = await subforumRepository.SubforumNameExistsAsync(name);

        if (nameTaken)
        {
            throw new BusinessConflictException($"Navnet '{name}' er allerede taget.");
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

        int postCount = await postRepository.GetPostCountBySubforumAsync(subforumId);

        if (postCount == 0)
        {
            await subforumRepository.DeleteAsync(subforumId);
        }
        else
        {
            throw new BusinessConflictException("Subforummet kan ikke slettes, da det indeholder indlæg.");
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