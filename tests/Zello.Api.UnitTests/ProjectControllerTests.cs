// using Microsoft.AspNetCore.Mvc;
// using Xunit;
// using Zello.Api.Controllers;
// using Zello.Domain.Entities.Dto;
// using Zello.Domain.Enums;
// using Zello.Application.Features.Projects;
//
// namespace Zello.Api.UnitTests;
//
// public class ProjectControllerTests {
//     private readonly ProjectController _controller;
//
//     public ProjectControllerTests() {
//         _controller = new ProjectController();
//     }
//
//     [Fact]
//     public void CreateProject_ValidInput_ReturnsCreatedResult() {
//         // Arrange
//         var project = new ProjectDto {
//             WorkspaceId = Guid.NewGuid(),
//             Name = "Test Project",
//             Description = "Test Description",
//             StartDate = DateTime.UtcNow,
//             Status = ProjectStatus.InProgress
//         };
//
//         // Act
//         var result = _controller.CreateProject(project);
//
//         // Assert
//         var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//         Assert.Equal("Invalid workspace ID", badRequestResult.Value);
//     }
//
//     [Fact]
//     public void GetProjectById_NonexistentId_ReturnsNotFound() {
//         // Arrange
//         var projectId = Guid.NewGuid();
//
//         // Act
//         var result = _controller.GetProjectById(projectId);
//
//         // Assert
//         var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//         Assert.Contains(projectId.ToString(), notFoundResult.Value.ToString());
//     }
//
//     [Fact]
//     public void GetAllProjects_ReturnsOkResult() {
//         // Act
//         var result = _controller.GetAllProjects();
//
//         // Assert
//         Assert.IsType<OkObjectResult>(result);
//     }
//
//     [Fact]
//     public void GetAllProjects_WithWorkspaceFilter_ReturnsOkResult() {
//         // Arrange
//         var workspaceId = Guid.NewGuid();
//
//         // Act
//         var result = _controller.GetAllProjects(workspaceId);
//
//         // Assert
//         Assert.IsType<OkObjectResult>(result);
//     }
//
//     [Fact]
//     public void UpdateProject_NonexistentId_ReturnsNotFound() {
//         // Arrange
//         var projectId = Guid.NewGuid();
//         var project = new ProjectDto {
//             Name = "Updated Project",
//             Description = "Updated Description",
//             Status = ProjectStatus.InProgress
//         };
//
//         // Act
//         var result = _controller.UpdateProject(projectId, project);
//
//         // Assert
//         var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//         Assert.Contains(projectId.ToString(), notFoundResult.Value.ToString());
//     }
//
//     [Fact]
//     public void DeleteProject_NonexistentId_ReturnsNotFound() {
//         // Arrange
//         var projectId = Guid.NewGuid();
//
//         // Act
//         var result = _controller.DeleteProject(projectId);
//
//         // Assert
//         var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//         Assert.Contains(projectId.ToString(), notFoundResult.Value.ToString());
//     }
//
//     [Fact]
//     public void AddProjectMember_NonexistentProject_ReturnsNotFound() {
//         // Arrange
//         var projectId = Guid.NewGuid();
//         var createMember = new CreateProjectMemberDto {
//             WorkspaceMemberId = Guid.NewGuid()
//         };
//
//         // Act
//         var result = _controller.AddProjectMember(projectId, createMember);
//
//         // Assert
//         var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//         Assert.Contains(projectId.ToString(), notFoundResult.Value.ToString());
//     }
//
//     [Fact]
//     public void CreateList_NonexistentProject_ReturnsNotFound() {
//         // Arrange
//         var projectId = Guid.NewGuid();
//         var list = new ListDto {
//             Name = "Test List"
//         };
//
//         // Act
//         var result = _controller.CreateList(projectId, list);
//
//         // Assert
//         var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//         Assert.Contains(projectId.ToString(), notFoundResult.Value.ToString());
//     }
//
//     [Fact]
//     public void GetProjectLists_NonexistentProject_ReturnsNotFound() {
//         // Arrange
//         var projectId = Guid.NewGuid();
//
//         // Act
//         var result = _controller.GetProjectLists(projectId);
//
//         // Assert
//         var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//         Assert.Contains(projectId.ToString(), notFoundResult.Value.ToString());
//     }
// }
