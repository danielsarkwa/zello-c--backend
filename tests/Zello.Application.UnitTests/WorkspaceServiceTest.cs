using Moq;
using Xunit;
using Zello.Application.Dtos;
using Zello.Application.Exceptions;
using Zello.Application.ServiceImplementations;
using Zello.Application.ServiceInterfaces;
using Zello.Application.ServiceInterfaces.ExceptionInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.RepositoryInterfaces;

namespace Zello.Application.Tests.ServiceTests {
    public class WorkspaceServiceTests {
        private readonly Mock<IWorkspaceRepository> _mockWorkspaceRepository;
        private readonly Mock<IWorkspaceValidationService> _mockValidationService;
        private readonly WorkspaceService _workspaceService;

        public WorkspaceServiceTests() {
            _mockWorkspaceRepository = new Mock<IWorkspaceRepository>();
            _mockValidationService = new Mock<IWorkspaceValidationService>();
            _workspaceService = new WorkspaceService(
                _mockWorkspaceRepository.Object,
                _mockValidationService.Object);
        }

        #region CreateWorkspaceAsync Tests
        [Fact]
        public async Task CreateWorkspaceAsync_ValidInput_ReturnsWorkspaceReadDto() {
            // Arrange
            var userId = Guid.NewGuid();
            var createDto = new WorkspaceCreateDto {
                Name = "Test Workspace",
            };
            var workspace = new Workspace { Id = Guid.NewGuid(), Name = createDto.Name };
            var workspaceMember = new WorkspaceMember { UserId = userId, WorkspaceId = workspace.Id };

            _mockValidationService
                .Setup(v => v.EnsureUserExists(userId))
                .Returns(Task.FromResult(workspaceMember));

            _mockWorkspaceRepository
                .Setup(r => r.AddAsync(It.IsAny<Workspace>()))
                .ReturnsAsync(workspace);

            // Act
            var result = await _workspaceService.CreateWorkspaceAsync(createDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workspace.Name, result.Name);

            _mockValidationService.Verify(v => v.EnsureUserExists(userId), Times.Once);
            _mockWorkspaceRepository.Verify(r => r.AddAsync(It.IsAny<Workspace>()), Times.Once);
        }
        #endregion

        #region GetAllWorkspacesAsync Tests
        [Fact]
        public async Task GetAllWorkspacesAsync_AdminAccess_ReturnsAllWorkspaces() {
            // Arrange
            var userId = Guid.NewGuid();
            var workspaces = new List<Workspace>
            {
                new Workspace { Id = Guid.NewGuid(), Name = "Workspace 1" },
                new Workspace { Id = Guid.NewGuid(), Name = "Workspace 2" }
            };
            var workspaceId = workspaces[0].Id;

            _mockValidationService
                .Setup(v => v.EnsureUserExists(userId))
                .Returns(Task.FromResult(new WorkspaceMember { UserId = userId, WorkspaceId = workspaceId }));

            _mockWorkspaceRepository
                .Setup(r => r.GetAllWorkspacesWithDetailsAsync())
                .ReturnsAsync(workspaces);

            // Act
            var result = await _workspaceService.GetAllWorkspacesAsync(userId, AccessLevel.Admin);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllWorkspacesAsync_NonAdminAccess_ReturnsOnlyMemberWorkspaces() {
            // Arrange
            var userId = Guid.NewGuid();
            var workspaces = new List<Workspace>
            {
                new Workspace
                {
                    Id = Guid.NewGuid(),
                    Name = "Workspace 1",
                    Members = new List<WorkspaceMember>
                    {
                        new WorkspaceMember { UserId = userId }
                    }
                },
                new Workspace
                {
                    Id = Guid.NewGuid(),
                    Name = "Workspace 2",
                    Members = new List<WorkspaceMember>()
                }
            };

            _mockValidationService
                .Setup(v => v.EnsureUserExists(userId))
                .Returns(Task.FromResult(new WorkspaceMember()));

            _mockWorkspaceRepository
                .Setup(r => r.GetAllWorkspacesWithDetailsAsync())
                .ReturnsAsync(workspaces);

            // Act
            var result = await _workspaceService.GetAllWorkspacesAsync(userId, AccessLevel.Member);

            // Assert
            Assert.Single(result);
            Assert.Equal("Workspace 1", result[0].Name);
        }
        #endregion

        #region GetWorkspaceByIdAsync Tests
        [Fact]
        public async Task GetWorkspaceByIdAsync_ValidAccess_ReturnsWorkspace() {
            // Arrange
            var workspaceId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var workspace = new Workspace {
                Id = workspaceId,
                Name = "Test Workspace"
            };

            _mockWorkspaceRepository
                .Setup(r => r.GetWorkspaceWithDetailsAsync(workspaceId))
                .ReturnsAsync(workspace);

            _mockValidationService
                .Setup(v => v.EnsureWorkspaceExists(workspaceId))
                .Returns(Task.FromResult(new WorkspaceMember()));

            _mockValidationService
                .Setup(v => v.ValidateWorkspaceAccess(workspaceId, userId, It.IsAny<AccessLevel?>()))
                .Returns(Task.FromResult(new WorkspaceMember()));

            // Act
            var result = await _workspaceService.GetWorkspaceByIdAsync(workspaceId, userId, AccessLevel.Member);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workspace.Name, result.Name);
        }
        #endregion

