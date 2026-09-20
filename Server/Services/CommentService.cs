namespace Services;

using Entities;
using RepositoryContracts;
using Services.Models;

public class CommentService
{
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository; // direkte, kun for simpelt eksistens-tjek (undgår cyklus med PostService)
    private readonly UserService userService;
    private readonly ReactionService reactionService;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        UserService userService,
        ReactionService reactionService)
    {
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
        this.userService = userService;
        this.reactionService = reactionService;
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
                throw new InvalidOperationException("Kommentaren, der svares på, hører ikke til dette postId.");
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
            throw new InvalidOperationException("Kommentaren kan ikke længere redigeres, da den er mere end 24 timer gammel.");
        }

        comment.Body = newBody;

        await commentRepository.UpdateAsync(comment);
    }

    // Getters
    public async Task<Comment> GetCommentByIdAsync(int commentId)
    {
        return await commentRepository.GetSingleAsync(commentId);
    }

    public Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        List<Comment> comments = commentRepository.GetManyAsync()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.Timestamp)
            .ToList();

        return Task.FromResult(comments);
    }

    public Task<List<Comment>> GetCommentsByPostIdsAsync(List<int> postIds)
    {
        List<Comment> comments = commentRepository.GetManyAsync()
            .Where(c => postIds.Contains(c.PostId))
            .OrderBy(c => c.Timestamp)
            .ToList();

        return Task.FromResult(comments);
    }

    public Task<int> GetCommentCountByPostIdAsync(int postId)
    {
        int commentCount = commentRepository.GetManyAsync()
            .Where(c => c.PostId == postId)
            .Count();

        return Task.FromResult(commentCount);
    }

    // Views
    public async Task<List<CommentDetailsDTO>> GetCommentDetailsForPostAsync(int postId)
    {
        List<Comment> comments = await GetCommentsByPostIdAsync(postId);

        List<CommentDetailsDTO> result = new();
        foreach (Comment comment in comments)
        {
            User commentUser = await userService.GetUserByIdAsync(comment.UserId);

            Dictionary<ReactionType, int> reactionCounts =
                await reactionService.GetReactionCountsAsync(comment.ContentId, ContentType.Comment);

            result.Add(new CommentDetailsDTO
            {
                CommentId = comment.ContentId,
                Body = comment.Body,
                UserId = comment.UserId,
                Username = commentUser.Username,
                CreatedAt = comment.Timestamp,
                ParentCommentId = comment.ParentCommentId,
                ReactionCounts = reactionCounts
            });
        }

        return result;
    }
}