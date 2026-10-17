namespace Services;

using Entities;
using RepositoryContracts;

public class ReactionService
{
    private readonly IReactionRepository reactionRepository;
    private readonly IUserRepository userRepository;
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;

    public ReactionService(
        IReactionRepository reactionRepository,
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        this.reactionRepository = reactionRepository;
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
    }

    // CRUD
    public async Task<Reaction> AddReactionAsync(int userId, int contentId, ContentType contentType, ReactionType reactionType)
    {
        await userRepository.GetSingleAsync(userId);
        await EnsureContentExistsAsync(contentId, contentType);

        Reaction? existing =
            await reactionRepository.GetReactionByUserAndContentAsync(
                userId, contentId, contentType);
        if (existing is not null)
        {
            throw new InvalidOperationException(
                "Brugeren har allerede reageret på dette indhold. Brug UpdateReactionAsync for at ændre reaktionen.");
        }

        Reaction reaction = new Reaction
        {
            UserId = userId,
            ContentId = contentId,
            ContentType = contentType,
            ReactionType = reactionType
        };

        return await reactionRepository.AddAsync(reaction);
    }

    public async Task UpdateReactionAsync(int userId, int contentId, ContentType contentType, ReactionType newReactionType)
    {
        Reaction? existing =
            await reactionRepository.GetReactionByUserAndContentAsync(
                userId, contentId, contentType);
        if (existing is null)
        {
            throw new InvalidOperationException("Brugeren har ikke reageret på dette indhold endnu.");
        }

        existing.ReactionType = newReactionType;
        await reactionRepository.UpdateAsync(existing);
    }

    public async Task RemoveReactionAsync(int userId, int contentId, ContentType contentType)
    {
        Reaction? existing =
            await reactionRepository.GetReactionByUserAndContentAsync(
                userId, contentId, contentType);
        if (existing is null)
        {
            throw new InvalidOperationException("Brugeren har ikke reageret på dette indhold.");
        }

        await reactionRepository.DeleteAsync(existing.ReactionId);
    }

    // Getters
    public Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(int contentId, ContentType contentType)
        => reactionRepository.GetReactionCountsAsync(contentId, contentType);

    // Hjælpemetoder
    private async Task EnsureContentExistsAsync(int contentId, ContentType contentType)
    {
        if (contentType == ContentType.Post)
        {
            await postRepository.GetSingleAsync(contentId);
        }
        else
        {
            await commentRepository.GetSingleAsync(contentId);
        }
    }
}