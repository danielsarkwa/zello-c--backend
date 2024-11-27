using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Zello.Domain.Enums;

namespace Zello.Domain.Entities;

// Old task entity
[Table("tasks")]
public class WorkTask {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("status")]
    public CurrentTaskStatus Status { get; set; }

    [Column("priority")]
    public Priority Priority { get; set; }

    [Column("deadline")]
    public DateTime? Deadline { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Column("list_id")]
    public Guid ListId { get; set; }

    // Navigation properties
    [ForeignKey("ProjectId")]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey("ListId")]
    public virtual TaskList List { get; set; } = null!;

    public virtual ICollection<TaskAssignee> Assignees { get; set; } = new List<TaskAssignee>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
