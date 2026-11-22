namespace RepositoryTests.Contracts;

using Entities;
using RepositoryTests.Factories;
using Xunit;
using ApiContracts;
using Exceptions;
using RepositoryContracts;

public class CommentRepositoryTests : IDisposable
{
    private readonly EfcRepositoryFactory factory;
    private readonly ICommentRepository repository;

    public CommentRepositoryTests()
    {
        factory = new EfcRepositoryFactory();
        repository = factory.CommentRepository;
    }

    public void Dispose()
    {
        factory.Dispose();
    }

    private static async Task<Comment> CreateCommentAsync(
        EfcRepositoryFactory factory,
        string body = "Test comment",
        User? user = null,
        int postId = 1,
        int? parentCommentId = null,
        DateTimeOffset? timestamp = null)
    {
        user ??= await factory.UserRepository.AddAsync(new User
        {
            Username = $"user_{Guid.NewGuid():N}",
            PasswordHash = "testhash"
        });

        return await factory.CommentRepository.AddAsync(new Comment
        {
            Body = body,
            UserId = user.UserId,
            PostId = postId,
            ParentCommentId = parentCommentId,
            Timestamp = timestamp ?? DateTimeOffset.UtcNow
        });
    }

    // CRUD

    [Fact]
    public async Task AddAsync_ShouldAssignId()
    {
        var comment = await CreateCommentAsync(factory);

        Assert.True(comment.ContentId > 0);
    }

    [Fact]
    public async Task AddAsync_ShouldAssignUniqueIds()
    {
        var comment1 = await CreateCommentAsync(factory);
        var comment2 = await CreateCommentAsync(factory);

        Assert.NotEqual(comment1.ContentId, comment2.ContentId);
    }

