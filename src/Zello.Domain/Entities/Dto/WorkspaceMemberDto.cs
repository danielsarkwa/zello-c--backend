using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zello.Domain.Entities.Api.User;

namespace Zello.Domain.Entities.Dto;

[Table("workspace_members")]
public class WorkspaceMemberDto {
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("access_level")]
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Member;

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("WorkspaceId")]
    public virtual WorkspaceDto Workspace { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual UserDto User { get; set; } = null!;
}
