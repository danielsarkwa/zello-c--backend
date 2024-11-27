using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zello.Domain.Enums;

namespace Zello.Domain.Entities;

[Table("projects")]
public class Project {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("start_date")]
    public DateTime? StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("status")]
    public ProjectStatus Status { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("WorkspaceId")]
    public virtual Workspace Workspace { get; set; } = null!;
    public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public virtual ICollection<TaskList> Lists { get; set; } = new List<TaskList>();
}
