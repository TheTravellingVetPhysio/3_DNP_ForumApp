namespace RepositoryTests.Contracts;

using Entities;
using FileRepositories;
using RepositoryTests.Factories;
using Xunit;
using ApiContracts;
using Exceptions;

public class ReactionRepositoryTests
{
    private static async Task<Reaction> CreateReactionAsync(
        FileRepositoryFactory factory,
        int userId = 1,
        int contentId = 1,
        ContentType contentType = ContentType.Post,
        ReactionType? reactionType = null)
    {
        reactionType ??= Enum.GetValues<ReactionType>().First();

        return await factory.ReactionRepository.AddAsync(new Reaction
        {
            UserId = userId,
            ContentId = contentId,
            ContentType = contentType,
            ReactionType = reactionType.Value
        });
    }

    // --------------------------------------------------
    // CRUD
    // --------------------------------------------------

    [Fact]
    public async Task AddAsync_ShouldAssignId()
    {
        using var factory = new FileRepositoryFactory();

        var reaction = await CreateReactionAsync(factory);

        Assert.True(reaction.ReactionId > 0);
    }

    [Fact]
    public async Task AddAsync_ShouldAssignUniqueIds()
    {
        using var factory = new FileRepositoryFactory();

        var reaction1 = await CreateReactionAsync(factory);
        var reaction2 = await CreateReactionAsync(factory);

        Assert.NotEqual(reaction1.ReactionId, reaction2.ReactionId);
    }

