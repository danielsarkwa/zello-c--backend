using Moq;
using Zello.Application.Dtos;
using Zello.Application.ServiceImplementations;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;

namespace Zello.Application.Tests.ServiceImplementations {
    public class TaskListServiceTests {
        private readonly Mock<ITaskListRepository> _mockTaskListRepository;
        private readonly TaskListService _service;

        public TaskListServiceTests() {
            _mockTaskListRepository = new Mock<ITaskListRepository>();
            _service = new TaskListService(_mockTaskListRepository.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTaskList_WhenTaskListExists() {
            // Arrange
            var taskListId = Guid.NewGuid();
            var taskList = new TaskList { Id = taskListId };
            var taskListDto = new ListReadDto { Id = taskListId };

            _mockTaskListRepository.Setup(r => r.GetByIdAsync(taskListId)).ReturnsAsync(taskList);

            // Act
            var result = await _service.GetByIdAsync(taskListId);

            // Assert
            Assert.Equal(taskListDto.Id, result?.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenTaskListDoesNotExist() {
            // Arrange
            var taskListId = Guid.NewGuid();
            _mockTaskListRepository.Setup(r => r.GetByIdAsync(taskListId)).ReturnsAsync((TaskList?)null);

            // Act
            var result = await _service.GetByIdAsync(taskListId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsTaskLists() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskLists = new List<TaskList> { new TaskList { Id = Guid.NewGuid() } };
            var taskListDtos = taskLists.Select(tl => new ListReadDto { Id = tl.Id }).ToList();

            _mockTaskListRepository.Setup(r => r.GetAllWithRelationsAsync(projectId)).ReturnsAsync(taskLists);

            // Act
            var result = await _service.GetAllAsync(projectId);

            // Assert
            Assert.Equal(taskListDtos.Count, result.Count());
            Assert.Equal(taskListDtos.First().Id, result.First().Id);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedTaskList_WhenUpdateIsSuccessful() {
            // Arrange
            var taskListId = Guid.NewGuid();
            var updateDto = new ListUpdateDto { Name = "Updated Name", Position = 1 };
            var taskList = new TaskList { Id = taskListId, Name = "Old Name", ProjectId = Guid.NewGuid(), Position = 0 };
            var updatedTaskListDto = new ListReadDto { Id = taskListId, Name = updateDto.Name, Position = updateDto.Position };

            _mockTaskListRepository.Setup(r => r.GetByIdAsync(taskListId)).ReturnsAsync(taskList);
            _mockTaskListRepository.Setup(r => r.UpdateAsync(taskList)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(taskListId, updateDto);

            // Assert
            Assert.Equal(updatedTaskListDto.Id, result.Id);
            Assert.Equal(updatedTaskListDto.Name, result.Name);
            Assert.Equal(updatedTaskListDto.Position, result.Position);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenTaskListDoesNotExist() {
            // Arrange
            var taskListId = Guid.NewGuid();
            var updateDto = new ListUpdateDto() { Name = "Updated Name", Position = 1 };
            _mockTaskListRepository.Setup(r => r.GetByIdAsync(taskListId)).ReturnsAsync((TaskList?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(taskListId, updateDto));
        }

        [Fact]
        public async Task UpdatePositionAsync_ReturnsUpdatedTaskList_WhenUpdateIsSuccessful() {
            // Arrange
            var taskListId = Guid.NewGuid();
            var newPosition = 1;
            var taskList = new TaskList { Id = taskListId, Position = 0 };
            var updatedTaskList = new TaskList { Id = taskListId, Position = newPosition };

            _mockTaskListRepository.Setup(r => r.UpdatePositionAsync(taskListId, newPosition)).ReturnsAsync(updatedTaskList);

            // Act
            var result = await _service.UpdatePositionAsync(taskListId, newPosition);

            // Assert
            Assert.Equal(updatedTaskList.Id, result.Id);
            Assert.Equal(updatedTaskList.Position, result.Position);
        }

        [Fact]
        public async Task CreateTaskAsync_ReturnsCreatedTask_WhenTaskIsCreated() {
            // Arrange
            var taskListId = Guid.NewGuid();
            var createDto = new TaskCreateDto { Name = "New Task", Description = "Task Description" };
            var userId = Guid.NewGuid();
            var taskList = new TaskList {
                Id = taskListId,
                Project = new Project {
                    Workspace = new Workspace {
                        Members = new List<WorkspaceMember>
                        {
                            new WorkspaceMember { UserId = userId }
                        }
                    }
                }
            };
            var taskId = Guid.NewGuid();
            var task = new WorkTask { Id = taskId, Name = createDto.Name, Description = createDto.Description };
            var taskDto = new TaskReadDto { Id = taskId, Name = createDto.Name, Description = createDto.Description };

            _mockTaskListRepository.Setup(r => r.GetByIdWithRelationsAsync(taskListId)).ReturnsAsync(taskList);
            _mockTaskListRepository.Setup(r => r.AddTaskAsync(It.IsAny<WorkTask>())).Callback<WorkTask>(t => t.Id = taskId).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateTaskAsync(taskListId, createDto, userId);

            // Assert
            Assert.Equal(taskDto.Id, result.Id);
            Assert.Equal(taskDto.Name, result.Name);
            Assert.Equal(taskDto.Description, result.Description);
        }
    }
}
