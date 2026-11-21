namespace RepositoryTests.Contracts;

using ApiContracts;
using Entities;
using Exceptions;
using FileRepositories;
using RepositoryTests.Factories;
using Xunit;

public class SubforumRepositoryTests
{
    private static async Task<Subforum> CreateSubforumAsync(
        FileRepositoryFactory factory,
        string name = "Test subforum",
        int userId = 1)
    {
        return await factory.SubforumRepository.AddAsync(new Subforum
        {
            Name = name,
            UserId = userId
        });
    }

    private static async Task<Post> CreatePostAsync(
        FileRepositoryFactory factory,
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
        FileRepositoryFactory factory,
        int postId,
        DateTimeOffset? timestamp = null)
    {
        return await factory.CommentRepository.AddAsync(new Comment
        {
            Body = "Test comment",
            UserId = 1,
            PostId = postId,
            Timestamp = timestamp ?? DateTimeOffset.UtcNow
        });
    }

    // --------------------------------------------------
    // CRUD
    // --------------------------------------------------

    [Fact]
    public async Task AddAsync_ShouldAssignId()
    {
        using var factory = new FileRepositoryFactory();

        var subforum = await CreateSubforumAsync(factory);

        Assert.True(subforum.SubforumId > 0);
    }

    [Fact]
    public async Task AddAsync_ShouldAssignUniqueIds()
    {
        using var factory = new FileRepositoryFactory();

        var subforum1 = await CreateSubforumAsync(factory, "First");
        var subforum2 = await CreateSubforumAsync(factory, "Second");

        Assert.NotEqual(subforum1.SubforumId, subforum2.SubforumId);
    }

