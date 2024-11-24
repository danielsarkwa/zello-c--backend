using System.ComponentModel.DataAnnotations;
using Zello.Domain.Enums;

namespace Zello.Application.Features.Tasks.Models;

public record UpdateTaskRequest(
    [Required]
    [MaxLength(100)]
    string Name = "",
    [MaxLength(500)]
    string Description = "",
    CurrentTaskStatus Status = default,
    Priority Priority = default,
    DateTime? Deadline = null
);
