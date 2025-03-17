using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

[Table("Users")]
[Index("Username", Name = "UQ__Users__F3DBC572", IsUnique = true)]
[Index("Email", Name = "UQ__Users__A9D10534", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string Role { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? JoinDate { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();

    [InverseProperty("User")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    [InverseProperty("User")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("User")]
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    [InverseProperty("User")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
} 