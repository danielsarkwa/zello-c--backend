namespace Zello.Application.Features.Tasks.Models;

/// <summary>
/// Request model for moving a task to a different list
/// </summary>
/// <example>
/// {
///     "targetListId": "123e4567-e89b-12d3-a456-426614174000"
/// }
/// </example>
public sealed record MoveTaskRequest(
    Guid TargetListId
);
