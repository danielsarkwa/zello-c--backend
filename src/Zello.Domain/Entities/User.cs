using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

[Table("users")]
public class User {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("username")]
    public required string Username { get; set; }

    [Column("name")]
    public required string Name { get; set; }

    [Column("email")]
    public required string Email { get; set; }

    [Column("password_hash")]
    public required string PasswordHash { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("access_level")]
    public AccessLevel AccessLevel { get; set; }

    // Navigation properties
    public virtual ICollection<WorkspaceMember> WorkspaceMembers { get; set; } = new List<WorkspaceMember>();
    public virtual ICollection<TaskAssignee> AssignedTasks { get; set; } = new List<TaskAssignee>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
