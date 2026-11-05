using ApiContracts;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp.Services;

public interface ICommentService
{
    Task<List<CommentDetailsDTO>> GetCommentsAsync(
        int? postId = null,
        int? userId = null,
        string? username = null);

    Task CreateCommentAsync(CreateCommentRequest request);

    Task UpdateCommentAsync(int commentId, UpdateCommentRequest request);

}
