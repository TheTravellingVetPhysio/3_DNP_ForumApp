using ApiContracts;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp.Services;

public interface ICommentService
{
    Task<List<CommentDetailsDTO>> GetCommentsAsync(
        int? postId,
        int? userId,
        string? username);

    Task CreateCommentAsync(CreateCommentRequest request);

    Task UpdateCommentAsync(int commentId, UpdateCommentRequest request);

}
