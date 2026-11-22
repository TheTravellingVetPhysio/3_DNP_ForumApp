namespace RepositoryTests.Contracts;

using ApiContracts;
using Entities;
using Exceptions;
using RepositoryContracts;
using RepositoryTests.Factories;
using Xunit;

public class PostRepositoryTests : IDisposable
{
    private readonly IRepositoryFactory factory;
    private readonly IPostRepository repository;

    public PostRepositoryTests()
    {
        factory = new EfcRepositoryFactory();
        repository = factory.PostRepository;
    }

    public void Dispose()
    {
        factory.Dispose();
    }

    // Helper

    private static async Task<Post> CreatePostAsync(
        IRepositoryFactory factory,
        string title = "Test title",
        string body = "Test body",
        User? user = null,
        Subforum? subforum = null)
    {
        user ??= await factory.UserRepository.AddAsync(new User
        {
            Username = $"user_{Guid.NewGuid():N}",
            PasswordHash = "testhash"
        });

        subforum ??= await factory.SubforumRepository.AddAsync(new Subforum
        {
            Name = $"subforum_{Guid.NewGuid():N}",
            UserId = user.UserId
        });

        return await factory.PostRepository.AddAsync(new Post
        {
            Title = title,
            Body = body,
            UserId = user.UserId,
            SubforumId = subforum.SubforumId
        });
    }

    // CRUD

    [Fact]
    public async Task AddAsync_ShouldAssignId_AndAllowRetrieval()
    {
        var post = await CreatePostAsync(factory);

        Assert.True(post.ContentId > 0);

        var retrievedPost = await repository.GetSingleAsync(post.ContentId);

        Assert.Equal("Test title", retrievedPost.Title);
        Assert.Equal("Test body", retrievedPost.Body);
        Assert.Equal(post.UserId, retrievedPost.UserId);
        Assert.Equal(post.SubforumId, retrievedPost.SubforumId);
    }

    [Fact]
    public async Task AddAsync_ShouldAssignUniqueIds_ToMultiplePosts()
    {
        var post1 = await CreatePostAsync(factory, "Post 1");
        var post2 = await CreatePostAsync(factory, "Post 2");

        Assert.NotEqual(post1.ContentId, post2.ContentId);
    }

