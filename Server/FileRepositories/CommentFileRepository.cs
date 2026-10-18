namespace FileRepositories;

using Entities;
using RepositoryContracts;
using ApiContracts;
public class CommentFileRepository : FileRepository<Comment>, ICommentRepository
{
    private readonly IUserRepository userRepository;
    private readonly IReactionRepository reactionRepository;

    public CommentFileRepository(
        IUserRepository userRepository,
        IReactionRepository reactionRepository)
        : base(
            filePath: "Data/comments.json",
            getId: c => c.ContentId,
            setId: (c, id) => c.ContentId = id)
    {
        this.userRepository = userRepository;
        this.reactionRepository = reactionRepository;
    }

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        List<Comment> comments = await GetManyAsync();

        return comments
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.Timestamp)
            .ToList();
    }

    public async Task<List<Comment>> GetCommentsByPostIdsAsync(List<int> postIds)
    {
        List<Comment> comments = await GetManyAsync();

        return comments
            .Where(c => postIds.Contains(c.PostId))
            .OrderBy(c => c.Timestamp)
            .ToList();
    }

    public async Task<int> GetCommentCountByPostIdAsync(int postId)
    {
        List<Comment> comments = await GetManyAsync();

        return comments.Count(c => c.PostId == postId);
    }

    public async Task<List<CommentDetailsDTO>> GetCommentDetailsAsync(
        int? postId,
        int? userId,
        string? username)
    {
        List<Comment> comments = await GetManyAsync();

        if (postId.HasValue)
            comments = comments.Where(c => c.PostId == postId.Value).ToList();

        if (userId.HasValue)
            comments = comments.Where(c => c.UserId == userId.Value).ToList();

        if (!string.IsNullOrWhiteSpace(username))
        {
            List<User> users = await userRepository.GetManyAsync();

            var matchingUserIds = users
                .Where(u => u.Username.Contains(username,
                    StringComparison.OrdinalIgnoreCase))
                .Select(u => u.UserId)
                .ToHashSet();

            comments = comments
                .Where(c => matchingUserIds.Contains(c.UserId))
                .ToList();
        }

        comments = comments.OrderBy(c => c.Timestamp).ToList();

        List<CommentDetailsDTO> result = new();

        foreach (Comment comment in comments)
        {
            User user = await userRepository.GetSingleAsync(comment.UserId);

            Dictionary<ReactionType, int> reactionCounts =
                await reactionRepository.GetReactionCountsAsync(
                    comment.ContentId, ContentType.Comment);

            result.Add(new CommentDetailsDTO
            {
                CommentId = comment.ContentId,
                Body = comment.Body,
                UserId = comment.UserId,
                Username = user.Username,
                CreatedAt = comment.Timestamp,
                ParentCommentId = comment.ParentCommentId,
                ReactionCounts = reactionCounts
            });
        }

        return result;
    }
}