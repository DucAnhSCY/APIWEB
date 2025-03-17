using System;

namespace diendan2;

public class CommentDTO
{
    public int CommentId { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Content { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateCommentDTO
{
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
}

public class UpdateCommentDTO
{
    public string Content { get; set; }
} 