        #region UpdateWorkspaceAsync Tests
        [Fact]
        public async Task UpdateWorkspaceAsync_ValidInput_ReturnsUpdatedWorkspace() {
            // Arrange
            var workspaceId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingWorkspace = new Workspace {
                Id = workspaceId,
                Name = "Old Name"
            };
            var updateDto = new WorkspaceUpdateDto {
                Name = "New Name"
            };

            _mockWorkspaceRepository
                .Setup(r => r.GetByIdAsync(workspaceId))
                .ReturnsAsync(existingWorkspace);

            _mockValidationService
                .Setup(v => v.EnsureWorkspaceExists(workspaceId))
                .Returns(Task.FromResult(new WorkspaceMember()));

            _mockValidationService
                .Setup(v => v.ValidateManagePermissions(workspaceId, userId, It.IsAny<AccessLevel?>()))
                .Returns(Task.CompletedTask);

            _mockWorkspaceRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Workspace>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _workspaceService.UpdateWorkspaceAsync(workspaceId, updateDto, userId, AccessLevel.Admin);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
        }
        #endregion

        #region DeleteWorkspaceAsync Tests
        [Fact]
        public async Task DeleteWorkspaceAsync_ValidInput_DeletesWorkspace() {
            // Arrange
            var workspaceId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingWorkspace = new Workspace { Id = workspaceId };

            _mockWorkspaceRepository
                .Setup(r => r.GetByIdAsync(workspaceId))
                .ReturnsAsync(existingWorkspace);

            _mockValidationService
                .Setup(v => v.EnsureWorkspaceExists(workspaceId))
                .Returns(Task.CompletedTask);

            _mockValidationService
                .Setup(v => v.ValidateManagePermissions(workspaceId, userId, It.IsAny<AccessLevel?>()))
                .Returns(Task.CompletedTask);

            _mockWorkspaceRepository
                .Setup(r => r.DeleteAsync(existingWorkspace))
                .Returns(Task.CompletedTask);

            // Act
            await _workspaceService.DeleteWorkspaceAsync(workspaceId, userId, AccessLevel.Admin);

            // Assert
            _mockWorkspaceRepository.Verify(r => r.DeleteAsync(existingWorkspace), Times.Once);
        }
        #endregion

