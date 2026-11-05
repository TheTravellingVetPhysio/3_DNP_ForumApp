using ApiContracts;

namespace BlazorApp.Services;

public interface IReactionService
{
    Task AddReactionAsync(CreateReactionRequest request);

    Task UpdateReactionAsync(UpdateReactionRequest request);

    Task RemoveReactionAsync(DeleteReactionRequest request);

    Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(int contentId, ContentType contentType);

    Task<ReactionType?> GetUserReactionAsync(int userId, int contentId, ContentType contentType);
}
