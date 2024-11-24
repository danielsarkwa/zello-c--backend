using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zello.Domain.Entities.Dto;

[Table("comments")]
public class CommentDto {
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("task_id", NullValueHandling = NullValueHandling.Ignore)]
    public Guid? TaskId { get; set; }

    [JsonProperty("user_id")]
    public Guid UserId { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;

    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [JsonProperty("updated_date", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? UpdatedDate { get; set; }

    [JsonIgnore]
    public virtual TaskDto Task { get; set; } = null!;

    [JsonIgnore]
    public virtual UserDto User { get; set; } = null!;
}
