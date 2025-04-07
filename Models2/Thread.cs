using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace diendan2.Models2;

[Table("Thread")]
public partial class Thread
{
    [Key]
    [Column("thread_id")]
    public int ThreadId { get; set; }

    [Column("title")]
    [StringLength(255)]
    [Unicode(false)]
    public string Title { get; set; } = null!;

    [Column("content", TypeName = "text")]
    public string Content { get; set; } = null!;

    [Column("createdAt", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("category_ID")]
    public int CategoryId { get; set; }

    public int UserId { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Threads")]
    public virtual Category Category { get; set; } = null!;

    [InverseProperty("Thread")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    [ForeignKey("UserId")]
    [InverseProperty("Threads")]
    public virtual User User { get; set; } = null!;
}
