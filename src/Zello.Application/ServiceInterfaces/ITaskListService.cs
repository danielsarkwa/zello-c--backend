using Zello.Application.Dtos;

namespace Zello.Application.ServiceInterfaces;

public interface ITaskListService {
    Task<ListReadDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ListReadDto>> GetAllAsync(Guid? projectId);
    Task<ListReadDto> UpdateAsync(Guid id, ListUpdateDto updateDto);
    Task<ListReadDto?> UpdatePositionAsync(Guid id, int newPosition);
    Task<TaskReadDto?> CreateTaskAsync(TaskCreateDto createDto, Guid userId);
    Task<IEnumerable<TaskReadDto>?> GetListTasksAsync(Guid listId);
}
