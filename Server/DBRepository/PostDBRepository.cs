    // Views - husk at rette PostOverviewDTO alle steder ved ændringer...
    public async Task<List<PostOverviewDTO>> ActivePostsInSubforumOverviewAsync(int subforumId)
    {
        List<Post> posts = GetPostsBySubforumQuery(subforumId).ToList();
        List<int> relevantPostIds = posts.Select(p => p.ContentId).ToList();
        List<Comment> relevantComments = await commentService.GetCommentsByPostIdsAsync(relevantPostIds);
        
        List<User> users = await userService.GetAllUsersAsync();

        List<PostSummary> summaries = posts
            .Select(p => new
            {
                Post = p,
                LatestCommentTime = relevantComments
                    .Where(c => c.PostId == p.ContentId)
                    .Select(c => (DateTimeOffset?)c.Timestamp)
                    .OrderByDescending(t => t)
                    .FirstOrDefault(),
                Username = users.Single(u => u.UserId == p.UserId).Username
            })
            .OrderByDescending(x => x.LatestCommentTime ?? x.Post.Timestamp)
            .Take(20)
            .Select(x => new PostOverviewDTO
            {
                PostId = x.Post.ContentId,
                Title = x.Post.Title,
                UserId = x.Post.UserId,
                Username = x.Username,
                CreatedAt = x.Post.Timestamp,
                LastActivityAt = x.LatestCommentTime ?? x.Post.Timestamp
            })
            .ToList();

        return summaries;
    }

    public async Task<List<PostOverviewDTO>> GetPostsOverviewByUserAsync(int userId)
    {
        List<Post> posts = GetPostsByUserQuery(userId).ToList();
        List<int> relevantPostIds = posts.Select(p => p.ContentId).ToList();

        List<Comment> relevantComments = await commentService.GetCommentsByPostIdsAsync(relevantPostIds);
        List<User> users = await userService.GetAllUsersAsync();

        List<PostOverviewDTO> summaries = posts
            .Select(p => new
            {
                Post = p,
                LatestCommentTime = relevantComments
                    .Where(c => c.PostId == p.ContentId)
                    .Select(c => (DateTimeOffset?)c.Timestamp)
                    .OrderByDescending(t => t)
                    .FirstOrDefault(),
                Username = users.Single(u => u.UserId == p.UserId).Username
            })
            .Select(x => new PostOverviewDTO
            {
                PostId = x.Post.ContentId,
                Title = x.Post.Title,
                UserId = x.Post.UserId,
                Username = x.Username,
                CreatedAt = x.Post.Timestamp,
                LastActivityAt = x.LatestCommentTime ?? x.Post.Timestamp
            })
            .ToList();

        return summaries;
    }

    public async Task<PostDetailsDTO> GetPostDetailsAsync(int postId)
    {
        Post post = await postRepository.GetSingleAsync(postId);
        User postUser = await userService.GetSingleAsync(post.UserId);

        Dictionary<ReactionType, int> reactionCounts =
                await reactionService.GetReactionCountsAsync(post.ContentId, ContentType.Post);

        List<CommentDetailsDTO> commentDetailsDTOs = await commentService.GetCommentDetailsForPostAsync(postId);

        return new PostDetailsDTO
        {
            PostId = post.ContentId,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            Username = postUser.Username,
            CreatedAt = post.Timestamp,
            ReactionCounts = reactionCounts,
            Comments = commentDetailsDTOs
        };
    }