using Moq;
using Zello.Application.Dtos;
using Zello.Application.ServiceImplementations;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;

namespace Zello.Application.Tests {
    public class ProjectServiceTests {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IWorkspaceRepository> _mockWorkspaceRepository;
        private readonly Mock<IWorkspaceMemberRepository> _mockWorkspaceMemberRepository;
        private readonly ProjectService _projectService;

        public ProjectServiceTests() {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockWorkspaceRepository = new Mock<IWorkspaceRepository>();
            _mockWorkspaceMemberRepository = new Mock<IWorkspaceMemberRepository>();
            _projectService = new ProjectService(_mockProjectRepository.Object, _mockWorkspaceRepository.Object, _mockWorkspaceMemberRepository.Object);
        }

        [Fact]
        public async Task CreateProjectAsync_ShouldThrowInvalidOperationException_WhenWorkspaceDoesNotExist() {
            // Arrange
            var projectDto = new ProjectCreateDto { WorkspaceId = Guid.NewGuid() };
            var userId = Guid.NewGuid();

            _mockWorkspaceRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Workspace)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _projectService.CreateProjectAsync(projectDto, userId));
        }

        [Fact]
        public async Task CreateProjectAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotWorkspaceMember() {
            // Arrange
            var projectDto = new ProjectCreateDto { WorkspaceId = Guid.NewGuid() };
            var userId = Guid.NewGuid();

            _mockWorkspaceRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Workspace());

            _mockWorkspaceMemberRepository.Setup(repo => repo.GetWorkspaceMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((WorkspaceMember)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _projectService.CreateProjectAsync(projectDto, userId));
        }

        [Fact]
        public async Task CreateProjectAsync_ShouldCreateProject_WhenValid() {
            // Arrange
            var workspaceId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var workspaceMemberId = Guid.NewGuid();

            var projectDto = new ProjectCreateDto {
                WorkspaceId = workspaceId,
                Name = "Test Project"
            };

            var workspaceMember = new WorkspaceMember {
                Id = workspaceMemberId,
                WorkspaceId = workspaceId,
                UserId = userId
            };

            var testProject = new Project {
                Id = projectId,
                Name = "Test Project",
                WorkspaceId = workspaceId,
                CreatedDate = DateTime.UtcNow
            };

            _mockWorkspaceRepository.Setup(repo => repo.GetByIdAsync(workspaceId))
                .ReturnsAsync(new Workspace { Id = workspaceId });

            _mockWorkspaceMemberRepository.Setup(repo => repo.GetWorkspaceMemberAsync(workspaceId, userId))
                .ReturnsAsync(workspaceMember);

            _mockProjectRepository.Setup(repo => repo.AddAsync(It.IsAny<Project>()))
                .ReturnsAsync((Project project) => project);

            _mockProjectRepository.Setup(repo => repo.AddProjectMemberAsync(It.IsAny<ProjectMember>()))
                .Returns(Task.CompletedTask);

            _mockProjectRepository.Setup(repo => repo.GetProjectByIdWithDetailsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(testProject);

            // Act
            var result = await _projectService.CreateProjectAsync(projectDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testProject.Id, result.Id);
            Assert.Equal(testProject.Name, result.Name);
            _mockProjectRepository.Verify(repo => repo.AddAsync(It.IsAny<Project>()), Times.Once);
            _mockProjectRepository.Verify(repo => repo.AddProjectMemberAsync(It.IsAny<ProjectMember>()), Times.Once);
            _mockProjectRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
