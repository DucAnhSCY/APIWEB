using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

[Table("Comment")]
public partial class Comment
{
    [Key]
    [Column("comment_id")]
    public int CommentId { get; set; }

    [Column("post_id")]
    public int PostId { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }

    [Column("content", TypeName = "text")]
    public string Content { get; set; } = null!;

    [Column("createdAt", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("PostId")]
    [InverseProperty("Comments")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Comments")]
    public virtual User User { get; set; } = null!;
}
