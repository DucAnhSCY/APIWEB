using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace APIWEB.Models;

public partial class DBContextTest : DbContext
{
    public DBContextTest()
    {
    }

    public DBContextTest(DbContextOptions<DBContextTest> options)
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
        => optionsBuilder.UseSqlServer("Data Source=cmcsv.ric.vn,10000;Initial Catalog=N10_NHOM6;Persist Security Info=True;User ID=cmcsv;Password=cM!@#2025;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__D5B1EDEC");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__E7957687");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Comment__post_id");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Comment__UserId");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__Likes__992C7930");

            entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Likes__post_id");

            entity.HasOne(d => d.User).WithMany(p => p.Likes)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Likes__UserId");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Post__3ED78766");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Post__UserId");

            entity.HasOne(d => d.Thread).WithMany(p => p.Posts)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Post__thread_id");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Report__779B7C58");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("pending");

            entity.HasOne(d => d.Post).WithMany(p => p.Reports)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Report__post_id");

            entity.HasOne(d => d.User).WithMany(p => p.Reports)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Report__UserId");
        });

        modelBuilder.Entity<Thread>(entity =>
        {
            entity.HasKey(e => e.ThreadId).HasName("PK__Thread__7411E2F0");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Category).WithMany(p => p.Threads)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Thread__category");

            entity.HasOne(d => d.User).WithMany(p => p.Threads)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__Thread__UserId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C");
            entity.Property(e => e.JoinDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("active");
            entity.Property(e => e.Role).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
