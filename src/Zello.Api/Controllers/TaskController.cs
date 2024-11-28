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
    private readonly ApplicationDbContext _context;

    public TaskController(ApplicationDbContext context) {
        _context = context;
    }

    // Helper method to check project access
    private async Task<(bool hasAccess, Project? project)> CheckProjectAccess(Guid taskId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);

        var task = await _context.Tasks
            .Include(t => t.List)
            .ThenInclude(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return (false, null);

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         task.List.Project.Members.Any(pm =>
                             pm.WorkspaceMember.UserId == userId
                         );

        return (hasAccess, task.List.Project);
    }

    [HttpGet("{taskId}")]
    [ProducesResponseType(typeof(TaskReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTaskById(Guid taskId) {
        var (hasAccess, _) = await CheckProjectAccess(taskId);
        if (!hasAccess)
            return Forbid("User does not have access to this task");

        var task = await _context.Tasks
            .Include(t => t.Assignees)
            .ThenInclude(a => a.User)
            .Include(t => t.Comments)
            .Include(t => t.List)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        return Ok(TaskReadDto.FromEntity(task));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTasks() {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var query = _context.Tasks
            .Include(t => t.List)
            .ThenInclude(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .Include(t => t.Assignees)
            .ThenInclude(a => a.User)
            .Include(t => t.Comments)
            .AsQueryable();

        // Filter to only show tasks from projects where user is a member or is admin
        if (userAccess != AccessLevel.Admin) {
            query = query.Where(t =>
                t.List.Project.Members.Any(pm =>
                    pm.WorkspaceMember.UserId == userId
                )
            );
        }

        var tasks = await query.ToListAsync();
        return Ok(tasks.Select(TaskReadDto.FromEntity));
    }

    [HttpPut("{taskId}")]
    public async Task<IActionResult>
        UpdateTask(Guid taskId, [FromBody] TaskUpdateDto taskUpdateDto) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (hasAccess, project) = await CheckProjectAccess(taskId);
        if (!hasAccess)
            return Forbid("User does not have access to this task");

        // Additional check for Member access level required for updates
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        var projectMember = project!.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

        if (projectMember?.AccessLevel < AccessLevel.Member && userAccess != AccessLevel.Admin)
            return Forbid("Insufficient permissions to update tasks");

        var existingTask = await _context.Tasks
            .Include(t => t.Assignees)
            .ThenInclude(a => a.User)
            .Include(t => t.Comments)
            .Include(t => t.List)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (existingTask == null)
            return NotFound($"Task with ID {taskId} not found");

        taskUpdateDto.UpdateEntity(existingTask);
        await _context.SaveChangesAsync();

        return Ok(TaskReadDto.FromEntity(existingTask));
    }

    [HttpDelete("{taskId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTask(Guid taskId) {
        var (hasAccess, project) = await CheckProjectAccess(taskId);
        if (!hasAccess)
            return Forbid("User does not have access to this task");

        // Additional check for Member access level required for deletion
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        var projectMember = project!.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

        if (projectMember?.AccessLevel < AccessLevel.Member && userAccess != AccessLevel.Admin)
            return Forbid("Insufficient permissions to delete tasks");

        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{taskId}/move")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MoveTask(Guid taskId, [FromBody] MoveTaskRequest request) {
        var (hasAccess, project) = await CheckProjectAccess(taskId);
        if (!hasAccess)
            return Forbid("User does not have access to this task");

        // Additional check for Member access level required for moving
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        var projectMember = project!.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

        if (projectMember?.AccessLevel < AccessLevel.Member && userAccess != AccessLevel.Admin)
            return Forbid("Insufficient permissions to move tasks");

        var task = await _context.Tasks
            .Include(t => t.Assignees)
            .ThenInclude(a => a.User)
            .Include(t => t.Comments)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        var targetList = await _context.Lists
            .FirstOrDefaultAsync(l => l.Id == request.TargetListId);

        if (targetList == null)
            return NotFound($"Target list with ID {request.TargetListId} not found");

        // Verify lists are in the same project
        if (targetList.ProjectId != project.Id)
            return BadRequest("Cannot move task to a list in a different project");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try {
            task.ListId = request.TargetListId;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(TaskReadDto.FromEntity(task));
        } catch (Exception) {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpPost("{taskId}/assignees")]
    [ProducesResponseType(typeof(TaskAssigneeReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignUserToTask(Guid taskId,
        [FromBody] AssignUserRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (hasAccess, project) = await CheckProjectAccess(taskId);
        if (!hasAccess)
            return Forbid("User does not have access to this task");

        // Additional check for Member access level required for assigning
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        var projectMember = project!.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

        if (projectMember?.AccessLevel < AccessLevel.Member && userAccess != AccessLevel.Admin)
            return Forbid("Insufficient permissions to assign tasks");

        if (request.UserId == Guid.Empty)
            return BadRequest("User ID cannot be empty.");

        var task = await _context.Tasks
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        // Verify assigned user is a project member
        bool isAssignedUserProjectMember = project.Members
            .Any(pm => pm.WorkspaceMember.UserId == request.UserId);

        if (!isAssignedUserProjectMember && userAccess != AccessLevel.Admin)
            return BadRequest("User must be a member of the project to be assigned");

        if (task.Assignees.Any(a => a.UserId == request.UserId))
            return BadRequest("User is already assigned to this task");

        var taskAssignee = new TaskAssignee {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = request.UserId,
            AssignedDate = DateTime.UtcNow
        };

        _context.TaskAssignees.Add(taskAssignee);
        await _context.SaveChangesAsync();

        var createdAssignee = await _context.TaskAssignees
            .Include(ta => ta.Task)
            .Include(ta => ta.User)
            .FirstAsync(ta => ta.Id == taskAssignee.Id);

        return CreatedAtAction(
            nameof(GetTaskById),
            new { taskId },
            TaskAssigneeReadDto.FromEntity(createdAssignee)
        );
    }

    [HttpDelete("{taskId}/assignees/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveTaskAssignee(Guid taskId, Guid userId) {
        var (hasAccess, project) = await CheckProjectAccess(taskId);
        if (!hasAccess)
            return Forbid("User does not have access to this task");

        // Additional check for Member access level required for unassigning
        var currentUserId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        var projectMember = project!.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == currentUserId);

        // Allow users to unassign themselves, otherwise require Member access
        if (currentUserId != userId &&
            projectMember?.AccessLevel < AccessLevel.Member &&
            userAccess != AccessLevel.Admin)
            return Forbid("Insufficient permissions to unassign others from tasks");

        var task = await _context.Tasks
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        var assignee = task.Assignees
            .FirstOrDefault(a => a.UserId == userId);

        if (assignee == null)
            return NotFound($"User {userId} is not assigned to task {taskId}");

        _context.TaskAssignees.Remove(assignee);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{taskId}/assignees")]
    [ProducesResponseType(typeof(IEnumerable<TaskAssigneeReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTaskAssignees(Guid taskId) {
        var (hasAccess, _) = await CheckProjectAccess(taskId);
        if (!hasAccess)
            return Forbid("User does not have access to this task");

        var task = await _context.Tasks
            .Include(t => t.Assignees)
            .ThenInclude(a => a.User)
            .Include(t => t.Assignees)
            .ThenInclude(a => a.Task)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        var assigneeDtos = task.Assignees
            .Select(TaskAssigneeReadDto.FromEntity)
            .ToList();

        return Ok(assigneeDtos);
    }

    [HttpGet("{taskId}/comments")]
    [ProducesResponseType(typeof(IEnumerable<CommentReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTaskComments(Guid taskId) {
        var (hasAccess, _) = await CheckProjectAccess(taskId);
        if (!hasAccess)
            return Forbid("User does not have access to this task");

        var task = await _context.Tasks
            .Include(t => t.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        var comments = task.Comments
            .Select(c => new CommentReadDto {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                Content = c.Content,
                CreatedDate = c.CreatedDate,
                User = UserReadDto.FromEntity(c.User)
            })
            .ToList();

        return Ok(comments);
    }

    [HttpPost("{taskId}/comments")]
    [ProducesResponseType(typeof(CommentReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddTaskComment(Guid taskId,
        [FromBody] AddCommentRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (hasAccess, project) = await CheckProjectAccess(taskId);
        if (!hasAccess)
            return Forbid("User does not have access to this task");

        // Get current user ID
        var userId = ClaimsHelper.GetUserId(User);
        if (userId == null)
            return BadRequest("User ID cannot be null.");

        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        var comment = new Comment {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = userId.Value,
            Content = request.Content,
            CreatedDate = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Reload the comment with user data for the response
        var createdComment = await _context.Comments
            .Include(c => c.User)
            .FirstAsync(c => c.Id == comment.Id);

        return CreatedAtAction(
            nameof(GetTaskComments),
            new { taskId },
            new CommentReadDto {
                Id = createdComment.Id,
                TaskId = createdComment.TaskId,
                UserId = createdComment.UserId,
                Content = createdComment.Content,
                CreatedDate = createdComment.CreatedDate,
                User = UserReadDto.FromEntity(createdComment.User)
            }
        );
    }
}
