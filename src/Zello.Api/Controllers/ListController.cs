using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Helpers;

namespace Zello.Api.Controllers;

/// <summary>
/// Controller for managing lists and their associated tasks
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class ListController : ControllerBase {
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the ListController
    /// </summary>
    /// <param name="context">The application database context</param>
    public ListController(ApplicationDbContext context) {
        _context = context;
    }

    /// <summary>
    /// Retrieves a specific list by its ID
    /// </summary>
    /// <param name="listId">The unique identifier of the list to retrieve</param>
    /// <returns>The requested list if found and user has access</returns>
    /// <response code="200">Returns the requested list</response>
    /// <response code="403">User does not have permission to access this list</response>
    /// <response code="404">List with specified ID was not found</response>
    [HttpGet("{listId}")]
    [ProducesResponseType(typeof(ListReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetListById(Guid listId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var list = await _context.Lists
            .Include(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(l => l.Id == listId);

        if (list == null)
            return NotFound($"List with ID {listId} not found");

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         list.Project.Members.Any(pm =>
                             pm.WorkspaceMember.UserId == userId
                         );

        if (!hasAccess)
            return Forbid("User is not a member of this project");

        return Ok(ListReadDto.FromEntity(list));
    }

    /// <summary>
    /// Retrieves all lists accessible to the current user
    /// </summary>
    /// <param name="projectId">Optional project ID to filter lists by project</param>
    /// <returns>Collection of lists the user has access to</returns>
    /// <response code="200">Returns all accessible lists</response>
    /// <response code="403">User does not have permission to access lists</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ListReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllLists([FromQuery] Guid? projectId = null) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        // Start with a query that includes project membership data
        var query = _context.Lists
            .Include(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .Include(l => l.Tasks)
            .ThenInclude(t => t.Assignees)
            .Include(l => l.Tasks)
            .ThenInclude(t => t.Comments)
            .AsQueryable();

        // Filter by project if specified
        if (projectId.HasValue)
            query = query.Where(l => l.ProjectId == projectId.Value);

        // Filter to only show lists from projects where user is a member or is admin
        query = query.Where(l =>
            userAccess == AccessLevel.Admin ||
            l.Project.Members.Any(pm =>
                pm.WorkspaceMember.UserId == userId
            )
        );

        var lists = await query
            .OrderBy(l => l.Position)
            .ToListAsync();

        return Ok(lists.Select(ListReadDto.FromEntity).ToList());
    }

    /// <summary>
    /// Updates an existing list
    /// </summary>
    /// <param name="listId">The unique identifier of the list to update</param>
    /// <param name="updateList">The updated list data</param>
    /// <returns>The updated list</returns>
    /// <response code="200">List was successfully updated</response>
    /// <response code="400">The request data is invalid</response>
    /// <response code="403">User does not have permission to update this list</response>
    /// <response code="404">List with specified ID was not found</response>
    [HttpPut("{listId}")]
    [ProducesResponseType(typeof(ListReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateList(Guid listId, [FromBody] ListUpdateDto updateList) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var existingList = await _context.Lists
            .Include(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(l => l.Id == listId);

        if (existingList == null)
            return NotFound($"List with ID {listId} not found");

        var projectMember = existingList.Project.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

        // First check if user has access to the project
        bool hasAccess = userAccess == AccessLevel.Admin ||
                         (projectMember != null && projectMember.AccessLevel >= AccessLevel.Member);

        if (!hasAccess)
            return Forbid("Insufficient permissions to update lists");

        // Ensure the list ID from the URL is used
        updateList.Id = listId;
        updateList.ToEntity(existingList);
        await _context.SaveChangesAsync();

        return Ok(ListReadDto.FromEntity(existingList));
    }

    /// <summary>
    /// Updates the position of a list within its project
    /// </summary>
    /// <param name="listId">The unique identifier of the list to reposition</param>
    /// <param name="updateList">The update data containing the new position</param>
    /// <returns>The updated list</returns>
    /// <response code="200">List position was successfully updated</response>
    /// <response code="400">The requested position is invalid</response>
    /// <response code="403">User does not have permission to update list positions</response>
    /// <response code="404">List with specified ID was not found</response>
    /// <response code="500">An error occurred while updating the list position</response>
    [HttpPut("{listId}/position")]
    [ProducesResponseType(typeof(ListReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateListPosition(Guid listId,
        [FromBody] ListUpdateDto updateList) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var list = await _context.Lists
            .Include(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(l => l.Id == listId);

        if (list == null)
            return NotFound($"List with ID {listId} not found");

        var projectMember = list.Project.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

        // Check if user has sufficient access
        bool hasAccess = userAccess == AccessLevel.Admin ||
                         (projectMember != null && projectMember.AccessLevel >= AccessLevel.Member);

        if (!hasAccess)
            return Forbid("Insufficient permissions to update list positions");

        var projectLists = await _context.Lists
            .Where(l => l.ProjectId == list.ProjectId)
            .OrderBy(l => l.Position)
            .ToListAsync();

        if (updateList.Position < 0 || updateList.Position >= projectLists.Count)
            return BadRequest("Invalid position");

        try {
            var oldPosition = list.Position;
            if (updateList.Position < oldPosition) {
                var listsToUpdate = await _context.Lists
                    .Where(l => l.ProjectId == list.ProjectId &&
                                l.Position >= updateList.Position &&
                                l.Position < oldPosition)
                    .ToListAsync();

                foreach (var l in listsToUpdate) {
                    l.Position++;
                }
            } else if (updateList.Position > oldPosition) {
                var listsToUpdate = await _context.Lists
                    .Where(l => l.ProjectId == list.ProjectId &&
                                l.Position > oldPosition &&
                                l.Position <= updateList.Position)
                    .ToListAsync();

                foreach (var l in listsToUpdate) {
                    l.Position--;
                }
            }

            list.Position = updateList.Position;
            await _context.SaveChangesAsync();

            return Ok(ListReadDto.FromEntity(list));
        } catch (Exception) {
            return StatusCode(500, "An error occurred while updating the list position");
        }
    }

    /// <summary>
    /// Creates a new task in the specified list
    /// </summary>
    /// <param name="listId">The unique identifier of the list to add the task to</param>
    /// <param name="createTask">The task data to create</param>
    /// <returns>The created task</returns>
    /// <response code="201">Task was successfully created</response>
    /// <response code="400">The task data is invalid</response>
    /// <response code="403">User does not have permission to create tasks in this list</response>
    /// <response code="404">List with specified ID was not found</response>
    /// <response code="500">An error occurred while creating the task</response>
    [HttpPost("{listId}/tasks")]
    [ProducesResponseType(typeof(TaskReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTask(Guid listId, [FromBody] TaskCreateDto createTask) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        try {
            var list = await _context.Lists
                .Include(l => l.Project)
                .ThenInclude(p => p.Members)
                .ThenInclude(pm => pm.WorkspaceMember)
                .FirstOrDefaultAsync(l => l.Id == listId);

            if (list == null)
                return NotFound($"List with ID {listId} not found");

            // Check project membership instead of workspace membership
            bool hasAccess = userAccess == AccessLevel.Admin ||
                             list.Project.Members.Any(pm =>
                                 pm.WorkspaceMember.UserId == userId
                             );

            if (!hasAccess)
                return Forbid("User is not a member of this project");

            // Set both ListId and ProjectId
            createTask.ListId = listId;
            createTask.ProjectId = list.ProjectId;

            var task = createTask.ToEntity();
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            var taskDto = TaskReadDto.FromEntity(task);

            return CreatedAtAction(
                nameof(GetListById),
                new { listId },
                taskDto
            );
        } catch (Exception) {
            return StatusCode(500, "An error occurred while creating the task");
        }
    }

    /// <summary>
    /// Retrieves all tasks in a specific list
    /// </summary>
    /// <param name="listId">The unique identifier of the list</param>
    /// <returns>Collection of tasks in the specified list</returns>
    /// <response code="200">Returns all tasks in the list</response>
    /// <response code="403">User does not have permission to access this list</response>
    /// <response code="404">List with specified ID was not found</response>
    /// <response code="500">An error occurred while retrieving tasks</response>
    [HttpGet("{listId}/tasks")]
    [ProducesResponseType(typeof(IEnumerable<TaskReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetListTasks(Guid listId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        try {
            var list = await _context.Lists
                .Include(l => l.Project)
                .ThenInclude(p => p.Members)
                .ThenInclude(pm => pm.WorkspaceMember)
                .FirstOrDefaultAsync(l => l.Id == listId);

            if (list == null)
                return NotFound($"List with ID {listId} not found");

            bool hasAccess = userAccess == AccessLevel.Admin ||
                             list.Project.Members.Any(pm =>
                                 pm.WorkspaceMember.UserId == userId
                             );

            if (!hasAccess)
                return Forbid("User is not a member of this project");

            var tasks = await _context.Tasks
                .Where(t => t.ListId == listId)
                .Include(t => t.Assignees)
                .Include(t => t.Comments)
                .OrderBy(t => t.CreatedDate)
                .ToListAsync();

            return Ok(tasks.Select(TaskReadDto.FromEntity).ToList());
        } catch (Exception) {
            return StatusCode(500, "An error occurred while retrieving tasks");
        }
    }
}
