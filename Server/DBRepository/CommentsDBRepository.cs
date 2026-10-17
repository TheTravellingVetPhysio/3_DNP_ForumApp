// Getters
    public async Task<Comment> GetCommentByIdAsync(int commentId)
    {
        return await commentRepository.GetSingleAsync(commentId);
    }

    public Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        List<Comment> comments = commentRepository.GetManyAsync()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.Timestamp)
            .ToList();

        return Task.FromResult(comments);
    }

    public Task<List<Comment>> GetCommentsByPostIdsAsync(List<int> postIds)
    {
        List<Comment> comments = commentRepository.GetManyAsync()
            .Where(c => postIds.Contains(c.PostId))
            .OrderBy(c => c.Timestamp)
            .ToList();

        return Task.FromResult(comments);
    }

    public Task<int> GetCommentCountByPostIdAsync(int postId)
    {
        int commentCount = commentRepository.GetManyAsync()
            .Where(c => c.PostId == postId)
            .Count();

        return Task.FromResult(commentCount);
    }

    // Views
    public async Task<List<CommentDetailsDTO>> GetCommentDetailsForPostAsync(int postId)
    {
        List<Comment> comments = await GetCommentsByPostIdAsync(postId);

        List<CommentDetailsDTO> result = new();
        foreach (Comment comment in comments)
        {
            User commentUser = await userService.GetUserByIdAsync(comment.UserId);

            Dictionary<ReactionType, int> reactionCounts =
                await reactionService.GetReactionCountsAsync(comment.ContentId, ContentType.Comment);

            result.Add(new CommentDetailsDTO
            {
                CommentId = comment.ContentId,
                Body = comment.Body,
                UserId = comment.UserId,
                Username = commentUser.Username,
                CreatedAt = comment.Timestamp,
                ParentCommentId = comment.ParentCommentId,
                ReactionCounts = reactionCounts
            });
        }

        return result;
    }