    [Fact]
    public async Task AddAsync_ShouldStartIdsAtOne()
    {
        var comment = await CreateCommentAsync(factory);

        Assert.Equal(1, comment.ContentId);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldReturnComment()
    {
        var comment = await CreateCommentAsync(factory);

        var result = await repository.GetSingleAsync(comment.ContentId);

        Assert.Equal(comment.ContentId, result.ContentId);
        Assert.Equal(comment.Body, result.Body);
        Assert.Equal(comment.UserId, result.UserId);
        Assert.Equal(comment.PostId, result.PostId);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenCommentDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(999));
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnAllComments()
    {
        await CreateCommentAsync(factory);
        await CreateCommentAsync(factory);
        await CreateCommentAsync(factory);

        var result = await repository.GetManyAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnEmptyList_WhenNoCommentsExist()
    {
        var result = await repository.GetManyAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateComment()
    {
        var comment = await CreateCommentAsync(factory);

        comment.Body = "Updated comment";

        await repository.UpdateAsync(comment);

        var result = await repository.GetSingleAsync(comment.ContentId);

        Assert.Equal("Updated comment", result.Body);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveId()
    {
        var comment = await CreateCommentAsync(factory);
        int originalId = comment.ContentId;

        comment.Body = "Updated comment";

        await repository.UpdateAsync(comment);

        var result = await repository.GetSingleAsync(originalId);

        Assert.Equal(originalId, result.ContentId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotAffectOtherComments()
    {
        var comment1 = await CreateCommentAsync(factory, "First comment");
        var comment2 = await CreateCommentAsync(factory, "Second comment");

        comment1.Body = "Updated comment";

        await repository.UpdateAsync(comment1);

        var result = await repository.GetSingleAsync(comment2.ContentId);

        Assert.Equal("Second comment", result.Body);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCommentDoesNotExist()
    {
        var comment = new Comment
        {
            ContentId = 999,
            Body = "Missing comment",
            UserId = 1,
            PostId = 1
        };

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.UpdateAsync(comment));
    }

    [Fact]
    public async Task DeleteAsync_ShouldMakeCommentUnavailable()
    {
        var comment = await CreateCommentAsync(factory);

        await repository.DeleteAsync(comment.ContentId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(comment.ContentId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotDeleteOtherComments()
    {
        var comment1 = await CreateCommentAsync(factory);
        var comment2 = await CreateCommentAsync(factory);

        await repository.DeleteAsync(comment1.ContentId);

        var result = await repository.GetSingleAsync(comment2.ContentId);

        Assert.Equal(comment2.ContentId, result.ContentId);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenCommentDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenDeletingSameCommentTwice()
    {
        var comment = await CreateCommentAsync(factory);

        await repository.DeleteAsync(comment.ContentId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.DeleteAsync(comment.ContentId));
    }

    // GetCommentsByPostIdAsync

    [Fact]
    public async Task GetCommentsByPostIdAsync_ShouldReturnCommentsForPost()
    {
        await CreateCommentAsync(factory, postId: 1);
        await CreateCommentAsync(factory, postId: 1);
        await CreateCommentAsync(factory, postId: 2);

        var result = await repository.GetCommentsByPostIdAsync(1);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(1, c.PostId));
    }

    [Fact]
    public async Task GetCommentsByPostIdAsync_ShouldReturnEmptyList_WhenNoCommentsExist()
    {
        var result = await repository.GetCommentsByPostIdAsync(999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCommentsByPostIdAsync_ShouldOrderByTimestampAscending()
    {
        var later = await CreateCommentAsync(
            factory,
            body: "Later",
            timestamp: DateTimeOffset.UtcNow.AddMinutes(2));

        var earlier = await CreateCommentAsync(
            factory,
            body: "Earlier",
            timestamp: DateTimeOffset.UtcNow.AddMinutes(-2));

        var middle = await CreateCommentAsync(
            factory,
            body: "Middle",
            timestamp: DateTimeOffset.UtcNow);

        var result = await repository.GetCommentsByPostIdAsync(1);

        Assert.Equal(
            new[] { earlier.ContentId, middle.ContentId, later.ContentId },
            result.Select(c => c.ContentId));
    }

    // GetCommentsByPostIdsAsync

    [Fact]
    public async Task GetCommentsByPostIdsAsync_ShouldReturnCommentsForAllSpecifiedPosts()
    {
        await CreateCommentAsync(factory, postId: 1);
        await CreateCommentAsync(factory, postId: 2);
        await CreateCommentAsync(factory, postId: 3);
        await CreateCommentAsync(factory, postId: 4);

        var result = await repository.GetCommentsByPostIdsAsync(new List<int> { 1, 3 });

        Assert.Equal(2, result.Count);
        Assert.All(
            result,
            c => Assert.Contains(c.PostId, new[] { 1, 3 }));
    }

    [Fact]
    public async Task GetCommentsByPostIdsAsync_ShouldReturnEmptyList_WhenNoPostsMatch()
    {
        await CreateCommentAsync(factory, postId: 1);

        var result = await repository.GetCommentsByPostIdsAsync(new List<int> { 2, 3 });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCommentsByPostIdsAsync_ShouldReturnEmptyList_WhenPostIdsIsEmpty()
    {
        await CreateCommentAsync(factory, postId: 1);

        var result = await repository.GetCommentsByPostIdsAsync(new List<int>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCommentsByPostIdsAsync_ShouldOrderByTimestampAscending()
    {
        var later = await CreateCommentAsync(
            factory,
            postId: 1,
            timestamp: DateTimeOffset.UtcNow.AddMinutes(2));

        var earlier = await CreateCommentAsync(
            factory,
            postId: 2,
            timestamp: DateTimeOffset.UtcNow.AddMinutes(-2));

        var middle = await CreateCommentAsync(
            factory,
            postId: 1,
            timestamp: DateTimeOffset.UtcNow);

        var result = await repository.GetCommentsByPostIdsAsync(new List<int> { 1, 2 });

        Assert.Equal(
            new[] { earlier.ContentId, middle.ContentId, later.ContentId },
            result.Select(c => c.ContentId));
    }

    // GetCommentCountByPostIdAsync

    [Fact]
    public async Task GetCommentCountByPostIdAsync_ShouldReturnCorrectCount()
    {
        await CreateCommentAsync(factory, postId: 1);
        await CreateCommentAsync(factory, postId: 1);
        await CreateCommentAsync(factory, postId: 1);
        await CreateCommentAsync(factory, postId: 2);

        var result = await repository.GetCommentCountByPostIdAsync(1);

        Assert.Equal(3, result);
    }

    [Fact]
    public async Task GetCommentCountByPostIdAsync_ShouldReturnZero_WhenPostHasNoComments()
    {
        var result = await repository.GetCommentCountByPostIdAsync(999);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetCommentCountByPostIdAsync_ShouldNotCountCommentsFromOtherPosts()
    {
        await CreateCommentAsync(factory, postId: 1);
        await CreateCommentAsync(factory, postId: 2);
        await CreateCommentAsync(factory, postId: 2);

        var result = await repository.GetCommentCountByPostIdAsync(1);

        Assert.Equal(1, result);
    }

    // GetCommentDetailsAsync

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldReturnAllComments_WhenNoFiltersAreProvided()
    {
        await CreateCommentAsync(factory);
        await CreateCommentAsync(factory);
        await CreateCommentAsync(factory);

        var result = await repository.GetCommentDetailsAsync(null, null, null);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldReturnEmptyList_WhenNoCommentsExist()
    {
        var result = await repository.GetCommentDetailsAsync(null, null, null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldFilterByPostId()
    {
        await CreateCommentAsync(factory, postId: 1);
        await CreateCommentAsync(factory, postId: 1);
        await CreateCommentAsync(factory, postId: 2);

        var result = await repository.GetCommentDetailsAsync(1, null, null);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(1, c.PostId));
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldFilterByUserId()
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

        await CreateCommentAsync(factory, user: user1);
        await CreateCommentAsync(factory, user: user1);
        await CreateCommentAsync(factory, user: user2);

        var result = await repository.GetCommentDetailsAsync(null, user1.UserId, null);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(user1.UserId, c.UserId));
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldFilterByUsername()
    {
        var user1 = await factory.UserRepository.AddAsync(new User
        {
            Username = "alice",
            PasswordHash = "hash"
        });

        var user2 = await factory.UserRepository.AddAsync(new User
        {
            Username = "bob",
            PasswordHash = "hash"
        });

        await CreateCommentAsync(factory, user: user1);
        await CreateCommentAsync(factory, user: user2);

        var result = await repository.GetCommentDetailsAsync(null, null, "alice");

        Assert.Single(result);
        Assert.Equal(user1.UserId, result[0].UserId);
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldFilterByUsernameCaseInsensitively()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "Alice",
            PasswordHash = "hash"
        });

        await CreateCommentAsync(factory, user: user);

        var result = await repository.GetCommentDetailsAsync(null, null, "ALICE");

        Assert.Single(result);
        Assert.Equal(user.UserId, result[0].UserId);
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldCombineFilters()
    {
        var user1 = await factory.UserRepository.AddAsync(new User
        {
            Username = "alice",
            PasswordHash = "hash"
        });

        var user2 = await factory.UserRepository.AddAsync(new User
        {
            Username = "bob",
            PasswordHash = "hash"
        });

        await CreateCommentAsync(factory, user: user1, postId: 1);
        await CreateCommentAsync(factory, user: user1, postId: 2);
        await CreateCommentAsync(factory, user: user2, postId: 1);

        var result = await repository.GetCommentDetailsAsync(1, user1.UserId, "alice");

        Assert.Single(result);
        Assert.Equal(user1.UserId, result[0].UserId);
        Assert.Equal(1, result[0].PostId);
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldReturnEmptyList_WhenNoCommentsMatchFilters()
    {
        await CreateCommentAsync(factory, postId: 1);

        var result = await repository.GetCommentDetailsAsync(999, null, null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldOrderByTimestampAscending()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "alice",
            PasswordHash = "hash"
        });

        var later = await CreateCommentAsync(
            factory,
            user: user,
            timestamp: DateTimeOffset.UtcNow.AddMinutes(2));

        var earlier = await CreateCommentAsync(
            factory,
            user: user,
            timestamp: DateTimeOffset.UtcNow.AddMinutes(-2));

        var middle = await CreateCommentAsync(
            factory,
            user: user,
            timestamp: DateTimeOffset.UtcNow);

        var result = await repository.GetCommentDetailsAsync(null, null, null);

        Assert.Equal(
            new[] { earlier.ContentId, middle.ContentId, later.ContentId },
            result.Select(c => c.CommentId));
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldMapCommentProperties()
    {
        var user = await factory.UserRepository.AddAsync(new User
        {
            Username = "alice",
            PasswordHash = "hash"
        });

        int parentCommentId = 42;
        var timestamp = DateTimeOffset.UtcNow;

        var comment = await CreateCommentAsync(
            factory,
            body: "A reply to another comment",
            user: user,
            postId: 7,
            parentCommentId: parentCommentId,
            timestamp: timestamp);

        var result = await repository.GetCommentDetailsAsync(null, null, null);

        var dto = Assert.Single(result);

        Assert.Equal(comment.ContentId, dto.CommentId);
        Assert.Equal(7, dto.PostId);
        Assert.Equal("A reply to another comment", dto.Body);
        Assert.Equal(user.UserId, dto.UserId);
        Assert.Equal("alice", dto.Username);
        Assert.Equal(timestamp, dto.CreatedAt);
        Assert.Equal(parentCommentId, dto.ParentCommentId);
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldMapNullParentCommentId()
    {
        await CreateCommentAsync(factory, parentCommentId: null);

        var result = await repository.GetCommentDetailsAsync(null, null, null);

        var dto = Assert.Single(result);

        Assert.Null(dto.ParentCommentId);
    }

    [Fact]
    public async Task GetCommentDetailsAsync_ShouldIncludeReactionCounts()
    {
        var comment = await CreateCommentAsync(factory);

        var reactionTypes = Enum.GetValues<ReactionType>();

        // Create five different users
        var users = new List<User>();

        for (int i = 1; i <= 5; i++)
        {
            var user = await factory.UserRepository.AddAsync(new User
            {
                Username = $"reactionuser{i}",
                PasswordHash = "hash"
            });

            users.Add(user);
        }

        // Add two reactions of the first type
        for (int i = 0; i < 2; i++)
        {
            await factory.ReactionRepository.AddAsync(new Reaction
            {
                UserId = users[i].UserId,
                ContentId = comment.ContentId,
                ContentType = ContentType.Comment,
                ReactionType = reactionTypes[0]
            });
        }

        // Add three reactions of the second type
        for (int i = 2; i < 5; i++)
        {
            await factory.ReactionRepository.AddAsync(new Reaction
            {
                UserId = users[i].UserId,
                ContentId = comment.ContentId,
                ContentType = ContentType.Comment,
                ReactionType = reactionTypes[1]
            });
        }

        var result = await repository.GetCommentDetailsAsync(null, null, null);

        var dto = Assert.Single(result);

        Assert.Equal(2, dto.ReactionCounts[reactionTypes[0]]);
        Assert.Equal(3, dto.ReactionCounts[reactionTypes[1]]);
    }
}