using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zello.Domain.Enums;

namespace Zello.Domain.Entities.Dto;

[Table("projects")]
public class ProjectDto {
    public ProjectDto() {
        Name = string.Empty;
        Description = string.Empty;
        StartDate = DateTime.UtcNow;
        Members = new List<ProjectMemberDto>();
        Lists = new List<ListDto>();
    }

    [Key]
    [Column("id")]
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("workspace_id")]
    [Column("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(100)]
    [JsonProperty("name")]
    public string Name { get; set; }

    [Column("description")]
    [MaxLength(500)]
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    [Column("start_date")]
    [JsonProperty("start_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    [JsonProperty("end_date", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? EndDate { get; set; }

    [Column("status")]
    [JsonProperty("status")]
    public ProjectStatus Status { get; set; }

    [Column("created_date")]
    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("updated_date")]
    [JsonProperty("updated_date", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("WorkspaceId")]
    [JsonIgnore]
    public virtual WorkspaceDto? Workspace { get; set; }

    [JsonProperty("members")]
    public virtual ICollection<ProjectMemberDto> Members { get; set; }

    [JsonProperty("lists")]
    public virtual ICollection<ListDto> Lists { get; set; }
}
