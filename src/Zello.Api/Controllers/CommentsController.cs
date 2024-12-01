using Microsoft.AspNetCore.Mvc;
using Zello.Application.Dtos;
using Zello.Application.ServiceInterfaces;
using Zello.Infrastructure.Helpers;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Api.Controllers;

/// <summary>
/// Controller for managing task comments within projects.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class CommentsController : ControllerBase {
    private readonly ICommentService _commentService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the CommentsController.
    /// </summary>
    /// <param name="commentService">The controller service.</param>
    /// <param name="authorizationService">The authorization service.</param>
    public CommentsController(ICommentService commentService, IAuthorizationService authorizationService) {
        _commentService = commentService;
        _authorizationService = authorizationService;
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
    [ProducesResponseType(typeof(IEnumerable<CommentReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetComments([FromQuery] Guid? taskId = null) {
        if (taskId.HasValue) {
            var comments = await _commentService.GetCommentsByTaskIdAsync(taskId.Value);
            return Ok(comments);
        } else {
            return BadRequest("Task ID is required.");
        }
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
    [ProducesResponseType(typeof(CommentReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCommentById(Guid commentId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        try {
            var commentDto = await _commentService.GetCommentByIdAsync(commentId);

            var comment = new Comment {
                Id = commentDto.Id,
                TaskId = commentDto.TaskId,
                Content = commentDto.Content,
                CreatedDate = commentDto.CreatedDate,
                UserId = commentDto.UserId
            };

            // bool hasAccess = await _userService.HasCommentAccessAsync(userId.Value, comment, userAccess);
            bool hasAccess = await _authorizationService.AuthorizeCommentAccessAsync(userId.Value, commentId, userAccess);

            if (!hasAccess) {
                return Forbid("User is not a member of this project");
            }

            return Ok(commentDto);
        } catch (Exception ex) {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new comment on a task.
    /// </summary>
    /// <param name="commentCreateDto">The comment creation request containing the task ID and comment content.</param>
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
    [ProducesResponseType(typeof(CommentReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateComment([FromBody] CommentCreateDto commentCreateDto) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (commentCreateDto == null)
            return BadRequest("Request cannot be null");

        if (string.IsNullOrWhiteSpace(commentCreateDto.Content))
            return BadRequest("Comment content cannot be empty");

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        try {
            var commentDto = await _commentService.CreateCommentAsync(commentCreateDto, userId.Value);

            var comment = new Comment {
                Id = commentDto.Id,
                TaskId = commentDto.TaskId,
                Content = commentDto.Content,
                CreatedDate = commentDto.CreatedDate,
                UserId = commentDto.UserId
            };

            bool hasAccess = await _authorizationService.AuthorizeProjectAccessAsync(userId.Value, comment.Task.List.Project.Id, AccessLevel.Member);

            if (!hasAccess)
                return Forbid("User is not a member of this project");

            return CreatedAtAction(nameof(GetCommentById), new { commentId = commentDto.Id }, commentDto);
        } catch (Exception ex) {
            if (ex.Message.Contains("not found"))
                return NotFound(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing comment.
    /// </summary>
    /// <param name="commentId">The ID of the comment to update.</param>
    /// <param name="commentUpdateDto">The update request containing the new comment content.</param>
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
    [ProducesResponseType(typeof(CommentReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateComment(Guid commentId,
        [FromBody] CommentUpdateDto commentUpdateDto) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        try {
            var commentDto = await _commentService.UpdateCommentAsync(commentId, commentUpdateDto);

            var comment = new Comment {
                Id = commentDto.Id,
                TaskId = commentDto.TaskId,
                Content = commentDto.Content,
                CreatedDate = commentDto.CreatedDate,
                UserId = commentDto.UserId
            };

            bool hasAccess = await _authorizationService.AuthorizeProjectAccessAsync(userId.Value, comment.Task.List.Project.Id, AccessLevel.Member);

            if (!hasAccess)
                return Forbid("User is not a member of this project");

            // Additional check: only allow comment owner or admin to update
            if (comment.UserId != userId && userAccess != AccessLevel.Admin)
                return Forbid("Only the comment owner or an admin can update this comment");

            return Ok(commentDto);
        } catch (Exception ex) {
            if (ex.Message.Contains("not found"))
                return NotFound(ex.Message);
            return BadRequest(ex.Message);
        }
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

        try {
            // get the comment
            var commentDto = await _commentService.GetCommentByIdAsync(commentId);

            var comment = new Comment {
                Id = commentDto.Id,
                TaskId = commentDto.TaskId,
                Content = commentDto.Content,
                CreatedDate = commentDto.CreatedDate,
                UserId = commentDto.UserId
            };

            bool hasAccess = await _authorizationService.AuthorizeProjectAccessAsync(userId.Value, comment.Task.List.Project.Id, AccessLevel.Member);

            if (!hasAccess)
                return Forbid("User is not a member of this project");

            // Additional check: only allow comment owner or admin to delete
            if (comment.UserId != userId && userAccess != AccessLevel.Admin)
                return Forbid("Only the comment owner or an admin can delete this comment");

            await _commentService.DeleteCommentAsync(commentId);

            return NoContent();
        } catch (Exception ex) {
            if (ex.Message.Contains("not found"))
                return NotFound(ex.Message);
            return BadRequest(ex.Message);
        }
    }
}
