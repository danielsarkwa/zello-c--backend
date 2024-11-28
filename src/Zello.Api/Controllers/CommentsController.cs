using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Application.Features.Comments.Models;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Entities.Requests;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Helpers;

namespace Zello.Api.Controllers;

/// <summary>
/// Controller for managing task comments within projects.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class CommentsController : ControllerBase {
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the CommentsController.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public CommentsController(ApplicationDbContext context) {
        _context = context;
    }

    /// <summary>
    /// Retrieves comments with optional filtering by task ID.
    /// </summary>
    /// <param name="taskId">Optional. The ID of the task to filter comments by.</param>
    /// <returns>A list of comments the user has access to view.</returns>
    /// <response code="200">Returns the list of comments.</response>
    /// <response code="400">If the user ID is missing from the claims.</response>
    /// <response code="403">If the user doesn't have access to view the comments.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Comment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetComments([FromQuery] Guid? taskId = null) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        // Start with a query that includes all necessary relationships
        var query = _context.Comments
            .Include(c => c.Task)
            .ThenInclude(t => t.List)
            .ThenInclude(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .AsQueryable();

        // Apply task filter if provided
        if (taskId.HasValue)
            query = query.Where(c => c.TaskId == taskId);

        // Filter to only show comments from projects where user is a member or is admin
        query = query.Where(c =>
            userAccess == AccessLevel.Admin ||
            c.Task.List.Project.Members.Any(pm =>
                pm.WorkspaceMember.UserId == userId
            )
        );

        var comments = await query
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();

        return Ok(comments);
    }

    /// <summary>
    /// Retrieves a specific comment by its ID.
    /// </summary>
    /// <param name="commentId">The ID of the comment to retrieve.</param>
    /// <returns>The requested comment if found and accessible.</returns>
    /// <response code="200">Returns the requested comment.</response>
    /// <response code="400">If the user ID is missing from the claims.</response>
    /// <response code="403">If the user doesn't have access to view the comment.</response>
    /// <response code="404">If the comment with the specified ID was not found.</response>
    [HttpGet("{commentId}")]
    [ProducesResponseType(typeof(Comment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCommentById(Guid commentId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var comment = await _context.Comments
            .Include(c => c.Task)
            .ThenInclude(t => t.List)
            .ThenInclude(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            return NotFound($"Comment with ID {commentId} not found");

        // Check if user has access to the project containing this comment
        bool hasAccess = userAccess == AccessLevel.Admin ||
                         comment.Task.List.Project.Members.Any(pm =>
                             pm.WorkspaceMember.UserId == userId
                         );

        if (!hasAccess)
            return Forbid("User is not a member of this project");

        return Ok(comment);
    }

    /// <summary>
    /// Creates a new comment on a task.
    /// </summary>
    /// <param name="request">The comment creation request containing the task ID and comment content.</param>
    /// <returns>The newly created comment.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/comments
    ///     {
    ///         "task_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "content": "This is a comment"
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created comment.</response>
    /// <response code="400">If the request is invalid or user ID is missing.</response>
    /// <response code="403">If the user doesn't have access to the task's project.</response>
    /// <response code="404">If the specified task was not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Comment), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request == null)
            return BadRequest("Request cannot be null");

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Comment content cannot be empty");

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        // Check if task exists and user has access
        var task = await _context.Tasks
            .Include(t => t.List)
            .ThenInclude(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId);

        if (task == null)
            return NotFound($"Task with ID {request.TaskId} not found");

        // Check if user has access to the project
        bool hasAccess = userAccess == AccessLevel.Admin ||
                         task.List.Project.Members.Any(pm =>
                             pm.WorkspaceMember.UserId == userId
                         );

        if (!hasAccess)
            return Forbid("User is not a member of this project");

        var comment = new Comment {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            UserId = userId.Value,
            Content = request.Content,
            CreatedDate = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCommentById),
            new { commentId = comment.Id },
            comment
        );
    }

    /// <summary>
    /// Updates an existing comment.
    /// </summary>
    /// <param name="commentId">The ID of the comment to update.</param>
    /// <param name="request">The update request containing the new comment content.</param>
    /// <returns>The updated comment.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/v1/comments/{commentId}
    ///     {
    ///         "content": "Updated comment text"
    ///     }
    ///
    /// Only the comment owner or an admin can update the comment.
    /// </remarks>
    /// <response code="200">Returns the updated comment.</response>
    /// <response code="400">If the request is invalid or user ID is missing.</response>
    /// <response code="403">If the user doesn't have access to update the comment.</response>
    /// <response code="404">If the comment with the specified ID was not found.</response>
    [HttpPut("{commentId}")]
    [ProducesResponseType(typeof(Comment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateComment(Guid commentId,
        [FromBody] UpdateCommentRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var comment = await _context.Comments
            .Include(c => c.Task)
            .ThenInclude(t => t.List)
            .ThenInclude(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            return NotFound($"Comment with ID {commentId} not found");

        // Check if user has access to the project
        bool hasAccess = userAccess == AccessLevel.Admin ||
                         comment.Task.List.Project.Members.Any(pm =>
                             pm.WorkspaceMember.UserId == userId
                         );

        if (!hasAccess)
            return Forbid("User is not a member of this project");

        // Additional check: only allow comment owner or admin to update
        if (comment.UserId != userId && userAccess != AccessLevel.Admin)
            return Forbid("Only the comment owner or an admin can update this comment");

        comment.Content = request.Content;
        await _context.SaveChangesAsync();

        return Ok(comment);
    }

    /// <summary>
    /// Deletes a specific comment.
    /// </summary>
    /// <param name="commentId">The ID of the comment to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <remarks>
    /// Only the comment owner or an admin can delete the comment.
    /// </remarks>
    /// <response code="204">If the comment was successfully deleted.</response>
    /// <response code="400">If the user ID is missing from the claims.</response>
    /// <response code="403">If the user doesn't have access to delete the comment.</response>
    /// <response code="404">If the comment with the specified ID was not found.</response>
    [HttpDelete("{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteComment(Guid commentId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var comment = await _context.Comments
            .Include(c => c.Task)
            .ThenInclude(t => t.List)
            .ThenInclude(l => l.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            return NotFound($"Comment with ID {commentId} not found");

        // Check if user has access to the project
        bool hasAccess = userAccess == AccessLevel.Admin ||
                         comment.Task.List.Project.Members.Any(pm =>
                             pm.WorkspaceMember.UserId == userId
                         );

        if (!hasAccess)
            return Forbid("User is not a member of this project");

        // Additional check: only allow comment owner or admin to delete
        if (comment.UserId != userId && userAccess != AccessLevel.Admin)
            return Forbid("Only the comment owner or an admin can delete this comment");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
