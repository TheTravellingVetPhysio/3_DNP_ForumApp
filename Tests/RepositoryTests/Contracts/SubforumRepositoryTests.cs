namespace RepositoryTests.Contracts;

using ApiContracts;
using Entities;
using Exceptions;
using RepositoryContracts;
using RepositoryTests.Factories;
using Xunit;

public class SubforumRepositoryTests : IDisposable
{
    private readonly IRepositoryFactory factory;
    private readonly ISubforumRepository repository;

    public SubforumRepositoryTests()
    {
        factory = new EfcRepositoryFactory();
        repository = factory.SubforumRepository;
    }

    public void Dispose()
    {
        factory.Dispose();
    }

    private static async Task<Subforum> CreateSubforumAsync(
        IRepositoryFactory factory,
        string name = "Test subforum")
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = $"user_{Guid.NewGuid():N}",
            PasswordHash = "testhash"
        });

        return await factory.SubforumRepository.AddAsync(new Subforum
        {
            Name = name,
            UserId = user.UserId
        });
    }

    private static async Task<Post> CreatePostAsync(
        IRepositoryFactory factory,
        int subforumId,
        DateTimeOffset? timestamp = null)
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = $"user_{Guid.NewGuid():N}",
            PasswordHash = "testhash"
        });

        return await factory.PostRepository.AddAsync(new Post
        {
            Title = "Test post",
            Body = "Test body",
            UserId = user.UserId,
            SubforumId = subforumId,
            Timestamp = timestamp ?? DateTimeOffset.UtcNow
        });
    }

    private static async Task<Comment> CreateCommentAsync(
        IRepositoryFactory factory,
        int postId,
        DateTimeOffset? timestamp = null)
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = $"user_{Guid.NewGuid():N}",
            PasswordHash = "testhash"
        });

        return await factory.CommentRepository.AddAsync(new Comment
        {
            Body = "Test comment",
            UserId = user.UserId,
            PostId = postId,
            Timestamp = timestamp ?? DateTimeOffset.UtcNow
        });
    }

    // CRUD

    [Fact]
    public async Task AddAsync_ShouldAssignId()
    {
        var subforum = await CreateSubforumAsync(factory);

        Assert.True(subforum.SubforumId > 0);
    }

    [Fact]
    public async Task AddAsync_ShouldAssignUniqueIds()
    {
        var subforum1 = await CreateSubforumAsync(factory, "First");
        var subforum2 = await CreateSubforumAsync(factory, "Second");

        Assert.NotEqual(subforum1.SubforumId, subforum2.SubforumId);
    }

    [Fact]
    public async Task AddAsync_ShouldStartIdsAtOne()
    {
        var subforum = await CreateSubforumAsync(factory);

        Assert.Equal(1, subforum.SubforumId);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldReturnSubforum()
    {
        var subforum = await CreateSubforumAsync(factory);

        var result = await repository.GetSingleAsync(subforum.SubforumId);

        Assert.Equal(subforum.SubforumId, result.SubforumId);
        Assert.Equal(subforum.Name, result.Name);
        Assert.Equal(subforum.UserId, result.UserId);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenSubforumDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(999));
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnAllSubforums()
    {
        await CreateSubforumAsync(factory, "First");
        await CreateSubforumAsync(factory, "Second");
        await CreateSubforumAsync(factory, "Third");

        var result = await repository.GetManyAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnEmptyList_WhenNoSubforumsExist()
    {
        var result = await repository.GetManyAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSubforum()
    {
        var subforum = await CreateSubforumAsync(factory);

        subforum.Name = "Updated subforum";

        await repository.UpdateAsync(subforum);

        var result = await repository.GetSingleAsync(subforum.SubforumId);

        Assert.Equal("Updated subforum", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveId()
    {
        var subforum = await CreateSubforumAsync(factory);
        int originalId = subforum.SubforumId;

        subforum.Name = "Updated subforum";

        await repository.UpdateAsync(subforum);

        var result = await repository.GetSingleAsync(originalId);

        Assert.Equal(originalId, result.SubforumId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotAffectOtherSubforums()
    {
        var subforum1 = await CreateSubforumAsync(factory, "First");
        var subforum2 = await CreateSubforumAsync(factory, "Second");

        subforum1.Name = "Updated first";

        await repository.UpdateAsync(subforum1);

        var result = await repository.GetSingleAsync(subforum2.SubforumId);

        Assert.Equal("Second", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenSubforumDoesNotExist()
    {
        var subforum = new Subforum
        {
            SubforumId = 999,
            Name = "Missing subforum",
            UserId = 1
        };

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.UpdateAsync(subforum));
    }

    [Fact]
    public async Task DeleteAsync_ShouldMakeSubforumUnavailable()
    {
        var subforum = await CreateSubforumAsync(factory);

        await repository.DeleteAsync(subforum.SubforumId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(subforum.SubforumId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotDeleteOtherSubforums()
    {
        var subforum1 = await CreateSubforumAsync(factory, "First");
        var subforum2 = await CreateSubforumAsync(factory, "Second");

        await repository.DeleteAsync(subforum1.SubforumId);

        var result = await repository.GetSingleAsync(subforum2.SubforumId);

        Assert.Equal(subforum2.SubforumId, result.SubforumId);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenSubforumDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenDeletingSameSubforumTwice()
    {
        var subforum = await CreateSubforumAsync(factory);

        await repository.DeleteAsync(subforum.SubforumId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.DeleteAsync(subforum.SubforumId));
    }

    // SubforumNameExistsAsync

    [Fact]
    public async Task SubforumNameExistsAsync_ShouldReturnTrue_WhenNameExists()
    {
        await CreateSubforumAsync(factory, "Java");

        var result = await repository.SubforumNameExistsAsync("Java");

        Assert.True(result);
    }

    [Fact]
    public async Task SubforumNameExistsAsync_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        await CreateSubforumAsync(factory, "Java");

        var result = await repository.SubforumNameExistsAsync("CSharp");

        Assert.False(result);
    }

    [Fact]
    public async Task SubforumNameExistsAsync_ShouldIgnoreCase()
    {
        await CreateSubforumAsync(factory, "Java");

        var result = await repository.SubforumNameExistsAsync("JAVA");

        Assert.True(result);
    }

    [Fact]
    public async Task SubforumNameExistsAsync_ShouldReturnFalse_WhenNoSubforumsExist()
    {
        var result = await repository.SubforumNameExistsAsync("Java");

        Assert.False(result);
    }

    // GetSubforumOverviewsAsync

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldReturnAllSubforums()
    {
        await CreateSubforumAsync(factory, "Java");
        await CreateSubforumAsync(factory, "CSharp");
        await CreateSubforumAsync(factory, "Python");

        var result = await repository.GetSubforumOverviewsAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldReturnEmptyList_WhenNoSubforumsExist()
    {
        var result = await repository.GetSubforumOverviewsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldMapSubforumProperties()
    {
        var subforum = await CreateSubforumAsync(factory, "Java");

        var result = await repository.GetSubforumOverviewsAsync();

        var dto = Assert.Single(result);

        Assert.Equal(subforum.SubforumId, dto.SubforumId);
        Assert.Equal("Java", dto.Name);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldReturnNullLastActivity_WhenSubforumHasNoActivity()
    {
        await CreateSubforumAsync(factory, "Empty");

        var result = await repository.GetSubforumOverviewsAsync();

        var dto = Assert.Single(result);

        Assert.Null(dto.LastActivityAt);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldUseLatestPostAsLastActivity()
    {
        var subforum = await CreateSubforumAsync(factory, "Java");

        var earlier = DateTimeOffset.UtcNow.AddDays(-2);
        var later = DateTimeOffset.UtcNow.AddDays(-1);

        await CreatePostAsync(factory, subforum.SubforumId, earlier);
        await CreatePostAsync(factory, subforum.SubforumId, later);

        var result = await repository.GetSubforumOverviewsAsync();

        var dto = Assert.Single(result);

        Assert.Equal(later, dto.LastActivityAt);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldUseLatestCommentAsLastActivity()
    {
        var subforum = await CreateSubforumAsync(factory, "Java");

        var post = await CreatePostAsync(
            factory,
            subforum.SubforumId,
            DateTimeOffset.UtcNow.AddDays(-3));

        var latestComment = DateTimeOffset.UtcNow.AddDays(-1);

        await CreateCommentAsync(
            factory,
            post.ContentId,
            DateTimeOffset.UtcNow.AddDays(-2));

        await CreateCommentAsync(
            factory,
            post.ContentId,
            latestComment);

        var result = await repository.GetSubforumOverviewsAsync();

        var dto = Assert.Single(result);

        Assert.Equal(latestComment, dto.LastActivityAt);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldUseLatestActivityBetweenPostsAndComments()
    {
        var subforum = await CreateSubforumAsync(factory, "Java");

        var post = await CreatePostAsync(
            factory,
            subforum.SubforumId,
            DateTimeOffset.UtcNow.AddDays(-3));

        var latestComment = DateTimeOffset.UtcNow.AddDays(-2);
        var latestPost = DateTimeOffset.UtcNow.AddDays(-1);

        await CreateCommentAsync(
            factory,
            post.ContentId,
            latestComment);

        await CreatePostAsync(
            factory,
            subforum.SubforumId,
            latestPost);

        var result = await repository.GetSubforumOverviewsAsync();

        var dto = Assert.Single(result);

        Assert.Equal(latestPost, dto.LastActivityAt);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldIgnoreActivityFromOtherSubforums()
    {
        var subforum1 = await CreateSubforumAsync(factory, "Java");
        var subforum2 = await CreateSubforumAsync(factory, "CSharp");

        var post1 = await CreatePostAsync(
            factory,
            subforum1.SubforumId,
            DateTimeOffset.UtcNow.AddDays(-3));

        await CreatePostAsync(
            factory,
            subforum2.SubforumId,
            DateTimeOffset.UtcNow);

        var commentTime = DateTimeOffset.UtcNow.AddDays(-2);

        await CreateCommentAsync(
            factory,
            post1.ContentId,
            commentTime);

        var result = await repository.GetSubforumOverviewsAsync();

        var javaDto = result.Single(s => s.SubforumId == subforum1.SubforumId);
        var csharpDto = result.Single(s => s.SubforumId == subforum2.SubforumId);

        Assert.Equal(commentTime, javaDto.LastActivityAt);
        Assert.NotEqual(commentTime, csharpDto.LastActivityAt);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldReturnCorrectActivityForMultipleSubforums()
    {
        var java = await CreateSubforumAsync(factory, "Java");
        var csharp = await CreateSubforumAsync(factory, "CSharp");
        var python = await CreateSubforumAsync(factory, "Python");

        var javaPostTime = DateTimeOffset.UtcNow.AddDays(-3);
        var csharpPostTime = DateTimeOffset.UtcNow.AddDays(-2);
        var latestCommentTime = DateTimeOffset.UtcNow.AddDays(-1);

        var javaPost = await CreatePostAsync(
            factory, java.SubforumId, javaPostTime);

        await CreatePostAsync(
            factory, csharp.SubforumId, csharpPostTime);

        await CreateCommentAsync(
            factory,
            javaPost.ContentId,
            latestCommentTime);

        var result = await repository.GetSubforumOverviewsAsync();

        var javaDto = result.Single(s => s.SubforumId == java.SubforumId);
        var csharpDto = result.Single(s => s.SubforumId == csharp.SubforumId);
        var pythonDto = result.Single(s => s.SubforumId == python.SubforumId);

        Assert.Equal(latestCommentTime, javaDto.LastActivityAt);
        Assert.Equal(csharpPostTime, csharpDto.LastActivityAt);
        Assert.Null(pythonDto.LastActivityAt);
    }
}