using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

[Table("Moderator")]
[Index("Email", Name = "UQ__Moderato__A9D10534DA56AE50", IsUnique = true)]
[Index("Username", Name = "UQ__Moderato__F3DBC5721CBF4AAC", IsUnique = true)]
public partial class Moderator
{
    [Key]
    [Column("Mod_id")]
    public int ModId { get; set; }

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

    [Column("Admin_id")]
    public int? AdminId { get; set; }

    [ForeignKey("AdminId")]
    [InverseProperty("Moderators")]
    public virtual Admin? Admin { get; set; }

    [InverseProperty("Mod")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    [InverseProperty("Mod")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [InverseProperty("Mod")]
    public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();
}
