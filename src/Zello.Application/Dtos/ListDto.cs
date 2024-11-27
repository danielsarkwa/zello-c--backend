using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Application.Dtos;
using Zello.Domain.Entities;

namespace Zello.Application.Dtos;

public class ListReadDto {
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Position { get; set; }
    public DateTime CreatedDate { get; set; }

    public static ListReadDto FromEntity(TaskList list) {
        return new ListReadDto {
            Id = list.Id,
            ProjectId = list.ProjectId,
            Name = list.Name,
            Position = list.Position,
            CreatedDate = list.CreatedDate,
        };
    }
}

public class ListCreateDto {
    [Required]
    [RegularExpression(@"^[{(]?[0-9A-Fa-f]{8}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{12}[)}]?$", ErrorMessage = "Invalid GUID format.")]
    [JsonProperty("projectId")]
    public Guid ProjectId { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Position must be a non-negative value.")]
    [JsonProperty("position")]
    public int Position { get; set; }

    public TaskList ToEntity() {
        return new TaskList {
            Id = Guid.NewGuid(),
            ProjectId = ProjectId,
            Name = Name,
            Position = Position,
            CreatedDate = DateTime.UtcNow,
        };
    }
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
