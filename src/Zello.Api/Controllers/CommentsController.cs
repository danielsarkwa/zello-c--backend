using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Zello.Application.Features.Comments.Models;
using Zello.Domain.Entities.Dto;
using Zello.Domain.Entities.Requests;
using Zello.Infrastructure.TestingDataStorage;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class CommentsController : ControllerBase {
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
    public IActionResult GetComments([FromQuery] Guid? taskId = null) {
        var comments = TestData.TestCommentCollection.Values
            .Where(c => !taskId.HasValue || c.TaskId == taskId)
            .OrderByDescending(c => c.CreatedDate)
            .ToList();

        return Ok(comments);
    }

    [HttpGet("{commentId}")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetCommentById(Guid commentId) {
        if (!TestData.TestCommentCollection.TryGetValue(commentId, out var comment))
            return NotFound($"Comment with ID {commentId} not found");

        return Ok(comment);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CreateComment([FromBody] CreateCommentRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request == null) {
            return BadRequest("Request cannot be null");
        }

        if (string.IsNullOrWhiteSpace(request.Content)) {
            return BadRequest("Comment content cannot be empty");
        }

        if (!TestData.TestTaskCollection.ContainsKey(request.TaskId))
            return NotFound($"Task with ID {request.TaskId} not found");

        // In a real app, we'd get the user ID from the authenticated user
        var userId = TestData.TestUserCollection.First().Key;

        var comment = new CommentDto {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            UserId = userId,
            Content = request.Content,
            CreatedDate = DateTime.UtcNow
        };

        TestData.TestCommentCollection.Add(comment.Id, comment);

        return CreatedAtAction(
            nameof(GetCommentById),
            new { commentId = comment.Id },
            comment
        );
    }

    [HttpPut("{commentId}")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateComment(Guid commentId, [FromBody] UpdateCommentRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!TestData.TestCommentCollection.TryGetValue(commentId, out var comment))
            return NotFound($"Comment with ID {commentId} not found");

        comment.Content = request.Content;
        comment.UpdatedDate = DateTime.UtcNow;

        TestData.TestCommentCollection[commentId] = comment;

        return Ok(comment);
    }

    [HttpDelete("{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteComment(Guid commentId) {
        if (!TestData.TestCommentCollection.ContainsKey(commentId))
            return NotFound($"Comment with ID {commentId} not found");

        TestData.TestCommentCollection.Remove(commentId);

        return NoContent();
    }
}
