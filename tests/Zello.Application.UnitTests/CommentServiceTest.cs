using Moq;
using Zello.Application.Dtos;
using Zello.Application.ServiceImplementations;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;

namespace Zello.Application.Tests.ServiceTests {
    public class CommentServiceTests {
        private readonly Mock<ICommentRepository> _mockCommentRepository;
        private readonly Mock<IWorkTaskRepository> _mockWorkTaskRepository;
        private readonly CommentService _commentService;

        public CommentServiceTests() {
            _mockCommentRepository = new Mock<ICommentRepository>();
            _mockWorkTaskRepository = new Mock<IWorkTaskRepository>();
            _commentService = new CommentService(_mockCommentRepository.Object, _mockWorkTaskRepository.Object);
        }

        #region GetCommentByIdAsync Tests
        [Fact]
        public async Task GetCommentByIdAsync_ExistingComment_ReturnsCommentReadDto() {
            // Arrange
            var commentId = Guid.NewGuid();
            var comment = CreateMockComment(commentId);

            _mockCommentRepository
                .Setup(repo => repo.GetCommentByIdAsync(commentId))
                .ReturnsAsync(comment);

            // Act
            var result = await _commentService.GetCommentByIdAsync(commentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(commentId, result.Id);
            Assert.Equal(comment.Content, result.Content);
        }

        [Fact]
        public async Task GetCommentByIdAsync_NonExistingComment_ThrowsException() {
            // Arrange
            var commentId = Guid.NewGuid();

            _mockCommentRepository
                .Setup(repo => repo.GetCommentByIdAsync(commentId))
                .ReturnsAsync((Comment)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _commentService.GetCommentByIdAsync(commentId));
        }
        #endregion

        #region GetCommentsByTaskIdAsync Tests
        [Fact]
        public async Task GetCommentsByTaskIdAsync_ExistingTask_ReturnsCommentReadDtos() {
            // Arrange
            var taskId = Guid.NewGuid();
            var comments = new List<Comment>
            {
                CreateMockComment(Guid.NewGuid(), taskId),
                CreateMockComment(Guid.NewGuid(), taskId)
            };

            _mockCommentRepository
                .Setup(repo => repo.GetCommentsByTaskIdAsync(taskId))
                .ReturnsAsync(comments);

            // Act
            var result = await _commentService.GetCommentsByTaskIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
        #endregion

        #region CreateCommentAsync Tests
        // [Fact]
        // public async Task CreateCommentAsync_ValidInput_CreatesComment() {
        //   // Arrange
        //   var taskId = Guid.NewGuid();
        //   var userId = Guid.NewGuid();
        //   var createDto = new CommentCreateDto {
        //     TaskId = taskId,
        //     Content = "Test Comment"
        //   };

        //   var workTask = new WorkTask { Id = taskId };

        //   _mockWorkTaskRepository
        //       .Setup(repo => repo.GetTaskByIdAsync(taskId))
        //       .ReturnsAsync(workTask);

        //   _mockCommentRepository
        //       .Setup(repo => repo.AddCommentAsync(It.IsAny<Comment>()))
        //       .ReturnsAsync((Comment)null);

        //   // Act
        //   var result = await _commentService.CreateCommentAsync(createDto, userId);

        //   // Assert
        //   Assert.NotNull(result);
        //   Assert.Equal(createDto.Content, result.Content);
        //   Assert.Equal(taskId, result.TaskId);
        //   Assert.Equal(userId, result.UserId);

        //   _mockCommentRepository.Verify(repo => repo.AddCommentAsync(It.IsAny<Comment>()), Times.Once);
        // }

        [Fact]
        public async Task CreateCommentAsync_NonExistingTask_ThrowsException() {
            // Arrange
            var taskId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var createDto = new CommentCreateDto {
                TaskId = taskId,
                Content = "Test Comment"
            };

            _mockWorkTaskRepository
                .Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync((WorkTask)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _commentService.CreateCommentAsync(createDto, userId));
        }
        #endregion

        #region UpdateCommentAsync Tests
        [Fact]
        public async Task UpdateCommentAsync_ExistingComment_UpdatesComment() {
            // Arrange
            var commentId = Guid.NewGuid();
            var existingComment = CreateMockComment(commentId);
            var updateDto = new CommentUpdateDto {
                Content = "Updated Comment"
            };

            _mockCommentRepository
                .Setup(repo => repo.GetCommentByIdAsync(commentId))
                .ReturnsAsync(existingComment);

            _mockCommentRepository
                .Setup(repo => repo.UpdateCommentAsync(It.IsAny<Comment>()))
                .Returns(Task.FromResult(existingComment));

            // Act
            var result = await _commentService.UpdateCommentAsync(commentId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Content, result.Content);

            _mockCommentRepository.Verify(repo => repo.UpdateCommentAsync(It.IsAny<Comment>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCommentAsync_NonExistingComment_ThrowsException() {
            // Arrange
            var commentId = Guid.NewGuid();
            var updateDto = new CommentUpdateDto {
                Content = "Updated Comment"
            };

            _mockCommentRepository
                .Setup(repo => repo.GetCommentByIdAsync(commentId))
                .ReturnsAsync((Comment)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _commentService.UpdateCommentAsync(commentId, updateDto));
        }
        #endregion

        #region DeleteCommentAsync Tests
        [Fact]
        public async Task DeleteCommentAsync_ValidCommentId_DeletesComment() {
            // Arrange
            var commentId = Guid.NewGuid();

            _mockCommentRepository
                .Setup(repo => repo.DeleteCommentAsync(commentId))
                .Returns(Task.FromResult(true));

            // Act
            await _commentService.DeleteCommentAsync(commentId);

            // Assert
            _mockCommentRepository.Verify(repo => repo.DeleteCommentAsync(commentId), Times.Once);
        }
        #endregion

        // Helper method to create mock comment with user
        private Comment CreateMockComment(Guid commentId, Guid? taskId = null) {
            return new Comment {
                Id = commentId,
                TaskId = taskId ?? Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Content = "Test Comment Content",
                CreatedDate = DateTime.UtcNow,
                User = new User {
                    Id = Guid.NewGuid(),
                    Name = "Test User",
                    Email = "test@example.com",
                    CreatedDate = DateTime.UtcNow,
                    Username = "testuser",
                    PasswordHash = "hashedpassword"
                }
            };
        }
    }
}
