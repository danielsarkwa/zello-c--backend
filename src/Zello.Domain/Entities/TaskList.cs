using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zello.Domain.Entities;

// Old list entity
[Table("lists")]
public class TaskList {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("position")]
    public int Position { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey("ProjectId")]
    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
}
