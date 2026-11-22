namespace RepositoryTests.Contracts;

using Entities;
using RepositoryContracts;
using RepositoryTests.Factories;
using Xunit;
using ApiContracts;
using Exceptions;

public class ReactionRepositoryTests : IDisposable
{
    private readonly IRepositoryFactory factory;
    private readonly IReactionRepository repository;

    public ReactionRepositoryTests()
    {
        factory = new EfcRepositoryFactory();
        repository = factory.ReactionRepository;
    }

    public void Dispose()
    {
        factory.Dispose();
    }

    private static async Task<Reaction> CreateReactionAsync(
        IRepositoryFactory factory,
        int userId = 1,
        int contentId = 1,
        ContentType contentType = ContentType.Post,
        ReactionType? reactionType = null)
    {
        // Sørger for, at brugeren eksisterer.
        var users = await factory.UserRepository.GetManyAsync();

        if (!users.Any(u => u.UserId == userId))
        {
            await factory.UserRepository.AddAsync(new User
            {
                UserId = userId,
                Username = $"user_{Guid.NewGuid():N}",
                PasswordHash = "testhash"
            });
        }

        reactionType ??= Enum.GetValues<ReactionType>().First();

        return await factory.ReactionRepository.AddAsync(new Reaction
        {
            UserId = userId,
            ContentId = contentId,
            ContentType = contentType,
            ReactionType = reactionType.Value
        });
    }

    // CRUD

    [Fact]
    public async Task AddAsync_ShouldAssignId()
    {
        var reaction = await CreateReactionAsync(factory);

        Assert.True(reaction.ReactionId > 0);
    }

    [Fact]
    public async Task AddAsync_ShouldAssignUniqueIds()
    {
        var reaction1 = await CreateReactionAsync(factory);
        var reaction2 = await CreateReactionAsync(factory);

        Assert.NotEqual(reaction1.ReactionId, reaction2.ReactionId);
    }

