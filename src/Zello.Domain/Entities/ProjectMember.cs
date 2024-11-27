using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zello.Domain.Entities.Api.User;

namespace Zello.Domain.Entities;

[Table("project_members")]
public class ProjectMember {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Column("workspace_member_id")]
    public Guid WorkspaceMemberId { get; set; }

    [Column("access_level")]
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Member;

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey(nameof(WorkspaceMemberId))]
    public virtual WorkspaceMember WorkspaceMember { get; set; } = null!;
}
