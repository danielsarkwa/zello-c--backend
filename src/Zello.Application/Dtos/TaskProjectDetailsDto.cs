namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object containing essential task and project relationship information.
/// Used primarily for authorization checks and project access validation.
/// </summary>
/// <example>
/// {
///     "taskId": "123e4567-e89b-12d3-a456-426614174000",
///     "projectId": "123e4567-e89b-12d3-a456-426614174001"
/// }
/// </example>
public class TaskProjectDetailsDto {
    /// <summary>
    /// The unique identifier of the project that contains the task
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// The unique identifier of the task
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid TaskId { get; set; }
}
