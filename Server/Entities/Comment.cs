using System;

namespace Entities;

public class Comment : Content
{
    public int PostId { get; set; }               // FK -> Post.ContentId
    public int? ParentCommentId { get; set; }      // FK -> Comment.ContentId (nullable! top-level kommentarer har ingen forælder)
}