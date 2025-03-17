using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

[Table("Likes")]
public partial class Like
{
    [Key]
    [Column("like_id")]
    public int LikeId { get; set; }

    [Column("post_id")]
    public int PostId { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }

    [ForeignKey("PostId")]
    [InverseProperty("Likes")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Likes")]
    public virtual User User { get; set; } = null!;
}
