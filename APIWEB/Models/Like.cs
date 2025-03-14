using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

public partial class Like
{
    [Key]
    [Column("like_id")]
    public int LikeId { get; set; }

    [Column("post_id")]
    public int PostId { get; set; }

    [Column("RegUser_id")]
    public int RegUserId { get; set; }

    [ForeignKey("PostId")]
    [InverseProperty("Likes")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("RegUserId")]
    [InverseProperty("Likes")]
    public virtual RegisteredUser RegUser { get; set; } = null!;
}
