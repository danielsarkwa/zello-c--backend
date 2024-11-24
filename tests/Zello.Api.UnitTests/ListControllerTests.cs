using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Zello.Api.Controllers;
using Zello.Domain.Entities.Dto;
using Zello.Domain.Enums;
using Zello.Infrastructure.TestingDataStorage;

namespace Zello.Api.UnitTests;

public class ListControllerTests {
    private readonly ListController _controller;

    public ListControllerTests() {
        _controller = new ListController();
        TestData.ResetTestData();
    }

    [Fact]
    public void GetAllLists_ReturnsOkResult() {
        // Act
        var result = _controller.GetAllLists();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeAssignableTo<IEnumerable<ListDto>>();
    }

    [Fact]
    public void GetAllLists_WithProjectFilter_ReturnsFilteredLists() {
        // Arrange
        var projectId = TestData.TestListCollection.First().Value.ProjectId;

        // Act
        var result = _controller.GetAllLists(projectId) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var lists = result!.Value as IEnumerable<ListDto>;
        lists.Should().NotBeNull();
        lists!.All(l => l.ProjectId == projectId).Should().BeTrue();
    }

    [Fact]
    public void GetListById_WithInvalidId_ReturnsNotFound() {
        // Act
        var result = _controller.GetListById(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void GetListById_WithValidId_ReturnsCorrectList() {
        // Arrange
        var existingList = TestData.TestListCollection.First();

        // Act
        var result = _controller.GetListById(existingList.Key) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var list = result!.Value as ListDto;
        list.Should().NotBeNull();
        list!.Id.Should().Be(existingList.Key);
    }

    [Fact]
    public void UpdateList_WithValidData_ReturnsUpdatedList() {
        // Arrange
        var existingList = TestData.TestListCollection.First();
        var updateDto = new UpdateListDto {
            Name = "Updated Name",
            Position = 1
        };

        // Act
        var result = _controller.UpdateList(existingList.Key, updateDto) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var updatedList = result!.Value as ListDto;
        updatedList.Should().NotBeNull();
        updatedList!.Name.Should().Be("Updated Name");
        updatedList.Position.Should().Be(1);
    }

    [Fact]
    public void UpdateList_WithInvalidId_ReturnsNotFound() {
        // Arrange
        var updateDto = new UpdateListDto {
            Name = "Test Name",
            Position = 0
        };

        // Act
        var result = _controller.UpdateList(Guid.NewGuid(), updateDto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void GetListTasks_WithValidId_ReturnsTasks() {
        // Arrange
        var existingList = TestData.TestListCollection.First();

        // Act
        var result = _controller.GetListTasks(existingList.Key) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var tasks = result!.Value as IEnumerable<TaskDto>;
        tasks.Should().NotBeNull();
        tasks!.All(t => t.ListId == existingList.Key).Should().BeTrue();
    }

    [Fact]
    public void CreateTask_WithValidData_ReturnsCreatedTask() {
        // Arrange
        var existingList = TestData.TestListCollection.First();
        var createDto = new CreateTaskDto {
            Name = "New Task",
            Description = "Test Description",
            Status = CurrentTaskStatus.NotStarted,
            Priority = Priority.Medium,
            Deadline = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = _controller.CreateTask(existingList.Key, createDto) as CreatedAtActionResult;

        // Assert
        result.Should().NotBeNull();
        var task = result!.Value as TaskDto;
        task.Should().NotBeNull();
        task!.Name.Should().Be("New Task");
        task.ListId.Should().Be(existingList.Key);
        task.ProjectId.Should().Be(existingList.Value.ProjectId);
        task.Assignees.Should().NotBeNull();
        task.Comments.Should().NotBeNull();
    }


    [Fact]
    public void UpdateListPosition_WithValidData_UpdatesPositions() {
        // Arrange
        var list = TestData.TestListCollection
            .First(l => l.Value.Position > 0);
        var newPosition = list.Value.Position - 1;

        // Act
        var result = _controller.UpdateListPosition(list.Key, newPosition) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var updatedList = result!.Value as ListDto;
        updatedList.Should().NotBeNull();
        updatedList!.Position.Should().Be(newPosition);
    }

    [Fact]
    public void UpdateListPosition_WithTooHighPosition_ReturnsBadRequest() {
        // Arrange
        var list = TestData.TestListCollection.First();
        var invalidPosition = TestData.TestListCollection.Count + 1;

        // Act
        var result = _controller.UpdateListPosition(list.Key, invalidPosition);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }


    [Fact]
    public void DeleteList_WithValidId_ReturnsNoContent() {
        // Arrange
        var existingList = TestData.TestListCollection.First();

        // Act
        var result = _controller.DeleteList(existingList.Key);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void DeleteList_WithInvalidId_ReturnsNotFound() {
        // Act
        var result = _controller.DeleteList(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
