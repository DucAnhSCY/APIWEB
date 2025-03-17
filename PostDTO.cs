using System;
using System.Collections.Generic;

namespace diendan2;

public class PostDTO
{
    public int PostId { get; set; }
    public int ThreadId { get; set; }
    public string ThreadTitle { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; }
    public string Content { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int CommentCount { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
}

public class CreatePostDTO
{
    public int ThreadId { get; set; }
    public int? UserId { get; set; }
    public string Content { get; set; }
}

public class UpdatePostDTO
{
    public string Content { get; set; }
}

public class PostDetailDTO : PostDTO
{
    public List<CommentDTO> Comments { get; set; }
    public List<ReportDTO> Reports { get; set; }
} 