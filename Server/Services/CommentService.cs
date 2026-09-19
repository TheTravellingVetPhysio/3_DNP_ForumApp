namespace Services;

using Entities;
using RepositoryContracts;

public class CommentService
{
    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        IReactionRepository reactionRepository)
    {
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
        this.userRepository = userRepository;
        this.reactionRepository = reactionRepository;
    }

    // CRUD
    public async Task<Comment> CreateCommentAsync(string body, int userId, int postId, int? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Kommentar må ikke være tom.");
        }

        await userRepository.GetSingleAsync(userId);
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
    

    // Views
    public async Task<List<CommentDetailsDTO>> GetCommentDetailsForPostAsync(int postId)
    {
        List<Comment> comments = commentRepository.GetManyAsync()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.Timestamp)
            .ToList();

        List<CommentDetailsDTO> result = new();
        foreach (Comment comment in comments)
        {
            User commentUser = await userRepository.GetSingleAsync(comment.UserId);

            Dictionary<ReactionType, int> reactionCounts = reactionRepository.GetManyAsync()
                .Where(r => r.ContentId == comment.ContentId && r.ContentType == ContentType.Comment)
                .GroupBy(r => r.ReactionType)
                .ToDictionary(g => g.Key, g => g.Count());

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