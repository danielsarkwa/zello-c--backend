using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zello.Application.Dtos;
using Zello.Domain.Entities;

namespace Zello.Application.Dtos;

public class ListReadDto {
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("position")]
    public int Position { get; set; }

    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("tasks")]
    public IEnumerable<TaskReadDto> Tasks { get; set; } = new List<TaskReadDto>();

    public static ListReadDto FromEntity(TaskList list) {
        return new ListReadDto {
            Id = list.Id,
            ProjectId = list.ProjectId,
            Name = list.Name,
            Position = list.Position,
            CreatedDate = list.CreatedDate,
            Tasks = list.Tasks.Select(TaskReadDto.FromEntity).ToList()
        };
    }
}

public class ListCreateDto {
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("tasks")]
    public ICollection<TaskCreateDto>? Tasks { get; set; }
}

public class ListUpdateDto {
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("Name")]
    public string? Name { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Position must be a non-negative value.")]
    [JsonProperty("position")]
    public int Position { get; set; }

    public TaskList ToEntity(TaskList list) {
        list.Name = Name ?? list.Name;
        list.Position = Position;
        return list;
    }
}
