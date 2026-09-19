namespace Services;

using Entities;
using RepositoryContracts;

public class CommentService
{
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository)
    {
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    public async Task<Comment> CreateCommentAsync(string body, int userId, int postId, int? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Kommentar må ikke være tom.");
        }

        await userRepository.GetSingleAsync(userId);
        await postRepository.GetSingleAsync(postId);

        // Domænespecifikke regler
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
}