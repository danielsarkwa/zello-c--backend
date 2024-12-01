using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Application.Dtos;
using Zello.Application.Features.Tasks.Models;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Helpers;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class TaskController : ControllerBase {
    private readonly IWorkTaskService _workTaskService;

    public TaskController(IWorkTaskService workTaskService) {
        _workTaskService = workTaskService;
    }

    /// <summary>
    /// Retrieves a specific task by ID
    /// </summary>
    /// <param name="taskId">The unique identifier of the task</param>
    /// <remarks>
    /// Required permissions:
    /// - Project member access, or
    /// - Admin access
    /// </remarks>
    /// <response code="200">Returns the requested task</response>
    /// <response code="403">User does not have access to this task</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{taskId}")]
    [ProducesResponseType(typeof(TaskReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTaskById(Guid taskId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var task = await _workTaskService.GetTaskByIdAsync(taskId, userId.Value, userAccess);
            return Ok(task);
        } catch (UnauthorizedAccessException) {
            return Forbid("User does not have access to this task");
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all tasks accessible to the user
    /// </summary>
    /// <remarks>
    /// Returns:
    /// - All tasks for Admin users
    /// - Only tasks in projects where the user is a member for non-Admin users
    /// </remarks>
    /// <response code="200">List of accessible tasks</response>
    /// <response code="400">User ID is missing</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTasks() {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID missing");

        var tasks = await _workTaskService.GetAllTasksAsync(userId.Value, userAccess);
        return Ok(tasks);
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="taskId">The unique identifier of the task to update</param>
    /// <param name="taskUpdateDto">Updated task details</param>
    /// <remarks>
    /// Required permissions:
    /// - Project member access, or
    /// - Admin access
    /// </remarks>
    /// <response code="200">Task successfully updated</response>
    /// <response code="400">Invalid update data</response>
    /// <response code="403">User does not have access to this task</response>
    /// <response code="404">Task not found</response>
    [HttpPut("{taskId}")]
    public async Task<IActionResult>
        UpdateTask(Guid taskId, [FromBody] TaskUpdateDto taskUpdateDto) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var task =
                await _workTaskService.UpdateTaskAsync(taskId, taskUpdateDto, userId.Value,
                    userAccess);
            return Ok(task);
        } catch (UnauthorizedAccessException ex) {
            return Forbid(ex.Message);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="taskId">The unique identifier of the task to delete</param>
    /// <remarks>
    /// Required permissions:
    /// - Project member access, or
    /// - Admin access
    /// </remarks>
    /// <response code="204">Task successfully deleted</response>
    /// <response code="403">User does not have access to delete this task</response>
    /// <response code="404">Task not found</response>
    [HttpDelete("{taskId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTask(Guid taskId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            await _workTaskService.DeleteTaskAsync(taskId, userId.Value, userAccess);
            return NoContent();
        } catch (UnauthorizedAccessException ex) {
            return Forbid(ex.Message);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Moves a task to a different list
    /// </summary>
    /// <param name="taskId">The unique identifier of the task to move</param>
    /// <param name="request">Move request containing target list ID</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/Task/{taskId}/move
    ///     {
    ///         "targetListId": "123e4567-e89b-12d3-a456-426614174000"
    ///     }
    /// </remarks>
    /// <response code="200">Task successfully moved</response>
    /// <response code="400">Invalid move request or target list does not exist</response>
    /// <response code="403">User does not have access to move this task</response>
    /// <response code="404">Task not found</response>
    [HttpPost("{taskId}/move")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MoveTask(Guid taskId, [FromBody] MoveTaskRequest request) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var task = await _workTaskService.MoveTaskAsync(taskId, request.TargetListId,
                userId.Value, userAccess);
            return Ok(task);
        } catch (UnauthorizedAccessException ex) {
            return Forbid(ex.Message);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        } catch (InvalidOperationException ex) {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Assigns a user to a task
    /// </summary>
    /// <param name="taskId">The unique identifier of the task</param>
    /// <param name="request">Assignment request containing user ID</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/Task/{taskId}/assignees
    ///     {
    ///         "user_id": "123e4567-e89b-12d3-a456-426614174000"
    ///     }
    /// </remarks>
    /// <response code="201">User successfully assigned to task</response>
    /// <response code="400">Invalid assignment request</response>
    /// <response code="403">Insufficient permissions to assign users</response>
    /// <response code="404">Task or user not found</response>
    [HttpPost("{taskId}/assignees")]
    [ProducesResponseType(typeof(TaskAssigneeReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignUserToTask(Guid taskId,
        [FromBody] AssignUserRequest request) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var assignee =
                await _workTaskService.AssignUserToTaskAsync(taskId, request.UserId, userId.Value,
                    userAccess);
            return CreatedAtAction(nameof(GetTaskById), new { taskId }, assignee);
        } catch (UnauthorizedAccessException ex) {
            return Forbid(ex.Message);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        } catch (InvalidOperationException ex) {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Removes an assignee from a task
    /// </summary>
    /// <param name="taskId">The unique identifier of the task</param>
    /// <param name="userId">The ID of the user to remove from the task</param>
    /// <remarks>
    /// Required permissions:
    /// - Project member access, or
    /// - Admin access
    /// </remarks>
    /// <response code="204">Assignee successfully removed</response>
    /// <response code="403">Insufficient permissions to remove assignee</response>
    /// <response code="404">Task or assignee not found</response>
    [HttpDelete("{taskId}/assignees/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveTaskAssignee(Guid taskId, Guid userId) {
        var currentUserId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (currentUserId == null) return BadRequest("User ID cannot be null.");

        try {
            await _workTaskService.RemoveTaskAssigneeAsync(taskId, userId, currentUserId.Value,
                userAccess);
            return NoContent();
        } catch (UnauthorizedAccessException ex) {
            return Forbid(ex.Message);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all assignees of a task
    /// </summary>
    /// <param name="taskId">The unique identifier of the task</param>
    /// <remarks>
    /// Required permissions:
    /// - Project member access, or
    /// - Admin access
    /// </remarks>
    /// <response code="200">List of task assignees</response>
    /// <response code="403">User does not have access to view task assignees</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{taskId}/assignees")]
    [ProducesResponseType(typeof(IEnumerable<TaskAssigneeReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTaskAssignees(Guid taskId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var assignees =
                await _workTaskService.GetTaskAssigneesAsync(taskId, userId.Value, userAccess);
            return Ok(assignees);
        } catch (UnauthorizedAccessException ex) {
            return Forbid(ex.Message);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all comments on a task
    /// </summary>
    /// <param name="taskId">The unique identifier of the task</param>
    /// <remarks>
    /// Required permissions:
    /// - Project member access, or
    /// - Admin access
    /// </remarks>
    /// <response code="200">List of task comments</response>
    /// <response code="403">User does not have access to view task comments</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{taskId}/comments")]
    [ProducesResponseType(typeof(IEnumerable<CommentReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTaskComments(Guid taskId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var comments =
                await _workTaskService.GetTaskCommentsAsync(taskId, userId.Value, userAccess);
            return Ok(comments);
        } catch (UnauthorizedAccessException ex) {
            return Forbid(ex.Message);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Adds a new comment to a task
    /// </summary>
    /// <param name="taskId">The unique identifier of the task</param>
    /// <param name="request">Comment creation request</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/Task/{taskId}/comments
    ///     {
    ///         "content": "This is a comment on the task"
    ///     }
    /// </remarks>
    /// <response code="201">Comment successfully added</response>
    /// <response code="400">Invalid comment data</response>
    /// <response code="403">User does not have access to comment on this task</response>
    /// <response code="404">Comment not found</response>
    [HttpPost("{taskId}/comments")]
    [ProducesResponseType(typeof(CommentReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddTaskComment(Guid taskId,
        [FromBody] AddCommentRequest request) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var comment = await _workTaskService.AddTaskCommentAsync(taskId, request.Content,
                userId.Value, userAccess);
            return CreatedAtAction(nameof(GetTaskById), new { taskId }, comment);
        } catch (UnauthorizedAccessException ex) {
            return Forbid(ex.Message);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
    }
}
