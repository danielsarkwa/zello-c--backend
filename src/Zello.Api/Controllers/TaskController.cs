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

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTasks() {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID missing");

        var tasks = await _workTaskService.GetAllTasksAsync(userId.Value, userAccess);
        return Ok(tasks);
    }

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