        #region AddWorkspaceMemberAsync Tests
        [Fact]
        public async Task AddWorkspaceMemberAsync_ValidInput_ReturnsNewMember() {
            // Arrange
            var workspaceId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var createMemberDto = new WorkspaceMemberCreateDto {
                UserId = Guid.NewGuid(),
                AccessLevel = AccessLevel.Member
            };
            var workspaceMember = new WorkspaceMember {
                WorkspaceId = workspaceId,
                UserId = createMemberDto.UserId
            };

            _mockValidationService
                .Setup(v => v.EnsureWorkspaceExists(workspaceId))
                .Returns(Task.FromResult(new WorkspaceMember()));

            _mockValidationService
                .Setup(v => v.ValidateManagePermissions(workspaceId, userId, It.IsAny<AccessLevel?>()))
                .Returns(Task.CompletedTask);

            _mockValidationService
                .Setup(v => v.EnsureUserExists(createMemberDto.UserId))
                .Returns(Task.FromResult(new WorkspaceMember())); // Updated this line

            _mockValidationService
                .Setup(v => v.EnsureNotExistingMember(workspaceId, createMemberDto.UserId))
                .Returns(Task.CompletedTask);

            _mockValidationService
                .Setup(v => v.ValidateAccessLevelAssignment(
                    workspaceId, userId, createMemberDto.AccessLevel, It.IsAny<AccessLevel?>()))
                .Returns(Task.CompletedTask);

            _mockWorkspaceRepository
                .Setup(r => r.AddWorkspaceMemberAsync(It.IsAny<WorkspaceMember>()))
                .ReturnsAsync(workspaceMember);

            // Act
            var result = await _workspaceService.AddWorkspaceMemberAsync(
                workspaceId, createMemberDto, userId, AccessLevel.Admin);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workspaceId, result.WorkspaceId);
            Assert.Equal(createMemberDto.UserId, result.UserId);
        }
        #endregion

        #region UpdateMemberAccessAsync Tests
        [Fact]
        public async Task UpdateMemberAccessAsync_ValidInput_ReturnsUpdatedMember() {
            // Arrange
            var memberId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var workspaceId = Guid.NewGuid();
            var existingMember = new WorkspaceMember {
                Id = memberId,
                WorkspaceId = workspaceId,
                UserId = Guid.NewGuid(),
                AccessLevel = AccessLevel.Member
            };
            var updateDto = new WorkspaceMemberUpdateDto {
                Role = AccessLevel.Admin
            };

            _mockWorkspaceRepository
                .Setup(r => r.GetMemberByIdAsync(memberId))
                .ReturnsAsync(existingMember);

            _mockValidationService
                .Setup(v => v.ValidateManagePermissions(workspaceId, userId, It.IsAny<AccessLevel?>()))
                .Returns(Task.CompletedTask);

            _mockValidationService
                .Setup(v => v.ValidateAccessLevelAssignment(
                    workspaceId, userId, updateDto.Role, It.IsAny<AccessLevel?>()))
                .Returns(Task.CompletedTask);

            _mockWorkspaceRepository
                .Setup(r => r.UpdateMemberAsync(It.IsAny<WorkspaceMember>()))
                .Returns(Task.FromResult(new WorkspaceMember()));

            // Act
            var result = await _workspaceService.UpdateMemberAccessAsync(
                memberId, updateDto, userId, AccessLevel.Admin);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(AccessLevel.Admin, result.AccessLevel);
        }

        [Fact]
        public async Task UpdateMemberAccessAsync_MemberNotFound_ThrowsException() {
            // Arrange
            var memberId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var updateDto = new WorkspaceMemberUpdateDto {
                Role = AccessLevel.Admin
            };

            _mockWorkspaceRepository
                .Setup(r => r.GetMemberByIdAsync(memberId))
                .ReturnsAsync((WorkspaceMember)null);

            // Act & Assert
            await Assert.ThrowsAsync<WorkspaceMemberNotFoundException>(() =>
                _workspaceService.UpdateMemberAccessAsync(memberId, updateDto, userId, AccessLevel.Admin));
        }
        #endregion
    }
}