using System;
using System.Collections.Generic;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BackgroundJob> BackgroundJobs { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BackgroundJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Backgrou__3214EC07794C5524");

            entity.HasIndex(e => new { e.Priority, e.CreatedAt, e.Id }, "IX_BackgroundJobs_Pending")
                .IsDescending(true, false, false)
                .HasFilter("([Status]=(0))");

            entity.HasIndex(e => e.LockedUntil, "IX_BackgroundJobs_Processing").HasFilter("([Status]=(1))");

            entity.HasIndex(e => new { e.BatchRunId, e.OfficeId }, "UQ_BackgroundJobs_Batch_Office").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.WorkerId).HasMaxLength(100);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasMany(d => d.Roles).WithMany(p => p.Permissions)
                .UsingEntity<Dictionary<string, object>>(
                    "PermissionRole",
                    r => r.HasOne<Role>().WithMany().HasForeignKey("RolesId"),
                    l => l.HasOne<Permission>().WithMany().HasForeignKey("PermissionsId"),
                    j =>
                    {
                        j.HasKey("PermissionsId", "RolesId");
                        j.ToTable("PermissionRole");
                        j.HasIndex(new[] { "RolesId" }, "IX_PermissionRole_RolesId");
                    });
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasMany(d => d.Users).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RoleUser",
                    r => r.HasOne<User>().WithMany().HasForeignKey("UsersId"),
                    l => l.HasOne<Role>().WithMany().HasForeignKey("RolesId"),
                    j =>
                    {
                        j.HasKey("RolesId", "UsersId");
                        j.ToTable("RoleUser");
                        j.HasIndex(new[] { "UsersId" }, "IX_RoleUser_UsersId");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
