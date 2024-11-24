using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Zello.Application.Features.Tasks.Models;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.TestingDataStorage;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class TaskController : ControllerBase {
    [HttpGet("{taskId}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTaskById(Guid taskId) {
        if (!TestData.TestTaskCollection.TryGetValue(taskId, out var task))
            return NotFound($"Task with ID {taskId} not found");

        return Ok(task);
    }

    [HttpPut("{taskId}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateTask(Guid taskId, [FromBody] UpdateTaskRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!TestData.TestTaskCollection.ContainsKey(taskId))
            return NotFound($"Task with ID {taskId} not found");

        var existingTask = TestData.TestTaskCollection[taskId];

        // Update only the fields from request
        existingTask.Name = request.Name;
        existingTask.Description = request.Description;
        existingTask.Status = request.Status;
        existingTask.Priority = request.Priority;
        existingTask.Deadline = request.Deadline;
        existingTask.UpdatedDate = DateTime.UtcNow;

        return Ok(existingTask);
    }


    [HttpDelete("{taskId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteTask(Guid taskId) {
        if (!TestData.TestTaskCollection.ContainsKey(taskId))
            return NotFound($"Task with ID {taskId} not found");

        TestData.TestTaskCollection.Remove(taskId);

        return NoContent();
    }


    [HttpPost("{taskId}/move")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult MoveTask(Guid taskId, [FromBody] MoveTaskRequest request) {
        if (!TestData.TestTaskCollection.TryGetValue(taskId, out var task))
            return NotFound($"Task with ID {taskId} not found");

        if (!TestData.TestListCollection.TryGetValue(request.TargetListId, out var targetList))
            return NotFound($"Target list with ID {request.TargetListId} not found");

        // Verify lists are in the same project
        if (task.ProjectId != targetList.ProjectId)
            return BadRequest("Cannot move task to a list in a different project");

        task.ListId = request.TargetListId;
        TestData.TestTaskCollection[taskId] = task;

        return Ok(task);
    }

    // In TaskController
    [HttpGet("{taskId}/comments")]
    [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTaskComments(Guid taskId) {
        if (!TestData.TestTaskCollection.ContainsKey(taskId))
            return NotFound($"Task with ID {taskId} not found");

        return RedirectToAction(
            "GetComments",
            "Comments",
            new { taskId }
        );
    }

    [HttpPost("{taskId}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AddTaskComment(Guid taskId, [FromBody] AddCommentRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!TestData.TestTaskCollection.ContainsKey(taskId))
            return NotFound($"Task with ID {taskId} not found");

        // In a real app, we'd get the user ID from the authenticated user
        var userId = TestData.TestUserCollection.First().Key;

        var comment = new CommentDto {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = userId,
            Content = request.Content,
            CreatedDate = DateTime.UtcNow
        };

        TestData.TestCommentCollection.Add(comment.Id, comment);

        return CreatedAtAction(
            nameof(GetTaskComments),
            new { taskId },
            comment
        );
    }

    [HttpPost("{taskId}/labels")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AddTaskLabels(Guid taskId, [FromBody] List<LabelDto> labels) {
        if (!TestData.TestTaskCollection.TryGetValue(taskId, out var task))
            return NotFound($"Task with ID {taskId} not found");

        // In a real implementation, this would save to a task-labels collection
        return Ok(task);
    }

    [HttpPost("{taskId}/assignees")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AddTaskAssignees(Guid taskId, [FromBody] List<Guid> userIds) {
        if (!TestData.TestTaskCollection.TryGetValue(taskId, out var task))
            return NotFound($"Task with ID {taskId} not found");

        // Validate all user IDs exist
        foreach (var userId in userIds) {
            if (!TestData.TestUserCollection.ContainsKey(userId))
                return BadRequest($"User with ID {userId} not found");
        }

        // In a real implementation, this would save to a task-assignees collection
        return Ok(task);
    }
}
