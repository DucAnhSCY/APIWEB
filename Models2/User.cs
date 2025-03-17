using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace diendan2.Models2;

[Index("Email", Name = "UQ__Users__A9D10534FA026530", IsUnique = true)]
[Index("Username", Name = "UQ__Users__F3DBC5724B0131F4", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

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

    [Column("role")]
    [StringLength(10)]
    [Unicode(false)]
    public string Role { get; set; } = null!;

    [Column("status")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column("joinDate", TypeName = "datetime")]
    public DateTime? JoinDate { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("User")]
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    [InverseProperty("User")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    [InverseProperty("User")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [InverseProperty("User")]
    public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();
}
