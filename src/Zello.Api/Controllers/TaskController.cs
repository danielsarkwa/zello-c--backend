using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Application.Dtos;
using Zello.Application.Features.Tasks.Models;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Helpers;
using Zello.Infrastructure.TestingDataStorage;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class TaskController : ControllerBase {
    private readonly ApplicationDbContext _context;

    public TaskController(ApplicationDbContext context) {
        _context = context;
    }

    [HttpGet("{taskId}")]
    [ProducesResponseType(typeof(TaskReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(Guid taskId) {
        var task = await _context.Tasks
            .Include(t => t.Assignees)
            .ThenInclude(a => a.User)
            .Include(t => t.Comments)
            .Include(t => t.List)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        var taskDto = new TaskReadDto {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            Deadline = task.Deadline,
            CreatedDate = task.CreatedDate,
            ListId = task.ListId,
            ProjectId = task.ProjectId,
            Assignees = task.Assignees.Select(a => new TaskAssigneeDto {
                Id = a.Id,
                TaskId = a.TaskId,
                UserId = a.UserId,
                AssignedDate = a.AssignedDate
            }).ToList(),
            Comments = task.Comments.Select(c => new CommentReadDto {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                Content = c.Content,
                CreatedDate = c.CreatedDate
            }).ToList(),
            List = task.List != null
                ? new ListReadDto {
                    Id = task.List.Id,
                    Name = task.List.Name,
                    ProjectId = task.List.ProjectId,
                    CreatedDate = task.List.CreatedDate
                }
                : null,
            Project = task.Project != null
                ? new ProjectReadDto {
                    Id = task.Project.Id,
                    Name = task.Project.Name,
                    WorkspaceId = task.Project.WorkspaceId,
                    CreatedDate = task.Project.CreatedDate
                }
                : null
        };

        return Ok(taskDto);
    }

    [HttpPut("{taskId}")]
    [ProducesResponseType(typeof(TaskReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult>
        UpdateTask(Guid taskId, [FromBody] TaskCreateDto taskUpdateDto) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingTask = await _context.Tasks
            .Include(t => t.Assignees)
            .ThenInclude(a => a.User)
            .Include(t => t.Comments)
            .Include(t => t.List)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (existingTask == null)
            return NotFound($"Task with ID {taskId} not found");

        // Update the existing task properties instead of creating a new entity
        existingTask.Name = taskUpdateDto.Name;
        existingTask.Description = taskUpdateDto.Description ?? existingTask.Description;
        existingTask.Status = taskUpdateDto.Status;
        existingTask.Priority = taskUpdateDto.Priority;
        existingTask.Deadline = taskUpdateDto.Deadline;
        existingTask.ListId = taskUpdateDto.ListId;
        existingTask.ProjectId = taskUpdateDto.ProjectId;

        _context.Tasks.Update(existingTask);
        await _context.SaveChangesAsync();

        var taskDto = new TaskReadDto {
            Id = existingTask.Id,
            Name = existingTask.Name,
            Description = existingTask.Description,
            Status = existingTask.Status,
            Priority = existingTask.Priority,
            Deadline = existingTask.Deadline,
            CreatedDate = existingTask.CreatedDate,
            ListId = existingTask.ListId,
            ProjectId = existingTask.ProjectId,
            Assignees = existingTask.Assignees.Select(a => new TaskAssigneeDto {
                Id = a.Id,
                TaskId = a.TaskId,
                UserId = a.UserId,
                AssignedDate = a.AssignedDate
            }).ToList(),
            Comments = existingTask.Comments.Select(c => new CommentReadDto {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                Content = c.Content,
                CreatedDate = c.CreatedDate
            }).ToList(),
            List = existingTask.List != null
                ? new ListReadDto {
                    Id = existingTask.List.Id,
                    Name = existingTask.List.Name,
                    ProjectId = existingTask.List.ProjectId,
                    CreatedDate = existingTask.List.CreatedDate
                }
                : null,
            Project = existingTask.Project != null
                ? new ProjectReadDto {
                    Id = existingTask.Project.Id,
                    Name = existingTask.Project.Name,
                    WorkspaceId = existingTask.Project.WorkspaceId,
                    CreatedDate = existingTask.Project.CreatedDate
                }
                : null
        };

        return Ok(taskDto);
    }

    [HttpDelete("{taskId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid taskId) {
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
    public async Task<IActionResult> MoveTask(Guid taskId, [FromBody] MoveTaskRequest request) {
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
        if (task.ProjectId != targetList.ProjectId)
            return BadRequest("Cannot move task to a list in a different project");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try {
            task.ListId = request.TargetListId;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var taskDto = new TaskReadDto {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                Deadline = task.Deadline,
                CreatedDate = task.CreatedDate,
                ListId = task.ListId,
                ProjectId = task.ProjectId,
                List = new ListReadDto {
                    Id = targetList.Id,
                    Name = targetList.Name,
                    ProjectId = targetList.ProjectId,
                    CreatedDate = targetList.CreatedDate
                },
                Project = task.Project != null
                    ? new ProjectReadDto {
                        Id = task.Project.Id,
                        Name = task.Project.Name,
                        WorkspaceId = task.Project.WorkspaceId,
                        CreatedDate = task.Project.CreatedDate
                    }
                    : null,
                Assignees = task.Assignees.Select(a => new TaskAssigneeDto {
                    Id = a.Id,
                    TaskId = a.TaskId,
                    UserId = a.UserId,
                    AssignedDate = a.AssignedDate
                }).ToList(),
                Comments = task.Comments.Select(c => new CommentReadDto {
                    Id = c.Id,
                    TaskId = c.TaskId,
                    UserId = c.UserId,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate
                }).ToList()
            };

            return Ok(taskDto);
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

        if (request.UserId == Guid.Empty)
            return BadRequest("User ID cannot be empty.");

        // Get requesting user ID from claims
        Guid? requestingUserId = ClaimsHelper.GetUserId(User);
        if (requestingUserId == null || requestingUserId == Guid.Empty)
            return BadRequest("User ID cannot be null.");

        // Verify task exists and load related data
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        // Verify user exists
        var userToAssign = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (userToAssign == null)
            return NotFound($"User with ID {request.UserId} not found");

        // Get workspace members for permission check
        var workspaceMembers = await _context.WorkspaceMembers
            .Where(m => m.WorkspaceId == task.Project.WorkspaceId)
            .ToListAsync();

        var requestingUserMember = workspaceMembers
            .FirstOrDefault(m => m.UserId == requestingUserId.Value);
        var assignedUserMember = workspaceMembers
            .FirstOrDefault(m => m.UserId == request.UserId);

        if (requestingUserMember == null || assignedUserMember == null)
            return Forbid("Both users must be members of the workspace");

        // Check if user is already assigned
        if (task.Assignees.Any(a => a.UserId == request.UserId))
            return BadRequest("User is already assigned to this task");

        // Create the assignment
        var taskAssignee = new TaskAssignee {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = request.UserId,
            AssignedDate = DateTime.UtcNow
        };

        _context.TaskAssignees.Add(taskAssignee);
        await _context.SaveChangesAsync();

        // Reload the entity with related data for the response
        var createdAssignee = await _context.TaskAssignees
            .Include(ta => ta.Task)
            .Include(ta => ta.User)
            .FirstAsync(ta => ta.Id == taskAssignee.Id);

        var response = TaskAssigneeReadDto.FromEntity(createdAssignee);

        return CreatedAtAction(
            nameof(GetTaskById),
            new { taskId },
            response
        );
    }


    [HttpDelete("{taskId}/assignees/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveTaskAssignee(Guid taskId, Guid userId) {
        // Get requesting user ID from claims
        Guid? requestingUserId = ClaimsHelper.GetUserId(User);
        if (requestingUserId == null)
            return BadRequest("User ID cannot be null.");

        // Verify task exists and load related data
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        // Find the assignee to remove
        var assignee = task.Assignees
            .FirstOrDefault(a => a.UserId == userId);

        if (assignee == null)
            return NotFound($"User {userId} is not assigned to task {taskId}");

        // Verify requesting user is a workspace member
        var workspaceMember = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(m =>
                m.WorkspaceId == task.Project.WorkspaceId &&
                m.UserId == requestingUserId.Value);

        if (workspaceMember == null)
            return Forbid("User must be a member of the workspace");

        // Remove the assignment
        _context.TaskAssignees.Remove(assignee);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{taskId}/assignees")]
    [ProducesResponseType(typeof(IEnumerable<TaskAssigneeReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskAssignees(Guid taskId) {
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
    public IActionResult GetTaskComments(Guid taskId) {
        var task = _context.Tasks
            .Include(t => t.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefault(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        var comments = task.Comments.Select(c => new CommentReadDto {
            Id = c.Id,
            TaskId = c.TaskId,
            UserId = c.UserId,
            Content = c.Content,
            CreatedDate = c.CreatedDate,
            User = new UserReadDto {
                Id = c.User.Id,
                Name = c.User.Name,
                Email = c.User.Email,
                CreatedDate = c.User.CreatedDate
            }
        }).ToList();

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

        // Get user ID from claims
        Guid? userId = ClaimsHelper.GetUserId(User);
        if (userId == null)
            return BadRequest("User ID cannot be null.");

        // Verify task exists and load related data
        var task = await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound($"Task with ID {taskId} not found");

        // Verify user is workspace member
        var workspaceMember = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(m =>
                m.WorkspaceId == task.Project.WorkspaceId &&
                m.UserId == userId.Value);

        if (workspaceMember == null)
            return Forbid("User must be a member of the workspace");

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

        var commentDto = new CommentReadDto {
            Id = createdComment.Id,
            TaskId = createdComment.TaskId,
            UserId = createdComment.UserId,
            Content = createdComment.Content,
            CreatedDate = createdComment.CreatedDate,
            User = UserReadDto.FromEntity(createdComment.User)
        };

        return CreatedAtAction(
            nameof(GetTaskComments),
            new { taskId },
            commentDto
        );
    }
}
