using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zello.Domain.Entities;

[Table("task_assignees")]
public class TaskAssignee {
    [Key]
    public Guid Id { get; set; }

    [Column("task_id")]
    public Guid TaskId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("assigned_date")]
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("TaskId")]
    public WorkTask Task { get; set; } = null!;

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
