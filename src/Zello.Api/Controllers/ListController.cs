using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Application.Dtos;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Helpers;

namespace Zello.Api.Controllers;

/// <summary>
/// Controller for managing lists and their associated tasks
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class ListController : ControllerBase {
    private readonly ITaskListService _taskListService;
    private readonly IUserService _userService;
    private readonly IProjectService _projectService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the ListController
    /// </summary>
    /// <param name="taskListService">The task service</param>
    /// <param name="userService">The user service</param>
    /// <param name="projectService">The project service</param>
    /// <param name="authorizationService">The authorization service</param>
    public ListController(ITaskListService taskListService, IUserService userService, IProjectService projectService, IAuthorizationService authorizationService) {
        _taskListService = taskListService;
        _userService = userService;
        _projectService = projectService;
        _authorizationService = authorizationService;
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

        var list = await _taskListService.GetByIdAsync(listId);

        if (list == null)
            return NotFound($"List with ID {listId} not found");

        // Verify user's access to the project
        bool hasAccess = await _authorizationService
            .AuthorizeProjectAccessAsync(userId.Value, list.ProjectId, AccessLevel.Member);

        if (!hasAccess) {
            return Forbid("User is not a member of this project");
        }

        return Ok(list);
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

        var lists = await _taskListService.GetAllAsync(projectId);

        // Check if user has access to the project
        if (projectId.HasValue) {
            var project = await _projectService.GetProjectByIdAsync(projectId.Value);

            if (project == null) {
                return NotFound($"Project with ID {projectId} not found");
            }

            // Verify user's access to the project
            bool hasAccess = await _authorizationService
                .AuthorizeProjectAccessAsync(userId.Value, projectId.Value, AccessLevel.Member);

            if (!hasAccess) {
                return Forbid("User is not a member of this project");
            }

            // Filter by project if specified
            lists = lists.Where(l => l.ProjectId == projectId.Value);

            // order by position
            lists = lists.OrderBy(l => l.Position);

            // Filter to only show lists from projects where user is a member or is admin
            lists = lists.Where(l => userAccess == AccessLevel.Admin ||
                                     project.Members.Any(pm => pm.WorkspaceMember != null && pm.WorkspaceMember.UserId == userId.Value));
        }

        return Ok(lists);
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

        var list = await _taskListService.UpdateAsync(listId, updateList);
        if (list == null)
            return NotFound($"List with ID {listId} not found");

        return Ok(list);
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
    public async Task<IActionResult> UpdateListPosition(
    Guid listId,
    [FromBody] ListUpdateDto updateList) {
        var userId = ClaimsHelper.GetUserId(User);
        if (userId == null)
            return BadRequest("User ID missing");

        // First, check if user has access to the project
        var list = await _taskListService.GetByIdAsync(listId);

        if (list == null)
            return NotFound($"List with ID {listId} not found");

        // Verify user's access to the project
        bool hasAccess = await _authorizationService
            .AuthorizeProjectAccessAsync(userId.Value, list.ProjectId, AccessLevel.Member);

        if (!hasAccess)
            throw new UnauthorizedAccessException("Insufficient permissions to update list position");

        try {
            var updatedList = await _taskListService.UpdatePositionAsync(
                listId,
                updateList.Position
            );

            return updatedList == null
                ? NotFound($"List with ID {listId} not found")
                : Ok(updatedList);
        } catch (ArgumentException ex) {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new task in the specified list
    /// </summary>
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
    public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto createTask) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        try {
            var task = await _taskListService.CreateTaskAsync(createTask, userId.Value);
            if (task == null)
                return NotFound($"List with ID {createTask.ListId} not found");

            var list = await _taskListService.GetListTasksAsync(createTask.ListId);
            if (list == null)
                return NotFound($"List with ID {createTask.ListId} not found");

            var projectId = list.FirstOrDefault()?.ProjectId;

            // Check project membership instead of workspace membership
            bool hasAccess = await _authorizationService.AuthorizeProjectMembershipAsync(userId.Value, projectId!.Value);

            if (!hasAccess)
                return Forbid("User is not a member of this project");

            return CreatedAtAction(nameof(GetListById), new { createTask.ListId }, task);
        } catch (UnauthorizedAccessException) {
            return Forbid();
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
            var tasks = await _taskListService.GetListTasksAsync(listId);

            if (tasks == null)
                return NotFound($"List with ID {listId} not found");

            var projectId = tasks.FirstOrDefault()?.ProjectId;

            bool hasAccess = await _authorizationService.AuthorizeProjectMembershipAsync(userId.Value, projectId!.Value);

            if (!hasAccess)
                return Forbid("User is not a member of this project");

            return Ok(tasks);
        } catch (Exception) {
            return StatusCode(500, "An error occurred while retrieving tasks");
        }
    }
}
