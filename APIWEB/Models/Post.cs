using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

[Table("Post")]
public partial class Post
{
    [Key]
    [Column("post_id")]
    public int PostId { get; set; }

    [Column("thread_id")]
    public int ThreadId { get; set; }

    [Column("RegUser_id")]
    public int? RegUserId { get; set; }

    [Column("Mod_id")]
    public int? ModId { get; set; }

    [Column("content", TypeName = "text")]
    public string Content { get; set; } = null!;

    [Column("createdAt", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Post")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Post")]
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    [ForeignKey("ModId")]
    [InverseProperty("Posts")]
    public virtual Moderator? Mod { get; set; }

    [ForeignKey("RegUserId")]
    [InverseProperty("Posts")]
    public virtual RegisteredUser? RegUser { get; set; }

    [InverseProperty("Post")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [ForeignKey("ThreadId")]
    [InverseProperty("Posts")]
    public virtual Thread Thread { get; set; } = null!;
}
