using System;
using ApiContracts;

namespace BlazorApp.Services;

public interface IPostService
{
    Task<PostDetailsDTO> GetPostDetailsByIdAsync(int postId);

    Task<List<PostOverviewDTO>> GetPostsAsync(
        string? title = null,
        int? userId = null,
        string? username = null,
        int? subforumId = null);

    Task<PostDetailsDTO> CreatePostAsync(CreatePostRequest request);

    Task UpdatePostAsync(int postId, UpdatePostRequest request);

    Task DeletePostAsync(int postId);

}
