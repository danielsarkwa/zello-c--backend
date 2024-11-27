using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;

namespace Zello.Infrastructure.Data;

public class ApplicationDbContext : DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<TaskList> Lists { get; set; }
    public DbSet<WorkTask> Tasks { get; set; }
    public DbSet<TaskAssignee> TaskAssignees { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // Configure enum conversions using string values
        modelBuilder.Entity<User>()
            .Property(e => e.AccessLevel)
            .HasConversion<string>();

        modelBuilder.Entity<Project>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<WorkTask>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<WorkTask>()
            .Property(e => e.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<WorkspaceMember>()
            .Property(e => e.AccessLevel)
            .HasConversion<string>();

        modelBuilder.Entity<ProjectMember>()
            .Property(e => e.AccessLevel)
            .HasConversion<string>();

        // Configure indexes
        modelBuilder.Entity<WorkspaceMember>()
            .HasIndex(wm => new { wm.WorkspaceId, wm.UserId })
            .IsUnique();

        modelBuilder.Entity<ProjectMember>()
            .HasIndex(pm => new { pm.ProjectId, pm.WorkspaceMemberId })
            .IsUnique();

        modelBuilder.Entity<TaskAssignee>()
            .HasIndex(ta => new { ta.TaskId, ta.UserId })
            .IsUnique();

        // Configure cascading deletes
        modelBuilder.Entity<Workspace>()
            .HasMany(w => w.Projects)
            .WithOne(p => p.Workspace)
            .HasForeignKey(p => p.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Lists)
            .WithOne(l => l.Project)
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskList>()
            .HasMany(l => l.Tasks)
            .WithOne(t => t.List)
            .HasForeignKey(t => t.ListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
