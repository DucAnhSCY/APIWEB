using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace diendan2.Models2;

public partial class DBContextTest2 : DbContext
{
    public DBContextTest2()
    {
    }

    public DBContextTest2(DbContextOptions<DBContextTest2> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Thread> Threads { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DUCANHLAPTOP;Initial Catalog=diendan;Persist Security Info=True;User ID=sa;Password=2005;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__D5B1EDEC0A75A5E3");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__E7957687031D2527");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments).HasConstraintName("FK__Comment__post_id__27F8EE98");

            entity.HasOne(d => d.User).WithMany(p => p.Comments).HasConstraintName("FK__Comment__UserId__28ED12D1");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__Likes__992C7930F109DFFF");

            entity.HasOne(d => d.Post).WithMany(p => p.Likes).HasConstraintName("FK__Likes__post_id__2BC97F7C");

            entity.HasOne(d => d.User).WithMany(p => p.Likes).HasConstraintName("FK__Likes__UserId__2CBDA3B5");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Post__3ED78766EEC420C7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Thread).WithMany(p => p.Posts).HasConstraintName("FK__Post__thread_id__2334397B");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Post__UserId__24285DB4");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Report__779B7C585397B6B6");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("pending");

            entity.HasOne(d => d.Post).WithMany(p => p.Reports).HasConstraintName("FK__Report__post_id__32767D0B");

            entity.HasOne(d => d.User).WithMany(p => p.Reports).HasConstraintName("FK__Report__UserId__336AA144");
        });

        modelBuilder.Entity<Thread>(entity =>
        {
            entity.HasKey(e => e.ThreadId).HasName("PK__Thread__7411E2F015BD206D");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Category).WithMany(p => p.Threads).HasConstraintName("FK__Thread__category__1E6F845E");

            entity.HasOne(d => d.User).WithMany(p => p.Threads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Thread__UserId__1F63A897");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CCE656C4D");

            entity.Property(e => e.JoinDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("active");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
