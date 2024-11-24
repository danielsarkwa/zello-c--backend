using Microsoft.AspNetCore.Mvc;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.TestingDataStorage;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class ListController : ControllerBase {
    [HttpGet("{listId}")]
    [ProducesResponseType(typeof(ListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetListById(Guid listId) {
        if (!TestData.TestListCollection.TryGetValue(listId, out var list))
            return NotFound($"List with ID {listId} not found");

        return Ok(list);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ListDto>), StatusCodes.Status200OK)]
    public IActionResult GetAllLists([FromQuery] Guid? projectId = null) {
        var lists = TestData.TestListCollection.Values
            .Where(l => !projectId.HasValue || l.ProjectId == projectId.Value)
            .Select(l => {
                var tasks = TestData.TestTaskCollection.Values
                    .Where(t => t.ListId == l.Id)
                    .Select(t => {
                        // Ensure all navigation collections are initialized
                        if (t.Assignees == null) t.Assignees = new List<TaskAssigneeDto>();
                        if (t.Comments == null) t.Comments = new List<CommentDto>();
                        return t;
                    })
                    .ToList();

                l.Tasks = tasks;
                return l;
            })
            .OrderBy(l => l.Position)
            .ToList();

        return Ok(lists);
    }

    [HttpPut("{listId}")]
    [ProducesResponseType(typeof(ListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateList(Guid listId, [FromBody] UpdateListDto updatedList) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!TestData.TestListCollection.ContainsKey(listId))
            return NotFound($"List with ID {listId} not found");

        var existingList = TestData.TestListCollection[listId];

        // Update only the modifiable properties
        existingList.Name = updatedList.Name;
        existingList.Position = updatedList.Position;

        TestData.TestListCollection[listId] = existingList;

        return Ok(existingList);
    }

    [HttpPut("{listId}/position")]
    [ProducesResponseType(typeof(ListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateListPosition(Guid listId, [FromBody] int newPosition) {
        if (!TestData.TestListCollection.TryGetValue(listId, out var list))
            return NotFound($"List with ID {listId} not found");

        var projectLists = TestData.TestListCollection.Values
            .Where(l => l.ProjectId == list.ProjectId)
            .OrderBy(l => l.Position)
            .ToList();

        // Validate position
        if (newPosition < 0 || newPosition >= projectLists.Count)
            return BadRequest("Invalid position");

        // Update positions
        var oldPosition = list.Position;
        if (newPosition < oldPosition) {
            // Moving left: increment positions of lists between new and old positions
            foreach (var l in projectLists.Where(l =>
                         l.Position >= newPosition && l.Position < oldPosition)) {
                l.Position++;
                TestData.TestListCollection[l.Id] = l;
            }
        } else if (newPosition > oldPosition) {
            // Moving right: decrement positions of lists between old and new positions
            foreach (var l in projectLists.Where(l =>
                         l.Position > oldPosition && l.Position <= newPosition)) {
                l.Position--;
                TestData.TestListCollection[l.Id] = l;
            }
        }

        list.Position = newPosition;
        TestData.TestListCollection[listId] = list;

        return Ok(list);
    }

    [HttpDelete("{listId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteList(Guid listId) {
        if (!TestData.TestListCollection.ContainsKey(listId))
            return NotFound($"List with ID {listId} not found");

        var list = TestData.TestListCollection[listId];

        // Remove the list
        TestData.TestListCollection.Remove(listId);

        // Reorder remaining lists in the project
        var projectLists = TestData.TestListCollection.Values
            .Where(l => l.ProjectId == list.ProjectId)
            .OrderBy(l => l.Position)
            .ToList();

        for (int i = 0; i < projectLists.Count; i++) {
            projectLists[i].Position = i;
            TestData.TestListCollection[projectLists[i].Id] = projectLists[i];
        }

        // Remove any tasks in this list
        var tasksToRemove = TestData.TestTaskCollection.Values
            .Where(t => t.ListId == listId)
            .Select(t => t.Id)
            .ToList();

        foreach (var taskId in tasksToRemove) {
            TestData.TestTaskCollection.Remove(taskId);
        }

        return NoContent();
    }

    [HttpPost("{listId}/tasks")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CreateTask(Guid listId, [FromBody] CreateTaskDto createTask) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!TestData.TestListCollection.TryGetValue(listId, out var list))
            return NotFound($"List with ID {listId} not found");

        var task = new TaskDto {
            Id = Guid.NewGuid(),
            Name = createTask.Name,
            Description = createTask.Description,
            Status = createTask.Status,
            Priority = createTask.Priority,
            Deadline = createTask.Deadline,
            ListId = listId,
            ProjectId = list.ProjectId,
            CreatedDate = DateTime.UtcNow,
            Comments = new List<CommentDto>(),
            Assignees = new List<TaskAssigneeDto>(),
        };

        TestData.TestTaskCollection.Add(task.Id, task);

        return CreatedAtAction(
            nameof(GetListById),
            new { listId },
            task
        );
    }

    [HttpGet("{listId}/tasks")]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetListTasks(Guid listId) {
        if (!TestData.TestListCollection.ContainsKey(listId))
            return NotFound($"List with ID {listId} not found");

        var tasks = TestData.TestTaskCollection.Values
            .Where(t => t.ListId == listId)
            .Select(t => {
                // Ensure all navigation collections are initialized
                if (t.Assignees == null) t.Assignees = new List<TaskAssigneeDto>();
                if (t.Comments == null) t.Comments = new List<CommentDto>();
                return t;
            })
            .OrderBy(t => t.CreatedDate)
            .ToList();

        return Ok(tasks);
    }
}