    [Fact]
    public async Task AddAsync_ShouldStartIdsAtOne()
    {
        using var factory = new FileRepositoryFactory();

        var subforum = await CreateSubforumAsync(factory);

        Assert.Equal(1, subforum.SubforumId);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldReturnSubforum()
    {
        using var factory = new FileRepositoryFactory();

        var subforum = await CreateSubforumAsync(factory);

        var result = await factory.SubforumRepository
            .GetSingleAsync(subforum.SubforumId);

        Assert.Equal(subforum.SubforumId, result.SubforumId);
        Assert.Equal(subforum.Name, result.Name);
        Assert.Equal(subforum.UserId, result.UserId);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenSubforumDoesNotExist()
    {
        using var factory = new FileRepositoryFactory();

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => factory.SubforumRepository.GetSingleAsync(999));
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnAllSubforums()
    {
        using var factory = new FileRepositoryFactory();

        await CreateSubforumAsync(factory, "First");
        await CreateSubforumAsync(factory, "Second");
        await CreateSubforumAsync(factory, "Third");

        var result = await factory.SubforumRepository.GetManyAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnEmptyList_WhenNoSubforumsExist()
    {
        using var factory = new FileRepositoryFactory();

        var result = await factory.SubforumRepository.GetManyAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSubforum()
    {
        using var factory = new FileRepositoryFactory();

        var subforum = await CreateSubforumAsync(factory);

        subforum.Name = "Updated subforum";

        await factory.SubforumRepository.UpdateAsync(subforum);

        var result = await factory.SubforumRepository
            .GetSingleAsync(subforum.SubforumId);

        Assert.Equal("Updated subforum", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveId()
    {
        using var factory = new FileRepositoryFactory();

        var subforum = await CreateSubforumAsync(factory);
        int originalId = subforum.SubforumId;

        subforum.Name = "Updated subforum";

        await factory.SubforumRepository.UpdateAsync(subforum);

        var result = await factory.SubforumRepository
            .GetSingleAsync(originalId);

        Assert.Equal(originalId, result.SubforumId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotAffectOtherSubforums()
    {
        using var factory = new FileRepositoryFactory();

        var subforum1 = await CreateSubforumAsync(factory, "First");
        var subforum2 = await CreateSubforumAsync(factory, "Second");

        subforum1.Name = "Updated first";

        await factory.SubforumRepository.UpdateAsync(subforum1);

        var result = await factory.SubforumRepository
            .GetSingleAsync(subforum2.SubforumId);

        Assert.Equal("Second", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenSubforumDoesNotExist()
    {
        using var factory = new FileRepositoryFactory();

        var subforum = new Subforum
        {
            SubforumId = 999,
            Name = "Missing subforum",
            UserId = 1
        };

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => factory.SubforumRepository.UpdateAsync(subforum));
    }

    [Fact]
    public async Task DeleteAsync_ShouldMakeSubforumUnavailable()
    {
        using var factory = new FileRepositoryFactory();

        var subforum = await CreateSubforumAsync(factory);

        await factory.SubforumRepository.DeleteAsync(subforum.SubforumId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => factory.SubforumRepository.GetSingleAsync(subforum.SubforumId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotDeleteOtherSubforums()
    {
        using var factory = new FileRepositoryFactory();

        var subforum1 = await CreateSubforumAsync(factory, "First");
        var subforum2 = await CreateSubforumAsync(factory, "Second");

        await factory.SubforumRepository.DeleteAsync(subforum1.SubforumId);

        var result = await factory.SubforumRepository
            .GetSingleAsync(subforum2.SubforumId);

        Assert.Equal(subforum2.SubforumId, result.SubforumId);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenSubforumDoesNotExist()
    {
        using var factory = new FileRepositoryFactory();

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => factory.SubforumRepository.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenDeletingSameSubforumTwice()
    {
        using var factory = new FileRepositoryFactory();

        var subforum = await CreateSubforumAsync(factory);

        await factory.SubforumRepository.DeleteAsync(subforum.SubforumId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => factory.SubforumRepository.DeleteAsync(subforum.SubforumId));
    }

    // --------------------------------------------------
    // SubforumNameExistsAsync
    // --------------------------------------------------

    [Fact]
    public async Task SubforumNameExistsAsync_ShouldReturnTrue_WhenNameExists()
    {
        using var factory = new FileRepositoryFactory();

        await CreateSubforumAsync(factory, "Java");

        var result = await factory.SubforumRepository
            .SubforumNameExistsAsync("Java");

        Assert.True(result);
    }

    [Fact]
    public async Task SubforumNameExistsAsync_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        using var factory = new FileRepositoryFactory();

        await CreateSubforumAsync(factory, "Java");

        var result = await factory.SubforumRepository
            .SubforumNameExistsAsync("CSharp");

        Assert.False(result);
    }

    [Fact]
    public async Task SubforumNameExistsAsync_ShouldIgnoreCase()
    {
        using var factory = new FileRepositoryFactory();

        await CreateSubforumAsync(factory, "Java");

        var result = await factory.SubforumRepository
            .SubforumNameExistsAsync("JAVA");

        Assert.True(result);
    }

    [Fact]
    public async Task SubforumNameExistsAsync_ShouldReturnFalse_WhenNoSubforumsExist()
    {
        using var factory = new FileRepositoryFactory();

        var result = await factory.SubforumRepository
            .SubforumNameExistsAsync("Java");

        Assert.False(result);
    }

    // --------------------------------------------------
    // GetSubforumOverviewsAsync
    // --------------------------------------------------

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldReturnAllSubforums()
    {
        using var factory = new FileRepositoryFactory();

        await CreateSubforumAsync(factory, "Java");
        await CreateSubforumAsync(factory, "CSharp");
        await CreateSubforumAsync(factory, "Python");

        var result = await factory.SubforumRepository
            .GetSubforumOverviewsAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldReturnEmptyList_WhenNoSubforumsExist()
    {
        using var factory = new FileRepositoryFactory();

        var result = await factory.SubforumRepository
            .GetSubforumOverviewsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldMapSubforumProperties()
    {
        using var factory = new FileRepositoryFactory();

        var subforum = await CreateSubforumAsync(factory, "Java");

        var result = await factory.SubforumRepository
            .GetSubforumOverviewsAsync();

        var dto = Assert.Single(result);

        Assert.Equal(subforum.SubforumId, dto.SubforumId);
        Assert.Equal("Java", dto.Name);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldReturnNullLastActivity_WhenSubforumHasNoActivity()
    {
        using var factory = new FileRepositoryFactory();

        await CreateSubforumAsync(factory, "Empty");

        var result = await factory.SubforumRepository
            .GetSubforumOverviewsAsync();

        var dto = Assert.Single(result);

        Assert.Null(dto.LastActivityAt);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldUseLatestPostAsLastActivity()
    {
        using var factory = new FileRepositoryFactory();

        var subforum = await CreateSubforumAsync(factory, "Java");

        var earlier = DateTimeOffset.UtcNow.AddDays(-2);
        var later = DateTimeOffset.UtcNow.AddDays(-1);

        await CreatePostAsync(factory, subforum.SubforumId, earlier);
        await CreatePostAsync(factory, subforum.SubforumId, later);

        var result = await factory.SubforumRepository
            .GetSubforumOverviewsAsync();

        var dto = Assert.Single(result);

        Assert.Equal(later, dto.LastActivityAt);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldUseLatestCommentAsLastActivity()
    {
        using var factory = new FileRepositoryFactory();

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

        var result = await factory.SubforumRepository
            .GetSubforumOverviewsAsync();

        var dto = Assert.Single(result);

        Assert.Equal(latestComment, dto.LastActivityAt);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldUseLatestActivityBetweenPostsAndComments()
    {
        using var factory = new FileRepositoryFactory();

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

        var result = await factory.SubforumRepository
            .GetSubforumOverviewsAsync();

        var dto = Assert.Single(result);

        Assert.Equal(latestPost, dto.LastActivityAt);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldIgnoreActivityFromOtherSubforums()
    {
        using var factory = new FileRepositoryFactory();

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

        var result = await factory.SubforumRepository
            .GetSubforumOverviewsAsync();

        var javaDto = result.Single(s => s.SubforumId == subforum1.SubforumId);
        var csharpDto = result.Single(s => s.SubforumId == subforum2.SubforumId);

        Assert.Equal(commentTime, javaDto.LastActivityAt);
        Assert.NotEqual(commentTime, csharpDto.LastActivityAt);
    }

    [Fact]
    public async Task GetSubforumOverviewsAsync_ShouldReturnCorrectActivityForMultipleSubforums()
    {
        using var factory = new FileRepositoryFactory();

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

        var result = await factory.SubforumRepository
            .GetSubforumOverviewsAsync();

        var javaDto = result.Single(s => s.SubforumId == java.SubforumId);
        var csharpDto = result.Single(s => s.SubforumId == csharp.SubforumId);
        var pythonDto = result.Single(s => s.SubforumId == python.SubforumId);

        Assert.Equal(latestCommentTime, javaDto.LastActivityAt);
        Assert.Equal(csharpPostTime, csharpDto.LastActivityAt);
        Assert.Null(pythonDto.LastActivityAt);
    }
}