using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zello.Domain.Enums;

namespace Zello.Domain.Entities.Dto;

[Table("tasks")]
public class TaskDto {
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
    public string Description { get; set; } = string.Empty;

    [Column("status")]
    [JsonProperty("status")]
    public CurrentTaskStatus Status { get; set; }

    [Column("priority")]
    [JsonProperty("priority")]
    public Priority Priority { get; set; }

    [Column("deadline")]
    [JsonProperty("deadline", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? Deadline { get; set; }

    [Column("created_date")]
    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("updated_date")]
    [JsonProperty("updated_date", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? UpdatedDate { get; set; }

    [Column("project_id")]
    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    [Column("list_id")]
    [JsonProperty("list_id")]
    public Guid ListId { get; set; }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual ProjectDto Project { get; set; } = null!;

    [ForeignKey("ListId")]
    [JsonIgnore]
    public virtual ListDto List { get; set; } = null!;

    [JsonProperty("assignees")]
    public virtual ICollection<TaskAssigneeDto> Assignees { get; set; } =
        new List<TaskAssigneeDto>();

    [JsonProperty("comments")]
    public virtual ICollection<CommentDto> Comments { get; set; } = new List<CommentDto>();
}
