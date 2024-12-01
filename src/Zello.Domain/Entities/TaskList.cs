using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zello.Domain.Entities;

// Old list entity
[Table("lists")]
public class TaskList {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("position")]
    public int Position { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("ProjectId")]
    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
}