    [Fact]
    public async Task AddAsync_ShouldStartIdAtOne_WhenRepositoryIsEmpty()
    {
        var post = await CreatePostAsync(factory);

        Assert.Equal(1, post.ContentId);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenPostDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(999));
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenIdIsZero()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(0));
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenIdIsNegative()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(-1));
    }

    [Fact]
    public async Task GetSingleAsync_ShouldReturnCorrectPost_WhenMultiplePostsExist()
    {
        await CreatePostAsync(factory, "Post 1", "Body 1");

        var post2 = await CreatePostAsync(factory, "Post 2", "Body 2");

        await CreatePostAsync(factory, "Post 3", "Body 3");

        var retrievedPost = await repository.GetSingleAsync(post2.ContentId);

        Assert.Equal(post2.ContentId, retrievedPost.ContentId);
        Assert.Equal("Post 2", retrievedPost.Title);
        Assert.Equal("Body 2", retrievedPost.Body);
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnAllPosts()
    {
        await CreatePostAsync(factory, "Post 1");
        await CreatePostAsync(factory, "Post 2");

        var posts = await repository.GetManyAsync();

        Assert.Equal(2, posts.Count);
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnEmptyList_WhenNoPostsExist()
    {
        var posts = await repository.GetManyAsync();

        Assert.Empty(posts);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingPost()
    {
        var post = await CreatePostAsync(factory, "Old title", "Old body");

        post.Title = "New title";
        post.Body = "New body";

        await repository.UpdateAsync(post);

        var updatedPost = await repository.GetSingleAsync(post.ContentId);

        Assert.Equal("New title", updatedPost.Title);
        Assert.Equal("New body", updatedPost.Body);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotModifyOtherPosts()
    {
        var post1 = await CreatePostAsync(factory, "Post 1", "Body 1");
        var post2 = await CreatePostAsync(factory, "Post 2", "Body 2");

        post1.Title = "Updated title";
        await repository.UpdateAsync(post1);

        var unchangedPost = await repository.GetSingleAsync(post2.ContentId);

        Assert.Equal("Post 2", unchangedPost.Title);
        Assert.Equal("Body 2", unchangedPost.Body);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenPostDoesNotExist()
    {
        var post = new Post
        {
            ContentId = 999,
            Title = "Unknown",
            Body = "Unknown",
            UserId = 1,
            SubforumId = 1
        };

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.UpdateAsync(post));
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreservePostId()
    {
        var post = await CreatePostAsync(factory);
        int originalId = post.ContentId;

        post.Title = "Updated title";

        await repository.UpdateAsync(post);

        var updatedPost = await repository.GetSingleAsync(originalId);

        Assert.Equal(originalId, updatedPost.ContentId);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveExistingPost()
    {
        var post = await CreatePostAsync(factory);

        await repository.DeleteAsync(post.ContentId);

        var posts = await repository.GetManyAsync();

        Assert.Empty(posts);
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotDeleteOtherPosts()
    {
        var post1 = await CreatePostAsync(factory, "Post 1");
        var post2 = await CreatePostAsync(factory, "Post 2");

        await repository.DeleteAsync(post1.ContentId);

        var remainingPost = await repository.GetSingleAsync(post2.ContentId);

        Assert.Equal("Post 2", remainingPost.Title);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenPostDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_ShouldMakePostUnavailable()
    {
        var post = await CreatePostAsync(factory);

        await repository.DeleteAsync(post.ContentId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(post.ContentId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenDeletingSamePostTwice()
    {
        var post = await CreatePostAsync(factory);

        await repository.DeleteAsync(post.ContentId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.DeleteAsync(post.ContentId));
    }

    // Counts

    [Fact]
    public async Task GetPostCountByUserAsync_ShouldReturnCorrectCount()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        await CreatePostAsync(factory, "Post 1", user: user);
        await CreatePostAsync(factory, "Post 2", user: user);

        var count = await repository.GetPostCountByUserAsync(user.UserId);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetPostCountByUserAsync_ShouldReturnZero_WhenUserHasNoPosts()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        var count = await repository.GetPostCountByUserAsync(user.UserId);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetPostCountByUserAsync_ShouldNotCountOtherUsersPosts()
    {
        var user1 = await factory.UserRepository.AddAsync(new User
        {
            Username = "user1",
            PasswordHash = "hash"
        });

        var user2 = await factory.UserRepository.AddAsync(new User
        {
            Username = "user2",
            PasswordHash = "hash"
        });

        await CreatePostAsync(factory, "Post 1", user: user1);
        await CreatePostAsync(factory, "Post 2", user: user2);

        var count = await repository.GetPostCountByUserAsync(user1.UserId);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetPostCountBySubforumAsync_ShouldReturnCorrectCount()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        var subforum = await factory.SubforumRepository.AddAsync(new Subforum
        {
            Name = "Test subforum",
            UserId = user.UserId
        });

        await CreatePostAsync(factory, "Post 1", subforum: subforum);
        await CreatePostAsync(factory, "Post 2", subforum: subforum);

        var count = await repository.GetPostCountBySubforumAsync(subforum.SubforumId);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetPostCountBySubforumAsync_ShouldReturnZero_WhenSubforumHasNoPosts()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        var subforum = await factory.SubforumRepository.AddAsync(new Subforum
        {
            Name = "Empty subforum",
            UserId = user.UserId
        });

        var count = await repository.GetPostCountBySubforumAsync(subforum.SubforumId);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetPostCountBySubforumAsync_ShouldNotCountPostsFromOtherSubforums()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        var subforum1 = await factory.SubforumRepository.AddAsync(new Subforum
        {
            Name = "Subforum 1",
            UserId = user.UserId
        });

        var subforum2 = await factory.SubforumRepository.AddAsync(new Subforum
        {
            Name = "Subforum 2",
            UserId = user.UserId
        });

        await CreatePostAsync(factory, "Post 1", subforum: subforum1);
        await CreatePostAsync(factory, "Post 2", subforum: subforum2);

        var count = await repository.GetPostCountBySubforumAsync(subforum1.SubforumId);

        Assert.Equal(1, count);
    }

    // Post overview

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldReturnAllPosts_WhenNoFiltersAreProvided()
    {
        await CreatePostAsync(factory, "Post 1");
        await CreatePostAsync(factory, "Post 2");

        var posts = await repository.GetPostsOverviewAsync(
            null, null, null, null);

        Assert.Equal(2, posts.Count);
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldReturnEmptyList_WhenNoPostsExist()
    {
        var posts = await repository.GetPostsOverviewAsync(
            null, null, null, null);

        Assert.Empty(posts);
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldFilterByTitle()
    {
        await CreatePostAsync(factory, "Java programming");
        await CreatePostAsync(factory, "CSharp programming");
        await CreatePostAsync(factory, "Java testing");

        var posts = await repository.GetPostsOverviewAsync(
            "Java", null, null, null);

        Assert.Equal(2, posts.Count);
        Assert.All(posts, post =>
            Assert.Contains("Java", post.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldIgnoreCase_WhenFilteringByTitle()
    {
        await CreatePostAsync(factory, "Java programming");

        var posts = await repository.GetPostsOverviewAsync(
            "JAVA", null, null, null);

        Assert.Single(posts);
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldFilterByUserId()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        await CreatePostAsync(factory, "Post 1", user: user);
        await CreatePostAsync(factory, "Post 2");

        var posts = await repository.GetPostsOverviewAsync(
            null, user.UserId, null, null);

        Assert.Single(posts);
        Assert.Equal(user.UserId, posts[0].UserId);
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldFilterByUsername()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "anotheruser",
            PasswordHash = "hash"
        });

        await CreatePostAsync(factory, "Post 1", user: user);
        await CreatePostAsync(factory, "Post 2");

        var posts = await repository.GetPostsOverviewAsync(
            null, null, "another", null);

        Assert.Single(posts);
        Assert.Equal(user.UserId, posts[0].UserId);
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldIgnoreCase_WhenFilteringByUsername()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "TestUser",
            PasswordHash = "hash"
        });

        await CreatePostAsync(factory, user: user);

        var posts = await repository.GetPostsOverviewAsync(
            null, null, "TESTUSER", null);

        Assert.Single(posts);
        Assert.Equal(user.UserId, posts[0].UserId);
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldFilterBySubforumId()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        var subforum = await factory.SubforumRepository.AddAsync(new Subforum
        {
            Name = "Target subforum",
            UserId = user.UserId
        });

        await CreatePostAsync(factory, "Post 1", user: user, subforum: subforum);
        await CreatePostAsync(factory, "Post 2");

        var posts = await repository.GetPostsOverviewAsync(
            null, null, null, subforum.SubforumId);

        Assert.Single(posts);
        Assert.Equal("Post 1", posts[0].Title);
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldReturnEmptyList_WhenNoPostsMatch()
    {
        await CreatePostAsync(factory, "Java programming");

        var posts = await repository.GetPostsOverviewAsync(
            "Unknown", null, null, null);

        Assert.Empty(posts);
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldApplyMultipleFiltersTogether()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        var subforum = await factory.SubforumRepository.AddAsync(new Subforum
        {
            Name = "Target subforum",
            UserId = user.UserId
        });

        await CreatePostAsync(factory, "Java programming", user: user, subforum: subforum);
        await CreatePostAsync(factory, "CSharp programming", user: user, subforum: subforum);
        await CreatePostAsync(factory, "Java programming");

        var posts = await repository.GetPostsOverviewAsync(
            "Java", user.UserId, "testuser", subforum.SubforumId);

        Assert.Single(posts);
        Assert.Equal("Java programming", posts[0].Title);
        Assert.Equal(user.UserId, posts[0].UserId);
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldOrderByLastActivityDescending()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "commentuser",
            PasswordHash = "hash"
        });

        var olderPost = await CreatePostAsync(factory, "Older post");
        var newerPost = await CreatePostAsync(factory, "Newer post");

        await factory.CommentRepository.AddAsync(new Comment
        {
            Body = "New comment",
            UserId = user.UserId,
            PostId = olderPost.ContentId,
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(1)
        });

        var posts = await repository.GetPostsOverviewAsync(
            null, null, null, null);

        Assert.Equal(olderPost.ContentId, posts[0].PostId);
        Assert.Equal(newerPost.ContentId, posts[1].PostId);
    }

    [Fact]
    public async Task GetPostsOverviewAsync_ShouldIncludeCorrectUserInformation()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "testuser",
            PasswordHash = "hash"
        });

        var post = await CreatePostAsync(factory, user: user);

        var posts = await repository.GetPostsOverviewAsync(
            null, null, null, null);

        var overview = Assert.Single(posts);

        Assert.Equal(post.ContentId, overview.PostId);
        Assert.Equal(post.Title, overview.Title);
        Assert.Equal(user.UserId, overview.UserId);
        Assert.Equal(user.Username, overview.Username);
        Assert.Equal(post.Timestamp, overview.CreatedAt);
    }

    // Post details

    [Fact]
    public async Task GetPostDetailsAsync_ShouldReturnPostDetails()
    {
        var post = await CreatePostAsync(factory, "Test title", "Test body");

        var user = await factory.UserRepository.GetSingleAsync(post.UserId);

        var details = await repository.GetPostDetailsAsync(post.ContentId);

        Assert.Equal(post.ContentId, details.PostId);
        Assert.Equal("Test title", details.Title);
        Assert.Equal("Test body", details.Body);
        Assert.Equal(post.UserId, details.UserId);
        Assert.Equal(user.Username, details.Username);
        Assert.Equal(post.SubforumId, details.SubforumId);
        Assert.Equal(post.Timestamp, details.CreatedAt);
        Assert.Empty(details.Comments);
    }

    [Fact]
    public async Task GetPostDetailsAsync_ShouldReturnComments()
    {
        var post = await CreatePostAsync(factory);

        var commentUser = await factory.UserRepository.AddAsync(new User
        {
            Username = "commentuser",
            PasswordHash = "hash"
        });

        await factory.CommentRepository.AddAsync(new Comment
        {
            Body = "Test comment",
            UserId = commentUser.UserId,
            PostId = post.ContentId
        });

        var details = await repository.GetPostDetailsAsync(post.ContentId);

        Assert.Single(details.Comments);
        Assert.Equal("Test comment", details.Comments[0].Body);
    }

    [Fact]
    public async Task GetPostDetailsAsync_ShouldThrow_WhenPostDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetPostDetailsAsync(999));
    }
}