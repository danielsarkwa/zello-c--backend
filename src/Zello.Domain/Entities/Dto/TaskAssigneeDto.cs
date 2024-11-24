using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zello.Domain.Entities.Dto;

public class TaskAssigneeDto {
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("task_id")]
    public Guid TaskId { get; set; }

    [JsonProperty("user_id")]
    public Guid UserId { get; set; }

    [JsonProperty("assigned_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public TaskDto Task { get; set; } = null!;

    [JsonIgnore]
    public UserDto User { get; set; } = null!;
}
