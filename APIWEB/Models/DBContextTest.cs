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

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Guest> Guests { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Moderator> Moderators { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<RegisteredUser> RegisteredUsers { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Thread> Threads { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=cmcsv.ric.vn,10000;Initial Catalog=N10_NHOM6;Persist Security Info=True;User ID=cmcsv;Password=cM!@#2025;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admin__4A311D2F5ACA6020");

            entity.Property(e => e.JoinDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("active");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__D5B1EDEC7E799583");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__E7957687E73EDC12");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comment__post_id__43D61337");

            entity.HasOne(d => d.RegUser).WithMany(p => p.Comments).HasConstraintName("FK__Comment__RegUser__44CA3770");
        });

        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.GuestId).HasName("PK__Guest__CB8A01EBE6FF9348");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__Likes__992C7930DFF21408");

            entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Likes__post_id__47A6A41B");

            entity.HasOne(d => d.RegUser).WithMany(p => p.Likes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Likes__RegUser_i__489AC854");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Logs__5E5486488FCD1843");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Moderator>(entity =>
        {
            entity.HasKey(e => e.ModId).HasName("PK__Moderato__D5F37643A11E1F5D");

            entity.Property(e => e.JoinDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("active");

            entity.HasOne(d => d.Admin).WithMany(p => p.Moderators).HasConstraintName("FK__Moderator__Admin__2FCF1A8A");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Post__3ED787660527EB70");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Mod).WithMany(p => p.Posts).HasConstraintName("FK__Post__Mod_id__40058253");

            entity.HasOne(d => d.RegUser).WithMany(p => p.Posts).HasConstraintName("FK__Post__RegUser_id__3F115E1A");

            entity.HasOne(d => d.Thread).WithMany(p => p.Posts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Post__thread_id__3E1D39E1");
        });

        modelBuilder.Entity<RegisteredUser>(entity =>
        {
            entity.HasKey(e => e.RegUserId).HasName("PK__Register__7C1CD14B6D1730C2");

            entity.Property(e => e.JoinDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("active");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Report__779B7C586C52CD0E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("pending");

            entity.HasOne(d => d.Mod).WithMany(p => p.Reports).HasConstraintName("FK__Report__Mod_id__503BEA1C");

            entity.HasOne(d => d.Post).WithMany(p => p.Reports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Report__post_id__4E53A1AA");

            entity.HasOne(d => d.RegUser).WithMany(p => p.Reports).HasConstraintName("FK__Report__RegUser___4F47C5E3");
        });

        modelBuilder.Entity<Thread>(entity =>
        {
            entity.HasKey(e => e.ThreadId).HasName("PK__Thread__7411E2F00F97B04B");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Category).WithMany(p => p.Threads)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Thread__category__3864608B");

            entity.HasOne(d => d.Mod).WithMany(p => p.Threads).HasConstraintName("FK__Thread__Mod_id__3A4CA8FD");

            entity.HasOne(d => d.RegUser).WithMany(p => p.Threads).HasConstraintName("FK__Thread__RegUser___395884C4");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
