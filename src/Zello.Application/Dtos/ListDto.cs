using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zello.Application.Dtos;
using Zello.Domain.Entities;

namespace Zello.Application.Dtos;

// ListReadDto
/// <summary>
/// Data transfer object for reading list information
/// </summary>
public class ListReadDto {
    /// <summary>
    /// Unique identifier of the list
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the project containing this list
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Name of the list
    /// </summary>
    /// <example>To Do</example>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Position of the list within the project
    /// </summary>
    /// <example>1</example>
    [JsonProperty("position")]
    public int Position { get; set; }

    /// <summary>
    /// Date when the list was created
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Tasks contained within this list
    /// </summary>
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
