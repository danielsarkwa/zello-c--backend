using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Application.Features.Comments.Models;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Dto;
using Zello.Domain.Entities.Requests;
using Zello.Infrastructure.Data;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class CommentsController : ControllerBase {
    private readonly ApplicationDbContext _context;

    public CommentsController(ApplicationDbContext context) {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Comment>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments([FromQuery] Guid? taskId = null) {
        var query = _context.Comments.AsQueryable();

        if (taskId.HasValue)
            query = query.Where(c => c.TaskId == taskId);

        var comments = await query
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();

        return Ok(comments);
    }

    [HttpGet("{commentId}")]
    [ProducesResponseType(typeof(Comment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentById(Guid commentId) {
        var comment = await _context.Comments.FindAsync(commentId);

        if (comment == null)
            return NotFound($"Comment with ID {commentId} not found");

        return Ok(comment);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Comment), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request == null)
            return BadRequest("Request cannot be null");

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Comment content cannot be empty");

        // Check if task exists
        var taskExists = await _context.Tasks.AnyAsync(t => t.Id == request.TaskId);
        if (!taskExists)
            return NotFound($"Task with ID {request.TaskId} not found");

        // In a real app, you'd get the user ID from the authenticated user
        // For now, I'll leave this TODO for you to implement based on your auth system
        var userId = Guid.Empty; // TODO: Get from authenticated user

        var comment = new Comment {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            UserId = userId,
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

    [HttpPut("{commentId}")]
    [ProducesResponseType(typeof(Comment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(Guid commentId,
        [FromBody] UpdateCommentRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            return NotFound($"Comment with ID {commentId} not found");

        comment.Content = request.Content;
        await _context.SaveChangesAsync();

        return Ok(comment);
    }

    [HttpDelete("{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid commentId) {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            return NotFound($"Comment with ID {commentId} not found");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
