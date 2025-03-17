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

    [Column("UserId")]
    public int? UserId { get; set; }

    [Column("content", TypeName = "text")]
    public string Content { get; set; } = null!;

    [Column("createdAt", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Post")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Post")]
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    [ForeignKey("UserId")]
    [InverseProperty("Posts")]
    public virtual User? User { get; set; }

    [InverseProperty("Post")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [ForeignKey("ThreadId")]
    [InverseProperty("Posts")]
    public virtual Thread Thread { get; set; } = null!;
}
