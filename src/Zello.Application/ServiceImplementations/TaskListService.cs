using Zello.Application.Dtos;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;

namespace Zello.Application.ServiceImplementations;

public class TaskListService : ITaskListService {
    private readonly ITaskListRepository _taskListRepository;

    public TaskListService(ITaskListRepository taskListRepository) {
        _taskListRepository = taskListRepository;
    }

    public async Task<ListReadDto?> GetByIdAsync(Guid id) {
        var list = await _taskListRepository.GetByIdWithRelationsAsync(id);
        if (list == null) return null;

        return new ListReadDto {
            Id = list.Id,
            Name = list.Name,
            ProjectId = list.ProjectId,
            Position = list.Position,
            Tasks = list.Tasks.Select(t => new TaskReadDto {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                Deadline = t.Deadline,
                ListId = t.ListId
            }).ToList()
        };
    }

    public async Task<IEnumerable<ListReadDto>> GetAllAsync(Guid? projectId) {
        var lists = await _taskListRepository.GetAllWithRelationsAsync(projectId);
        return lists.Select(list => new ListReadDto {
            Id = list.Id,
            Name = list.Name,
            ProjectId = list.ProjectId,
            Position = list.Position
        });
    }

    public async Task<ListReadDto> UpdateAsync(Guid id, ListUpdateDto updateDto) {
        var list = await _taskListRepository.GetByIdAsync(id);
        if (list == null) throw new KeyNotFoundException("Task list not found");

        list.Name = updateDto.Name ?? list.Name;
        list.ProjectId = list.ProjectId;
        list.Position = updateDto.Position >= 0 ? updateDto.Position : list.Position;

        await _taskListRepository.UpdateAsync(list);
        return new ListReadDto {
            Id = list.Id,
            Name = list.Name,
            ProjectId = list.ProjectId,
            Position = list.Position
        };
    }

    public async Task<ListReadDto?> UpdatePositionAsync(Guid id, int newPosition) {
        var list = await _taskListRepository.UpdatePositionAsync(id, newPosition);
        if (list == null) return null;

        return new ListReadDto {
            Id = list.Id,
            Name = list.Name,
            ProjectId = list.ProjectId,
            Position = list.Position
        };
    }

    public async Task<TaskReadDto?> CreateTaskAsync(TaskCreateDto createDto, Guid userId) {
        var list = await _taskListRepository.GetByIdWithRelationsAsync(createDto.ListId);
        if (list == null) return null;

        var task = new WorkTask {
            Id = Guid.NewGuid(),
            ListId = createDto.ListId,
            ProjectId = list.ProjectId,
            CreatedDate = DateTime.UtcNow,
            Status = createDto.Status,
            Priority = createDto.Priority,
            Name = createDto.Name,
            Description = createDto.Description ?? "",
        };

        await _taskListRepository.AddTaskAsync(task);
        return new TaskReadDto {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            CreatedDate = task.CreatedDate,
            Deadline = task.Deadline,
            Priority = task.Priority,
            Status = task.Status,
            ProjectId = task.ProjectId,
            ListId = task.ListId,
            Assignees = task.Assignees.Select(a => new TaskAssigneeReadDto { UserId = a.UserId }).ToList(),
            Comments = task.Comments.Select(c => new CommentReadDto { Id = c.Id, Content = c.Content }).ToList()
        };
    }

    public async Task<IEnumerable<TaskReadDto>?> GetListTasksAsync(Guid listId) {
        if (!await _taskListRepository.ExistsAsync(listId))
            return null;

        var tasks = await _taskListRepository.GetListTasksAsync(listId);
        return tasks?.Select(TaskReadDto.FromEntity);
    }
}
