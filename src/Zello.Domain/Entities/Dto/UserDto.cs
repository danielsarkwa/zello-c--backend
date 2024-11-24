using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zello.Domain.Entities.Api.User;

namespace Zello.Domain.Entities.Dto;

[Table("users")]
public class UserDto {
    [Key]
    [Column("id")]
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("username")]
    [JsonProperty("username")]
    public string Username { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    [JsonProperty("name")]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    [Column("email")]
    [JsonProperty("email")]
    public string Email { get; set; }

    [Required]
    [Column("password_hash")]
    [JsonProperty("password_hash")]
    [JsonIgnore] // Security: never serialize password hash
    public string PasswordHash { get; set; }

    [Column("is_active")]
    [JsonProperty("is_active")]
    public bool IsActive { get; set; }

    [Column("created_date")]
    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("workspace_memberships")]
    public virtual ICollection<WorkspaceMemberDto> WorkspaceMembers { get; set; }

    [JsonProperty("assigned_tasks")]
    public virtual ICollection<TaskAssigneeDto> AssignedTasks { get; set; }

    [JsonProperty("comments")]
    public virtual ICollection<CommentDto> Comments { get; set; }


    [Required]
    [Column("access_level")]
    [JsonProperty("access_level")]
    [JsonConverter(typeof(StringEnumConverter))]
    public AccessLevel AccessLevel { get; set; }

    public UserDto(Guid id, string username, string email, string name) {
        AccessLevel = AccessLevel.Guest;
        Id = id;
        Username = username;
        Email = email;
        Name = name;
        PasswordHash = string.Empty; // Initialize PasswordHash
        IsActive = true;
        CreatedDate = DateTime.UtcNow;
        WorkspaceMembers = new List<WorkspaceMemberDto>();
        AssignedTasks = new List<TaskAssigneeDto>();
        Comments = new List<CommentDto>();
    }

    // Required by EF Core
    protected UserDto() {
        AccessLevel = AccessLevel.Guest;
        Username = string.Empty;
        Name = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        CreatedDate = DateTime.UtcNow;
        WorkspaceMembers = new List<WorkspaceMemberDto>();
        AssignedTasks = new List<TaskAssigneeDto>();
        Comments = new List<CommentDto>();
    }
}
