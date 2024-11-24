using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zello.Domain.Enums;

namespace Zello.Domain.Entities.Dto;

public class CreateTaskDto {
    public CreateTaskDto() {
        Name = string.Empty;
        Description = string.Empty;
    }

    [Required]
    [MaxLength(100)]
    [JsonProperty("name")]
    public string Name { get; set; }

    [MaxLength(500)]
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    [JsonProperty("status")]
    [JsonConverter(typeof(StringEnumConverter))]
    public CurrentTaskStatus Status { get; set; }

    [JsonProperty("priority")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Priority Priority { get; set; }

    [JsonProperty("deadline", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? Deadline { get; set; }
}
