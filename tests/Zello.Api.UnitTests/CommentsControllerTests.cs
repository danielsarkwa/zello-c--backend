// using FluentAssertions;
// using Microsoft.AspNetCore.Mvc;
// using Zello.Api.Controllers;
// using Zello.Application.Features.Comments.Models;
// using Zello.Domain.Entities.Requests;
// using Zello.Infrastructure.TestingDataStorage;
//
// namespace Zello.Api.UnitTests;
//
// public class CommentsControllerTests {
//     private readonly CommentsController _controller;
//
//     public CommentsControllerTests() {
//         _controller = new CommentsController();
//         TestData.ResetTestData(); // Reset to known state
//     }
//
//     [Fact]
//     public void GetComments_ReturnsOkResult() {
//         // Act
//         var result = _controller.GetComments();
//
//         // Assert
//         result.Should().BeOfType<OkObjectResult>();
//     }
//
//     [Fact]
//     public void GetCommentById_WithInvalidId_ReturnsNotFound() {
//         // Act
//         var result = _controller.GetCommentById(Guid.NewGuid());
//
//         // Assert
//         result.Should().BeOfType<NotFoundObjectResult>();
//     }
//
//     [Fact]
//     public void CreateComment_WithEmptyContent_ReturnsBadRequest() {
//         // Arrange
//         var request = new CreateCommentRequest {
//             TaskId = Guid.NewGuid(),
//             Content = string.Empty
//         };
//
//         // Act
//         var result = _controller.CreateComment(request);
//
//         // Assert
//         result.Should().BeOfType<BadRequestObjectResult>();
//     }
//
//     [Fact]
//     public void CreateComment_WithNonExistentTask_ReturnsNotFound() {
//         // Arrange
//         var request = new CreateCommentRequest {
//             TaskId = Guid.NewGuid(),
//             Content = "Test comment"
//         };
//
//         // Act
//         var result = _controller.CreateComment(request);
//
//         // Assert
//         result.Should().BeOfType<NotFoundObjectResult>();
//     }
//
//     [Fact]
//     public void UpdateComment_WithNonExistentComment_ReturnsNotFound() {
//         // Arrange
//         var request = new UpdateCommentRequest { Content = "Updated content" };
//
//         // Act
//         var result = _controller.UpdateComment(Guid.NewGuid(), request);
//
//         // Assert
//         result.Should().BeOfType<NotFoundObjectResult>();
//     }
//
//     [Fact]
//     public void DeleteComment_WithNonExistentComment_ReturnsNotFound() {
//         // Act
//         var result = _controller.DeleteComment(Guid.NewGuid());
//
//         // Assert
//         result.Should().BeOfType<NotFoundObjectResult>();
//     }
// }
