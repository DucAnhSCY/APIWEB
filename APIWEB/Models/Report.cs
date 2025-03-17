using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

[Table("Report")]
public partial class Report
{
    [Key]
    [Column("report_id")]
    public int ReportId { get; set; }

    [Column("post_id")]
    public int PostId { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }

    [Column("reason", TypeName = "text")]
    public string Reason { get; set; } = null!;

    [Column("status")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column("createdAt", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("PostId")]
    [InverseProperty("Reports")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Reports")]
    public virtual User User { get; set; } = null!;
}
