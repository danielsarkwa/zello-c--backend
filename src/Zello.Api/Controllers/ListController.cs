using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Helpers;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class ListController : ControllerBase {
    private readonly ApplicationDbContext _context;

    public ListController(ApplicationDbContext context) {
        _context = context;
    }

    [HttpGet("{listId}")]
    [ProducesResponseType(typeof(ListReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetListById(Guid listId) {
        var list = _context.Lists.Find(listId);

        if (list == null) {
            return NotFound($"List with ID {listId} not found");
        }

        return Ok(list);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ListReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLists([FromQuery] Guid? projectId = null) {
        var lists = await _context.Lists
            .Where(l => !projectId.HasValue || l.ProjectId == projectId.Value)
            .Include(l => l.Tasks)
            .ThenInclude(t => t.Assignees)
            .Include(l => l.Tasks)
            .ThenInclude(t => t.Comments)
            .OrderBy(l => l.Position)
            .ToListAsync();

        return Ok(lists);
    }

    [HttpPut("{listId}")]
    [ProducesResponseType(typeof(ListReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateList(Guid listId, [FromBody] ListCreateDto updateList) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingList = await _context.Lists
            .FirstOrDefaultAsync(l => l.Id == listId);

        if (existingList == null)
            return NotFound($"List with ID {listId} not found");

        // Update only the modifiable properties
        var updatedList = updateList.ToEntity();

        _context.Lists.Update(updatedList);
        await _context.SaveChangesAsync();

        return Ok(existingList);
    }

    [HttpPut("{listId}/position")]
    [ProducesResponseType(typeof(ListReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateListPosition(Guid listId, [FromBody] int newPosition) {
        var list = await _context.Lists
            .FirstOrDefaultAsync(l => l.Id == listId);

        if (list == null)
            return NotFound($"List with ID {listId} not found");

        var projectLists = await _context.Lists
            .Where(l => l.ProjectId == list.ProjectId)
            .OrderBy(l => l.Position)
            .ToListAsync();

        // Validate position
        if (newPosition < 0 || newPosition >= projectLists.Count)
            return BadRequest("Invalid position");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try {
            var oldPosition = list.Position;
            if (newPosition < oldPosition) {
                // Moving left: increment positions of lists between new and old positions
                var listsToUpdate = await _context.Lists
                    .Where(l => l.ProjectId == list.ProjectId &&
                                l.Position >= newPosition &&
                                l.Position < oldPosition)
                    .ToListAsync();

                foreach (var l in listsToUpdate) {
                    l.Position++;
                }
            } else if (newPosition > oldPosition) {
                // Moving right: decrement positions of lists between old and new positions
                var listsToUpdate = await _context.Lists
                    .Where(l => l.ProjectId == list.ProjectId &&
                                l.Position > oldPosition &&
                                l.Position <= newPosition)
                    .ToListAsync();

                foreach (var l in listsToUpdate) {
                    l.Position--;
                }
            }

            list.Position = newPosition;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(list);
        } catch (Exception) {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // TODO: FIX THIS
    // [HttpDelete("{listId}")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public IActionResult DeleteList(Guid listId) {
    //   if (!TestData.TestListCollection.ContainsKey(listId))
    //     return NotFound($"List with ID {listId} not found");

    //   var list = TestData.TestListCollection[listId];

    //   // Remove the list
    //   TestData.TestListCollection.Remove(listId);

    //   // Reorder remaining lists in the project
    //   var projectLists = TestData.TestListCollection.Values
    //       .Where(l => l.ProjectId == list.ProjectId)
    //       .OrderBy(l => l.Position)
    //       .ToList();

    //   for (int i = 0; i < projectLists.Count; i++) {
    //     projectLists[i].Position = i;
    //     TestData.TestListCollection[projectLists[i].Id] = projectLists[i];
    //   }

    //   // Remove any tasks in this list
    //   var tasksToRemove = TestData.TestTaskCollection.Values
    //       .Where(t => t.ListId == listId)
    //       .Select(t => t.Id)
    //       .ToList();

    //   foreach (var taskId in tasksToRemove) {
    //     TestData.TestTaskCollection.Remove(taskId);
    //   }

    //   return NoContent();
    // }

    [HttpPost("{listId}/tasks")]
    [ProducesResponseType(typeof(TaskReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTask(Guid listId, [FromBody] TaskCreateDto createTask) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Get user ID from claims
        Guid? userId = ClaimsHelper.GetUserId(User);
        if (userId == null)
            return BadRequest("User ID cannot be null.");

        try {
            // Get the list including its project and workspace
            var list = await _context.Lists
                .Include(l => l.Project)
                .ThenInclude(p => p.Workspace)
                .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(l => l.Id == listId);

            if (list == null)
                return NotFound($"List with ID {listId} not found");

            // Check if user is a workspace member
            var isMember = list.Project.Workspace.Members
                .Any(m => m.UserId == userId.Value);

            if (!isMember)
                return Forbid("User is not a member of the workspace");

            // Create new task entity
            var task = new WorkTask {
                Id = Guid.NewGuid(),
                Name = createTask.Name,
                Status = createTask.Status,
                Priority = createTask.Priority,
                Deadline = createTask.Deadline,
                ListId = listId,
                ProjectId = list.ProjectId,
                CreatedDate = DateTime.UtcNow
            };

            // Add to database
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Map to DTO for response
            var taskDto = new TaskReadDto {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                Deadline = task.Deadline,
                ListId = task.ListId,
                ProjectId = task.ProjectId,
                CreatedDate = task.CreatedDate,
                Comments = new List<CommentReadDto>(),
                Assignees = new List<TaskAssigneeDto>()
            };

            return CreatedAtAction(
                nameof(GetListById),
                new { listId },
                taskDto
            );
        } catch (Exception) {
            // Log the exception
            return StatusCode(500, "An error occurred while creating the task");
        }
    }

    [HttpGet("{listId}/tasks")]
    [ProducesResponseType(typeof(IEnumerable<TaskReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetListTasks(Guid listId) {
        try {
            // Check if list exists
            var listExists = await _context.Lists
                .AnyAsync(l => l.Id == listId);

            if (!listExists)
                return NotFound($"List with ID {listId} not found");

            // Get tasks with related data
            var tasks = await _context.Tasks
                .Where(t => t.ListId == listId)
                .Include(t => t.Assignees)
                .Include(t => t.Comments)
                .OrderBy(t => t.CreatedDate)
                .Select(t => new TaskReadDto {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    Deadline = t.Deadline,
                    ListId = t.ListId,
                    ProjectId = t.ProjectId,
                    CreatedDate = t.CreatedDate,
                    Assignees = t.Assignees.Select(a => new TaskAssigneeDto {
                        Id = a.Id,
                        TaskId = a.TaskId,
                        UserId = a.UserId,
                        AssignedDate = a.AssignedDate
                    }).ToList(),
                    Comments = t.Comments.Select(c => new CommentReadDto {
                        Id = c.Id,
                        Content = c.Content,
                        CreatedDate = c.CreatedDate,
                        TaskId = c.TaskId,
                        UserId = c.UserId
                    }).ToList()
                })
                .ToListAsync();

            return Ok(tasks);
        } catch (Exception) {
            // Log the exception
            return StatusCode(500, "An error occurred while retrieving tasks");
        }
    }
}
