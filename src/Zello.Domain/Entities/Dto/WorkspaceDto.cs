using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zello.Domain.Entities.Dto;

[Table("workspaces")]
public class WorkspaceDto {
    [Key]
    [Column("id")]
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(100)]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(500)]
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }

    [Column("owner_id")]
    [JsonProperty("owner_id")]
    public Guid OwnerId { get; set; }

    [Column("created_date")]
    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [JsonProperty("projects")]
    public virtual ICollection<ProjectDto> Projects { get; set; } = new List<ProjectDto>();

    [JsonProperty("members")]
    public virtual ICollection<WorkspaceMemberDto> Members { get; set; } =
        new List<WorkspaceMemberDto>();
}
