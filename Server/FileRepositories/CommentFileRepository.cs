namespace FileRepositories;

using Entities;
using RepositoryContracts;
using Models;

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

    public async Task<List<CommentDetailsDTO>> GetCommentDetailsForPostAsync(
    int postId)
    {
        List<Comment> comments = await GetCommentsByPostIdAsync(postId);
        List<CommentDetailsDTO> result = new();

        foreach (Comment comment in comments)
        {
            User user = await userRepository.GetSingleAsync(comment.UserId);

            Dictionary<ReactionType, int> reactionCounts =
                await reactionRepository.GetReactionCountsAsync(
                    comment.ContentId,
                    ContentType.Comment);

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