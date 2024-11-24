using Zello.Domain.Entities.Dto;
using Zello.Domain.Enums;
using Zello.Infrastructure.TestingDataStorage;

public class TestDataSnapshot : IDisposable {
    private static readonly object _lock = new();
    private readonly Dictionary<Guid, ProjectDto> _projects = new();
    private readonly Dictionary<Guid, WorkspaceDto> _workspaces = new();
    private readonly Dictionary<Guid, UserDto> _users = new();
    private readonly Dictionary<Guid, WorkspaceMemberDto> _workspaceMembers = new();
    private readonly Dictionary<Guid, ProjectMemberDto> _projectMembers = new();
    private readonly Dictionary<Guid, ListDto> _lists = new();
    private readonly Dictionary<Guid, TaskDto> _tasks = new();
    private readonly Dictionary<Guid, CommentDto> _comments = new();

    public TestDataSnapshot() {
        lock (_lock) {
            // Ensure test data exists first
            EnsureTestDataExists();

            // Take a snapshot of all collections
            foreach (var kvp in TestData.TestProjectCollection)
                _projects[kvp.Key] = kvp.Value;

            foreach (var kvp in TestData.TestWorkspaceCollection)
                _workspaces[kvp.Key] = kvp.Value;

            foreach (var kvp in TestData.TestUserCollection)
                _users[kvp.Key] = kvp.Value;

            foreach (var kvp in TestData.TestWorkspaceMemberCollection)
                _workspaceMembers[kvp.Key] = kvp.Value;

            foreach (var kvp in TestData.TestProjectMemberCollection)
                _projectMembers[kvp.Key] = kvp.Value;

            foreach (var kvp in TestData.TestListCollection)
                _lists[kvp.Key] = kvp.Value;

            foreach (var kvp in TestData.TestTaskCollection)
                _tasks[kvp.Key] = kvp.Value;

            foreach (var kvp in TestData.TestCommentCollection)
                _comments[kvp.Key] = kvp.Value;
        }
    }

    private void EnsureTestDataExists() {
        // Create a user if none exists
        if (!TestData.TestUserCollection.Any()) {
            var user = new UserDto(
                Guid.NewGuid(),
                "testuser",
                "test@example.com",
                "Test User"
            );
            TestData.TestUserCollection.Add(user.Id, user);
        }

        var userId = TestData.TestUserCollection.First().Key;

        // Create a workspace if none exists
        if (!TestData.TestWorkspaceCollection.Any()) {
            var workspace = new WorkspaceDto {
                Id = Guid.NewGuid(),
                Name = "Test Workspace",
                OwnerId = userId,
                CreatedDate = DateTime.UtcNow,
                Projects = new List<ProjectDto>()
            };
            TestData.TestWorkspaceCollection.Add(workspace.Id, workspace);
        }

        var workspaceId = TestData.TestWorkspaceCollection.First().Key;

        // Create a workspace member if none exists
        if (!TestData.TestWorkspaceMemberCollection.Any()) {
            var workspaceMember = new WorkspaceMemberDto {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            TestData.TestWorkspaceMemberCollection.Add(workspaceMember.Id, workspaceMember);
        }

        // Create a project if none exists
        if (!TestData.TestProjectCollection.Any()) {
            var project = new ProjectDto {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                Name = "Test Project",
                Description = "Test Project Description",
                CreatedDate = DateTime.UtcNow
            };
            TestData.TestProjectCollection.Add(project.Id, project);
        }

        var projectId = TestData.TestProjectCollection.First().Key;

        // Create a list if none exists
        if (!TestData.TestListCollection.Any()) {
            var list = new ListDto {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = "Test List",
                Position = 0,
                CreatedDate = DateTime.UtcNow,
                Tasks = new List<TaskDto>()
            };
            TestData.TestListCollection.Add(list.Id, list);
        }

        var listId = TestData.TestListCollection.First().Key;

        // Create a task if none exists
        if (!TestData.TestTaskCollection.Any()) {
            var task = new TaskDto {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                ListId = listId,
                Name = "Test Task",
                Description = "Test Task Description",
                CreatedDate = DateTime.UtcNow,
                Status = CurrentTaskStatus.NotStarted,
                Priority = Priority.Medium
            };
            TestData.TestTaskCollection.Add(task.Id, task);
        }
    }

    public void RestoreState() {
        lock (_lock) {
            TestData.TestProjectCollection.Clear();
            foreach (var kvp in _projects)
                TestData.TestProjectCollection.Add(kvp.Key, kvp.Value);

            TestData.TestWorkspaceCollection.Clear();
            foreach (var kvp in _workspaces)
                TestData.TestWorkspaceCollection.Add(kvp.Key, kvp.Value);

            TestData.TestUserCollection.Clear();
            foreach (var kvp in _users)
                TestData.TestUserCollection.Add(kvp.Key, kvp.Value);

            TestData.TestWorkspaceMemberCollection.Clear();
            foreach (var kvp in _workspaceMembers)
                TestData.TestWorkspaceMemberCollection.Add(kvp.Key, kvp.Value);

            TestData.TestProjectMemberCollection.Clear();
            foreach (var kvp in _projectMembers)
                TestData.TestProjectMemberCollection.Add(kvp.Key, kvp.Value);

            TestData.TestListCollection.Clear();
            foreach (var kvp in _lists)
                TestData.TestListCollection.Add(kvp.Key, kvp.Value);

            TestData.TestTaskCollection.Clear();
            foreach (var kvp in _tasks)
                TestData.TestTaskCollection.Add(kvp.Key, kvp.Value);

            TestData.TestCommentCollection.Clear();
            foreach (var kvp in _comments)
                TestData.TestCommentCollection.Add(kvp.Key, kvp.Value);
        }
    }

    public void Dispose() {
        RestoreState();
    }
}
