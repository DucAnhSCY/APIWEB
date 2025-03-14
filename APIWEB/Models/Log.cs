using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

public partial class Log
{
    [Key]
    public int LogId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string LogLevel { get; set; } = null!;

    [Column(TypeName = "text")]
    public string Message { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
}
