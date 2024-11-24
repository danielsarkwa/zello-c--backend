using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zello.Domain.Entities.Api.User;

namespace Zello.Domain.Entities.Dto;

[Table("project_members")]
public class ProjectMemberDto {
    public ProjectMemberDto() {
        Project = null!;
        WorkspaceMember = null!;
    }

    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Column("workspace_member_id")]
    public Guid WorkspaceMemberId { get; set; }

    [Column("access_level")]
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Member; // Default to Member

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("ProjectId")]
    public virtual ProjectDto Project { get; set; }

    [ForeignKey("WorkspaceMemberId")]
    public virtual WorkspaceMemberDto WorkspaceMember { get; set; }
}
