using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

[Index("Name", Name = "UQ__Categori__72E12F1BAC242E55", IsUnique = true)]
public partial class Category
{
    [Key]
    [Column("category_ID")]
    public int CategoryId { get; set; }

    [NotMapped]
    [JsonIgnore]
    public object? CategoryID { get; internal set; }

    [Column("name")]
    [StringLength(100)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [InverseProperty("Category")]
    public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();
}