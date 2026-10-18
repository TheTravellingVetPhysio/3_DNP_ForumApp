namespace Services;

using Entities;
using RepositoryContracts;
using ApiContracts;
using Exceptions;

public class CommentService
{
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository; // direkte, kun for simpelt eksistens-tjek/read funktioner (undgår cyklus med PostService)
    private readonly UserService userService;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        UserService userService)
    {
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
        this.userService = userService;
    }

    // CRUD
    public async Task<Comment> CreateCommentAsync(string body, int userId, int postId, int? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Kommentar må ikke være tom.");
        }

        await userService.EnsureUserExistsAsync(userId);
        await postRepository.GetSingleAsync(postId);

        if (parentCommentId is not null)
        {
            Comment parent = await commentRepository.GetSingleAsync(parentCommentId.Value);
            if (parent.PostId != postId)
            {
                throw new BusinessConflictException("Kommentaren, der svares på, hører ikke til dette postId.");
            }
        }

        Comment comment = new Comment
        {
            Body = body,
            UserId = userId,
            PostId = postId,
            ParentCommentId = parentCommentId
        };

        return await commentRepository.AddAsync(comment);
    }

    public async Task UpdateCommentAsync(int commentId, string newBody)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newBody);

        Comment comment = await commentRepository.GetSingleAsync(commentId);

        TimeSpan timeSinceCreation = DateTimeOffset.UtcNow - comment.Timestamp;
        if (timeSinceCreation > TimeSpan.FromHours(24))
        {
            throw new BusinessConflictException("Kommentaren kan ikke længere redigeres, da den er mere end 24 timer gammel.");
        }

        comment.Body = newBody;

        await commentRepository.UpdateAsync(comment);
    }

    // Getters
    public Task<Comment> GetCommentByIdAsync(int commentId)
        => commentRepository.GetSingleAsync(commentId);

    public Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
        => commentRepository.GetCommentsByPostIdAsync(postId);

    public Task<List<Comment>> GetCommentsByPostIdsAsync(List<int> postIds)
        => commentRepository.GetCommentsByPostIdsAsync(postIds);

    public Task<int> GetCommentCountByPostIdAsync(int postId)
        => commentRepository.GetCommentCountByPostIdAsync(postId);

    // Views
    public Task<List<CommentDetailsDTO>> GetCommentDetailsForPostAsync(int postId)
        => commentRepository.GetCommentDetailsForPostAsync(postId);
}