    [Fact]
    public async Task AddAsync_ShouldStartIdsAtOne()
    {
        var reaction = await CreateReactionAsync(factory);

        Assert.Equal(1, reaction.ReactionId);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldReturnReaction()
    {
        var reaction = await CreateReactionAsync(factory);

        var result = await repository.GetSingleAsync(reaction.ReactionId);

        Assert.Equal(reaction.ReactionId, result.ReactionId);
        Assert.Equal(reaction.UserId, result.UserId);
        Assert.Equal(reaction.ContentId, result.ContentId);
        Assert.Equal(reaction.ContentType, result.ContentType);
        Assert.Equal(reaction.ReactionType, result.ReactionType);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenReactionDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(999));
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnAllReactions()
    {
        await CreateReactionAsync(factory);
        await CreateReactionAsync(factory, userId: 2);
        await CreateReactionAsync(factory, userId: 3);

        var result = await repository.GetManyAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnEmptyList_WhenNoReactionsExist()
    {
        var result = await repository.GetManyAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReaction()
    {
        var reactionTypes = Enum.GetValues<ReactionType>();

        var reaction = await CreateReactionAsync(
            factory,
            reactionType: reactionTypes[0]);

        reaction.ReactionType = reactionTypes[1];

        await repository.UpdateAsync(reaction);

        var result = await repository.GetSingleAsync(reaction.ReactionId);

        Assert.Equal(reactionTypes[1], result.ReactionType);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveId()
    {
        var reaction = await CreateReactionAsync(factory);
        int originalId = reaction.ReactionId;

        reaction.ContentId = 2;

        await repository.UpdateAsync(reaction);

        var result = await repository.GetSingleAsync(originalId);

        Assert.Equal(originalId, result.ReactionId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotAffectOtherReactions()
    {
        var reaction1 = await CreateReactionAsync(factory, contentId: 1);
        var reaction2 = await CreateReactionAsync(factory, contentId: 2);

        reaction1.ContentId = 3;

        await repository.UpdateAsync(reaction1);

        var result = await repository.GetSingleAsync(reaction2.ReactionId);

        Assert.Equal(2, result.ContentId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenReactionDoesNotExist()
    {
        var reaction = new Reaction
        {
            ReactionId = 999,
            UserId = 1,
            ContentId = 1,
            ContentType = ContentType.Post,
            ReactionType = Enum.GetValues<ReactionType>().First()
        };

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.UpdateAsync(reaction));
    }

    [Fact]
    public async Task DeleteAsync_ShouldMakeReactionUnavailable()
    {
        var reaction = await CreateReactionAsync(factory);

        await repository.DeleteAsync(reaction.ReactionId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.GetSingleAsync(reaction.ReactionId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotDeleteOtherReactions()
    {
        var reaction1 = await CreateReactionAsync(factory, userId: 1);
        var reaction2 = await CreateReactionAsync(factory, userId: 2);

        await repository.DeleteAsync(reaction1.ReactionId);

        var result = await repository.GetSingleAsync(reaction2.ReactionId);

        Assert.Equal(reaction2.ReactionId, result.ReactionId);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenReactionDoesNotExist()
    {
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenDeletingSameReactionTwice()
    {
        var reaction = await CreateReactionAsync(factory);

        await repository.DeleteAsync(reaction.ReactionId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => repository.DeleteAsync(reaction.ReactionId));
    }

    // GetReactionByUserAndContentAsync

    [Fact]
    public async Task GetReactionByUserAndContentAsync_ShouldReturnMatchingReaction()
    {
        var reaction = await CreateReactionAsync(
            factory,
            userId: 1,
            contentId: 10,
            contentType: ContentType.Post);

        var result = await repository.GetReactionByUserAndContentAsync(
            1, 10, ContentType.Post);

        Assert.NotNull(result);
        Assert.Equal(reaction.ReactionId, result.ReactionId);
    }

    [Fact]
    public async Task GetReactionByUserAndContentAsync_ShouldReturnNull_WhenReactionDoesNotExist()
    {
        var result = await repository.GetReactionByUserAndContentAsync(
            1, 10, ContentType.Post);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetReactionByUserAndContentAsync_ShouldMatchUserId()
    {
        await CreateReactionAsync(
            factory,
            userId: 1,
            contentId: 10);

        var result = await repository.GetReactionByUserAndContentAsync(
            2, 10, ContentType.Post);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetReactionByUserAndContentAsync_ShouldMatchContentId()
    {
        await CreateReactionAsync(
            factory,
            userId: 1,
            contentId: 10);

        var result = await repository.GetReactionByUserAndContentAsync(
            1, 20, ContentType.Post);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetReactionByUserAndContentAsync_ShouldMatchContentType()
    {
        await CreateReactionAsync(
            factory,
            userId: 1,
            contentId: 10,
            contentType: ContentType.Post);

        var result = await repository.GetReactionByUserAndContentAsync(
            1, 10, ContentType.Comment);

        Assert.Null(result);
    }

    // GetReactionCountsAsync

    [Fact]
    public async Task GetReactionCountsAsync_ShouldReturnEmptyDictionary_WhenNoReactionsExist()
    {
        var result = await repository.GetReactionCountsAsync(
            1, ContentType.Post);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReactionCountsAsync_ShouldCountReactionsByType()
    {
        var reactionTypes = Enum.GetValues<ReactionType>();

        // Two reactions of the first type
        await CreateReactionAsync(factory, userId: 1,
            reactionType: reactionTypes[0]);

        await CreateReactionAsync(factory, userId: 2,
            reactionType: reactionTypes[0]);

        // Three reactions of the second type
        await CreateReactionAsync(factory, userId: 3,
            reactionType: reactionTypes[1]);

        await CreateReactionAsync(factory, userId: 4,
            reactionType: reactionTypes[1]);

        await CreateReactionAsync(factory, userId: 5,
            reactionType: reactionTypes[1]);

        var result = await repository.GetReactionCountsAsync(
            1, ContentType.Post);

        Assert.Equal(2, result[reactionTypes[0]]);
        Assert.Equal(3, result[reactionTypes[1]]);
    }

    [Fact]
    public async Task GetReactionCountsAsync_ShouldOnlyCountReactionsForSpecifiedContentId()
    {
        var reactionTypes = Enum.GetValues<ReactionType>();

        await CreateReactionAsync(factory,
            userId: 1,
            contentId: 1,
            reactionType: reactionTypes[0]);

        await CreateReactionAsync(factory,
            userId: 2,
            contentId: 1,
            reactionType: reactionTypes[0]);

        await CreateReactionAsync(factory,
            userId: 3,
            contentId: 2,
            reactionType: reactionTypes[0]);

        var result = await repository.GetReactionCountsAsync(
            1, ContentType.Post);

        Assert.Equal(2, result[reactionTypes[0]]);
    }

    [Fact]
    public async Task GetReactionCountsAsync_ShouldOnlyCountReactionsForSpecifiedContentType()
    {
        var reactionType = Enum.GetValues<ReactionType>().First();

        await CreateReactionAsync(factory,
            userId: 1,
            contentId: 1,
            contentType: ContentType.Post,
            reactionType: reactionType);

        await CreateReactionAsync(factory,
            userId: 2,
            contentId: 1,
            contentType: ContentType.Comment,
            reactionType: reactionType);

        var result = await repository.GetReactionCountsAsync(
            1, ContentType.Post);

        Assert.Equal(1, result[reactionType]);
    }

    [Fact]
    public async Task GetReactionCountsAsync_ShouldReturnEmptyDictionary_WhenOnlyOtherContentHasReactions()
    {
        await CreateReactionAsync(factory,
            contentId: 2,
            contentType: ContentType.Post);

        var result = await repository.GetReactionCountsAsync(
            1, ContentType.Post);

        Assert.Empty(result);
    }
}