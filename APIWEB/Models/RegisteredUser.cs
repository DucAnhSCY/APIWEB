using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

[Table("RegisteredUser")]
[Index("Email", Name = "UQ__Register__A9D10534ED092508", IsUnique = true)]
[Index("Username", Name = "UQ__Register__F3DBC5724F766C73", IsUnique = true)]
public partial class RegisteredUser
{
    [Key]
    [Column("RegUser_id")]
    public int RegUserId { get; set; }

    [Column("username")]
    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column("password")]
    [StringLength(255)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    [Column("status")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column("joinDate", TypeName = "datetime")]
    public DateTime? JoinDate { get; set; }

    [InverseProperty("RegUser")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("RegUser")]
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    [InverseProperty("RegUser")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    [InverseProperty("RegUser")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [InverseProperty("RegUser")]
    public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();
}
