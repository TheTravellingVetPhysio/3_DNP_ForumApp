namespace FileRepositories;

using ApiContracts;
using Entities;
using RepositoryContracts;

public class SubforumFileRepository : FileRepository<Subforum>, ISubforumRepository
{

    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;

    public SubforumFileRepository(
        IPostRepository postRepository,
        ICommentRepository commentRepository)
        : base(
            filePath: "Data/subforums.json",
            getId: s => s.SubforumId,
            setId: (s, id) => s.SubforumId = id)
    {
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
    }

    public async Task<List<SubforumDTO>> GetSubforumOverviewsAsync()
        {
        List<Subforum> subforums = await GetManyAsync();
        List<Post> posts = await postRepository.GetManyAsync();
        List<Comment> comments = await commentRepository.GetManyAsync();

        List<SubforumDTO> result = new();

        foreach (Subforum subforum in subforums)
        {
            List<Post> subforumPosts = posts
                .Where(p => p.SubforumId == subforum.SubforumId)
                .ToList();

            List<int> postIds = subforumPosts
                .Select(p => p.ContentId)
                .ToList();

            List<Comment> subforumComments = comments
                .Where(c => postIds.Contains(c.PostId))
                .ToList();

            DateTimeOffset? latestPost = subforumPosts
                .Max(p => (DateTimeOffset?)p.Timestamp);

            DateTimeOffset? latestComment = subforumComments
                .Max(c => (DateTimeOffset?)c.Timestamp);

            DateTimeOffset? lastActivity = new[]
            {
                latestPost,
                latestComment
            }.Max();

            result.Add(new SubforumDTO
            {
                SubforumId = subforum.SubforumId,
                Name = subforum.Name,
                LastActivityAt = lastActivity
            });
        }

        return result;
    }

    public async Task<bool> SubforumNameExistsAsync(string name)
    {
        List<Subforum> subforums = await GetManyAsync();

        return subforums.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

}