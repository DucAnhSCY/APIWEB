using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace diendan2.Models2;

[Index("Name", Name = "UQ__Categori__72E12F1B21F8C79D", IsUnique = true)]
public partial class Category
{
    [Key]
    [Column("category_ID")]
    public int CategoryId { get; set; }

    [Column("name")]
    [StringLength(100)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [InverseProperty("Category")]
    public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();
}
