using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zello.Domain.Entities.Dto;

[Table("lists")]
public class ListDto {
    public ListDto() {
        Name = string.Empty;
        Tasks = new List<TaskDto>();
    }

    [Key]
    [Column("id")]
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(100)] // Adding reasonable max length
    [JsonProperty("name")]
    public string Name { get; set; }

    [Column("position")]
    [JsonProperty("position")]
    public int Position { get; set; }

    [Column("created_date")]
    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual ProjectDto? Project { get; set; }

    [JsonProperty("tasks")]
    public virtual ICollection<TaskDto> Tasks { get; set; }
}
