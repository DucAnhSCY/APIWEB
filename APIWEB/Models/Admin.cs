using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

[Table("Admin")]
[Index("Email", Name = "UQ__Admin__A9D105346850B775", IsUnique = true)]
[Index("Username", Name = "UQ__Admin__F3DBC572A1D3DEC5", IsUnique = true)]
public partial class Admin
{
    [Key]
    [Column("Admin_id")]
    public int AdminId { get; set; }

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

    [InverseProperty("Admin")]
    public virtual ICollection<Moderator> Moderators { get; set; } = new List<Moderator>();
}