    [Fact]
    public async Task AddAsync_ShouldStartIdsAtOne()
    {
        using var factory = new FileRepositoryFactory();

        var reaction = await CreateReactionAsync(factory);

        Assert.Equal(1, reaction.ReactionId);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldReturnReaction()
    {
        using var factory = new FileRepositoryFactory();

        var reaction = await CreateReactionAsync(factory);

        var result = await factory.ReactionRepository
            .GetSingleAsync(reaction.ReactionId);

        Assert.Equal(reaction.ReactionId, result.ReactionId);
        Assert.Equal(reaction.UserId, result.UserId);
        Assert.Equal(reaction.ContentId, result.ContentId);
        Assert.Equal(reaction.ContentType, result.ContentType);
        Assert.Equal(reaction.ReactionType, result.ReactionType);
    }

    [Fact]
    public async Task GetSingleAsync_ShouldThrow_WhenReactionDoesNotExist()
    {
        using var factory = new FileRepositoryFactory();

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => factory.ReactionRepository.GetSingleAsync(999));
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnAllReactions()
    {
        using var factory = new FileRepositoryFactory();

        await CreateReactionAsync(factory);
        await CreateReactionAsync(factory, userId: 2);
        await CreateReactionAsync(factory, userId: 3);

        var result = await factory.ReactionRepository.GetManyAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnEmptyList_WhenNoReactionsExist()
    {
        using var factory = new FileRepositoryFactory();

        var result = await factory.ReactionRepository.GetManyAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReaction()
    {
        using var factory = new FileRepositoryFactory();

        var reactionTypes = Enum.GetValues<ReactionType>();

        var reaction = await CreateReactionAsync(
            factory,
            reactionType: reactionTypes[0]);

        reaction.ReactionType = reactionTypes[1];

        await factory.ReactionRepository.UpdateAsync(reaction);

        var result = await factory.ReactionRepository
            .GetSingleAsync(reaction.ReactionId);

        Assert.Equal(reactionTypes[1], result.ReactionType);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveId()
    {
        using var factory = new FileRepositoryFactory();

        var reaction = await CreateReactionAsync(factory);
        int originalId = reaction.ReactionId;

        reaction.ContentId = 2;

        await factory.ReactionRepository.UpdateAsync(reaction);

        var result = await factory.ReactionRepository
            .GetSingleAsync(originalId);

        Assert.Equal(originalId, result.ReactionId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotAffectOtherReactions()
    {
        using var factory = new FileRepositoryFactory();

        var reaction1 = await CreateReactionAsync(factory, contentId: 1);
        var reaction2 = await CreateReactionAsync(factory, contentId: 2);

        reaction1.ContentId = 3;

        await factory.ReactionRepository.UpdateAsync(reaction1);

        var result = await factory.ReactionRepository
            .GetSingleAsync(reaction2.ReactionId);

        Assert.Equal(2, result.ContentId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenReactionDoesNotExist()
    {
        using var factory = new FileRepositoryFactory();

        var reaction = new Reaction
        {
            ReactionId = 999,
            UserId = 1,
            ContentId = 1,
            ContentType = ContentType.Post,
            ReactionType = Enum.GetValues<ReactionType>().First()
        };

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => factory.ReactionRepository.UpdateAsync(reaction));
    }

    [Fact]
    public async Task DeleteAsync_ShouldMakeReactionUnavailable()
    {
        using var factory = new FileRepositoryFactory();

        var reaction = await CreateReactionAsync(factory);

        await factory.ReactionRepository.DeleteAsync(reaction.ReactionId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => factory.ReactionRepository.GetSingleAsync(reaction.ReactionId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotDeleteOtherReactions()
    {
        using var factory = new FileRepositoryFactory();

        var reaction1 = await CreateReactionAsync(factory, userId: 1);
        var reaction2 = await CreateReactionAsync(factory, userId: 2);

        await factory.ReactionRepository.DeleteAsync(reaction1.ReactionId);

        var result = await factory.ReactionRepository
            .GetSingleAsync(reaction2.ReactionId);

        Assert.Equal(reaction2.ReactionId, result.ReactionId);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenReactionDoesNotExist()
    {
        using var factory = new FileRepositoryFactory();

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => factory.ReactionRepository.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenDeletingSameReactionTwice()
    {
        using var factory = new FileRepositoryFactory();

        var reaction = await CreateReactionAsync(factory);

        await factory.ReactionRepository.DeleteAsync(reaction.ReactionId);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => factory.ReactionRepository.DeleteAsync(reaction.ReactionId));
    }

    // --------------------------------------------------
    // GetReactionByUserAndContentAsync
    // --------------------------------------------------

    [Fact]
    public async Task GetReactionByUserAndContentAsync_ShouldReturnMatchingReaction()
    {
        using var factory = new FileRepositoryFactory();

        var reaction = await CreateReactionAsync(
            factory,
            userId: 1,
            contentId: 10,
            contentType: ContentType.Post);

        var result = await factory.ReactionRepository
            .GetReactionByUserAndContentAsync(
                1, 10, ContentType.Post);

        Assert.NotNull(result);
        Assert.Equal(reaction.ReactionId, result.ReactionId);
    }

    [Fact]
    public async Task GetReactionByUserAndContentAsync_ShouldReturnNull_WhenReactionDoesNotExist()
    {
        using var factory = new FileRepositoryFactory();

        var result = await factory.ReactionRepository
            .GetReactionByUserAndContentAsync(
                1, 10, ContentType.Post);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetReactionByUserAndContentAsync_ShouldMatchUserId()
    {
        using var factory = new FileRepositoryFactory();

        await CreateReactionAsync(
            factory,
            userId: 1,
            contentId: 10);

        var result = await factory.ReactionRepository
            .GetReactionByUserAndContentAsync(
                2, 10, ContentType.Post);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetReactionByUserAndContentAsync_ShouldMatchContentId()
    {
        using var factory = new FileRepositoryFactory();

        await CreateReactionAsync(
            factory,
            userId: 1,
            contentId: 10);

        var result = await factory.ReactionRepository
            .GetReactionByUserAndContentAsync(
                1, 20, ContentType.Post);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetReactionByUserAndContentAsync_ShouldMatchContentType()
    {
        using var factory = new FileRepositoryFactory();

        await CreateReactionAsync(
            factory,
            userId: 1,
            contentId: 10,
            contentType: ContentType.Post);

        var result = await factory.ReactionRepository
            .GetReactionByUserAndContentAsync(
                1, 10, ContentType.Comment);

        Assert.Null(result);
    }

    // --------------------------------------------------
    // GetReactionCountsAsync
    // --------------------------------------------------

    [Fact]
    public async Task GetReactionCountsAsync_ShouldReturnEmptyDictionary_WhenNoReactionsExist()
    {
        using var factory = new FileRepositoryFactory();

        var result = await factory.ReactionRepository
            .GetReactionCountsAsync(1, ContentType.Post);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReactionCountsAsync_ShouldCountReactionsByType()
    {
        using var factory = new FileRepositoryFactory();

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

        var result = await factory.ReactionRepository
            .GetReactionCountsAsync(1, ContentType.Post);

        Assert.Equal(2, result[reactionTypes[0]]);
        Assert.Equal(3, result[reactionTypes[1]]);
    }

    [Fact]
    public async Task GetReactionCountsAsync_ShouldOnlyCountReactionsForSpecifiedContentId()
    {
        using var factory = new FileRepositoryFactory();

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

        var result = await factory.ReactionRepository
            .GetReactionCountsAsync(1, ContentType.Post);

        Assert.Equal(2, result[reactionTypes[0]]);
    }

    [Fact]
    public async Task GetReactionCountsAsync_ShouldOnlyCountReactionsForSpecifiedContentType()
    {
        using var factory = new FileRepositoryFactory();

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

        var result = await factory.ReactionRepository
            .GetReactionCountsAsync(1, ContentType.Post);

        Assert.Equal(1, result[reactionType]);
    }

    [Fact]
    public async Task GetReactionCountsAsync_ShouldReturnEmptyDictionary_WhenOnlyOtherContentHasReactions()
    {
        using var factory = new FileRepositoryFactory();

        await CreateReactionAsync(factory,
            contentId: 2,
            contentType: ContentType.Post);

        var result = await factory.ReactionRepository
            .GetReactionCountsAsync(1, ContentType.Post);

        Assert.Empty(result);
    }
}
