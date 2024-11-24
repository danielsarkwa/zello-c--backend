using Zello.Domain.Entities.Dto;
using Zello.Domain.Enums;

namespace Zello.Infrastructure.TestingDataStorage;

public static class TestData {
    public static readonly object _lock = new();

    // Fixed GUIDs for Users
    private static readonly Guid User1Id = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid User2Id = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid WorkspaceId = new("33333333-3333-3333-3333-333333333333");
    private static readonly Guid WorkspaceMember1Id = new("44444444-4444-4444-4444-444444444444");
    private static readonly Guid WorkspaceMember2Id = new("55555555-5555-5555-5555-555555555555");
    private static readonly Guid Project1Id = new("66666666-6666-6666-6666-666666666666");
    private static readonly Guid Project2Id = new("77777777-7777-7777-7777-777777777777");
    private static readonly Guid ProjectMember1Id = new("88888888-8888-8888-8888-888888888888");
    private static readonly Guid ProjectMember2Id = new("99999999-9999-9999-9999-999999999999");
    private static readonly Guid BacklogId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid InProgressId = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid MobileBacklogId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid MobileInProgressId = new("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid MobileDoneId = new("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private static readonly Guid Task1Id = new("ffffffff-ffff-ffff-ffff-ffffffffffff");
    private static readonly Guid Task2Id = new("11111111-2222-3333-4444-555555555555");
    private static readonly Guid MobileTask1Id = new("22222222-3333-4444-5555-666666666666");
    private static readonly Guid MobileTask2Id = new("33333333-4444-5555-6666-777777777777");
    private static readonly Guid MobileTask3Id = new("44444444-5555-6666-7777-888888888888");
    private static readonly Guid Comment1Id = new("55555555-6666-7777-8888-999999999999");
    private static readonly Guid Comment2Id = new("66666666-7777-8888-9999-aaaaaaaaaaaa");
    private static readonly Guid Comment3Id = new("77777777-8888-9999-aaaa-bbbbbbbbbbbb");


    private static readonly Dictionary<Guid, UserDto> _userCollection = new();
    private static readonly Dictionary<Guid, ListDto> _listCollection = new();
    private static readonly Dictionary<Guid, ProjectDto> _projectCollection = new();
    private static readonly Dictionary<Guid, TaskDto> _taskCollection = new();
    private static readonly Dictionary<Guid, WorkspaceDto> _workspaceCollection = new();
    private static readonly Dictionary<Guid, CommentDto> _commentCollection = new();
    private static readonly Dictionary<Guid, WorkspaceMemberDto> _workspaceMemberCollection = new();
    private static readonly Dictionary<Guid, ProjectMemberDto> _projectMemberCollection = new();

    // Public properties with thread-safe access
    public static Dictionary<Guid, UserDto> TestUserCollection {
        get {
            lock (_lock) {
                return new Dictionary<Guid, UserDto>(_userCollection);
            }
        }
    }

    public static Dictionary<Guid, ListDto> TestListCollection {
        get {
            lock (_lock) {
                return new Dictionary<Guid, ListDto>(_listCollection);
            }
        }
    }

    public static Dictionary<Guid, ProjectDto> TestProjectCollection {
        get {
            lock (_lock) {
                return new Dictionary<Guid, ProjectDto>(_projectCollection);
            }
        }
    }

    public static Dictionary<Guid, TaskDto> TestTaskCollection {
        get {
            lock (_lock) {
                return new Dictionary<Guid, TaskDto>(_taskCollection);
            }
        }
    }

    public static Dictionary<Guid, WorkspaceDto> TestWorkspaceCollection {
        get {
            lock (_lock) {
                return new Dictionary<Guid, WorkspaceDto>(_workspaceCollection);
            }
        }
    }

    public static Dictionary<Guid, CommentDto> TestCommentCollection {
        get {
            lock (_lock) {
                return new Dictionary<Guid, CommentDto>(_commentCollection);
            }
        }
    }

    public static Dictionary<Guid, WorkspaceMemberDto> TestWorkspaceMemberCollection {
        get {
            lock (_lock) {
                return new Dictionary<Guid, WorkspaceMemberDto>(_workspaceMemberCollection);
            }
        }
    }

    public static Dictionary<Guid, ProjectMemberDto> TestProjectMemberCollection {
        get {
            lock (_lock) {
                return new Dictionary<Guid, ProjectMemberDto>(_projectMemberCollection);
            }
        }
    }

    public static void AddUser(UserDto user) {
        lock (_lock) {
            _userCollection.Add(user.Id, user);
        }
    }

    public static bool TryGetUser(Guid id, out UserDto? user) {
        lock (_lock) {
            return _userCollection.TryGetValue(id, out user);
        }
    }

    public static UserDto? FindUserByUsername(string username) {
        lock (_lock) {
            return _userCollection.Values.FirstOrDefault(u => u.Username == username);
        }
    }

    public static bool UpdateUser(UserDto user) {
        lock (_lock) {
            if (_userCollection.ContainsKey(user.Id)) {
                _userCollection[user.Id] = user;
                return true;
            }
            return false;
        }
    }

    public static bool DeleteWorkspace(Guid workspaceId) {
        lock (_lock) {
            // Delete related data first
            var workspaceMembers = _workspaceMemberCollection.Values
                .Where(m => m.WorkspaceId == workspaceId)
                .ToList();

            foreach (var member in workspaceMembers) {
                _workspaceMemberCollection.Remove(member.Id);
            }

            var projects = _projectCollection.Values
                .Where(p => p.WorkspaceId == workspaceId)
                .ToList();

            foreach (var project in projects) {
                // Remove project members
                var projectMembers = _projectMemberCollection.Values
                    .Where(pm => pm.ProjectId == project.Id)
                    .ToList();

                foreach (var projectMember in projectMembers) {
                    _projectMemberCollection.Remove(projectMember.Id);
                }

                // Remove lists and tasks
                var lists = _listCollection.Values
                    .Where(l => l.ProjectId == project.Id)
                    .ToList();

                foreach (var list in lists) {
                    var tasks = _taskCollection.Values
                        .Where(t => t.ListId == list.Id)
                        .ToList();

                    foreach (var task in tasks) {
                        var comments = _commentCollection.Values
                            .Where(c => c.TaskId == task.Id)
                            .ToList();

                        foreach (var comment in comments) {
                            _commentCollection.Remove(comment.Id);
                        }

                        _taskCollection.Remove(task.Id);
                    }

                    _listCollection.Remove(list.Id);
                }

                _projectCollection.Remove(project.Id);
            }

            // Finally remove the workspace
            return _workspaceCollection.Remove(workspaceId);
        }
    }

    static TestData() {
        SeedTestData();
    }

    public static void ResetTestData() {
        lock (_lock) {
            _userCollection.Clear();
            _listCollection.Clear();
            _projectCollection.Clear();
            _taskCollection.Clear();
            _workspaceCollection.Clear();
            _commentCollection.Clear();
            _workspaceMemberCollection.Clear();
            _projectMemberCollection.Clear();
            SeedTestData();
        }
    }

    private static void SeedTestData() {
        lock (_lock) {
            // Create test users
            _userCollection.Add(User1Id, new UserDto(
                User1Id,
                "john.doe",
                "john@example.com",
                "John Doe"
            ));

            _userCollection.Add(User2Id, new UserDto(
                User2Id,
                "jane.smith",
                "jane@example.com",
                "Jane Smith"
            ));

            // Create test workspace
            var workspace = new WorkspaceDto {
                Id = WorkspaceId,
                Name = "Development Team",
                OwnerId = User1Id,
                CreatedDate = DateTime.UtcNow
            };
            _workspaceCollection.Add(WorkspaceId, workspace);

            // Add workspace members
            _workspaceMemberCollection.Add(WorkspaceMember1Id, new WorkspaceMemberDto {
                Id = WorkspaceMember1Id,
                WorkspaceId = WorkspaceId,
                UserId = User1Id,
                CreatedDate = DateTime.UtcNow
            });

            _workspaceMemberCollection.Add(WorkspaceMember2Id, new WorkspaceMemberDto {
                Id = WorkspaceMember2Id,
                WorkspaceId = WorkspaceId,
                UserId = User2Id,
                CreatedDate = DateTime.UtcNow
            });

            // Create projects
            var project2 = new ProjectDto {
                Id = Project2Id,
                WorkspaceId = WorkspaceId,
                Name = "Mobile App Development",
                Description = "Develop mobile application",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                Status = ProjectStatus.InProgress,
                CreatedDate = DateTime.UtcNow
            };
            _projectCollection.Add(Project2Id, project2);

            // Add project members
            _projectMemberCollection.Add(ProjectMember1Id, new ProjectMemberDto {
                Id = ProjectMember1Id,
                ProjectId = Project2Id,
                WorkspaceMemberId = WorkspaceMember1Id,
                CreatedDate = DateTime.UtcNow
            });

            _projectMemberCollection.Add(ProjectMember2Id, new ProjectMemberDto {
                Id = ProjectMember2Id,
                ProjectId = Project2Id,
                WorkspaceMemberId = WorkspaceMember2Id,
                CreatedDate = DateTime.UtcNow
            });

            var project = new ProjectDto {
                Id = Project1Id,
                WorkspaceId = WorkspaceId,
                Name = "Website Redesign",
                Description = "Redesign company website",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                Status = ProjectStatus.InProgress,
                CreatedDate = DateTime.UtcNow
            };
            _projectCollection.Add(Project1Id, project);

            // Create lists
            _listCollection.Add(BacklogId, new ListDto {
                Id = BacklogId,
                ProjectId = Project1Id,
                Name = "Backlog",
                Position = 0,
                CreatedDate = DateTime.UtcNow,
                Tasks = new List<TaskDto>()
            });

            _listCollection.Add(InProgressId, new ListDto {
                Id = InProgressId,
                ProjectId = Project1Id,
                Name = "In Progress",
                Position = 1,
                CreatedDate = DateTime.UtcNow,
                Tasks = new List<TaskDto>()
            });

            // Create tasks
            _taskCollection.Add(Task1Id, new TaskDto {
                Id = Task1Id,
                Name = "Design Homepage",
                Description = "Create new homepage design",
                Status = CurrentTaskStatus.NotStarted,
                Priority = Priority.High,
                Deadline = DateTime.UtcNow.AddDays(14),
                ProjectId = Project1Id,
                ListId = InProgressId,
                CreatedDate = DateTime.UtcNow
            });

            _taskCollection.Add(Task2Id, new TaskDto {
                Id = Task2Id,
                Name = "Backend API",
                Description = "Implement REST API",
                Status = CurrentTaskStatus.NotStarted,
                Priority = Priority.Medium,
                Deadline = DateTime.UtcNow.AddDays(30),
                ProjectId = Project1Id,
                ListId = BacklogId,
                CreatedDate = DateTime.UtcNow
            });

            // Add comment
            _commentCollection.Add(Comment1Id, new CommentDto {
                Id = Comment1Id,
                TaskId = Task1Id,
                UserId = User1Id,
                Content = "Initial design review needed",
                CreatedDate = DateTime.UtcNow
            });

            // Create Mobile Lists
            _listCollection.Add(MobileBacklogId, new ListDto {
                Id = MobileBacklogId,
                ProjectId = Project2Id,
                Name = "Mobile Backlog",
                Position = 0,
                CreatedDate = DateTime.UtcNow,
                Tasks = new List<TaskDto>()
            });

            _listCollection.Add(MobileInProgressId, new ListDto {
                Id = MobileInProgressId,
                ProjectId = Project2Id,
                Name = "Mobile In Progress",
                Position = 1,
                CreatedDate = DateTime.UtcNow,
                Tasks = new List<TaskDto>()
            });

            _listCollection.Add(MobileDoneId, new ListDto {
                Id = MobileDoneId,
                ProjectId = Project2Id,
                Name = "Mobile Done",
                Position = 2,
                CreatedDate = DateTime.UtcNow,
                Tasks = new List<TaskDto>()
            });

            // Add Mobile Tasks
            _taskCollection.Add(MobileTask1Id, new TaskDto {
                Id = MobileTask1Id,
                Name = "UI Design",
                Description = "Create mobile app UI mockups",
                Status = CurrentTaskStatus.InProgress,
                Priority = Priority.High,
                Deadline = DateTime.UtcNow.AddDays(7),
                ProjectId = Project2Id,
                ListId = MobileInProgressId,
                CreatedDate = DateTime.UtcNow,
                Assignees = new List<TaskAssigneeDto> {
                    new() {
                        Id = new Guid("88888888-9999-aaaa-bbbb-cccccccccccc"),
                        TaskId = MobileTask1Id,
                        UserId = User1Id
                    }
                }
            });

            _taskCollection.Add(MobileTask2Id, new TaskDto {
                Id = MobileTask2Id,
                Name = "API Integration",
                Description = "Implement REST API integration",
                Status = CurrentTaskStatus.NotStarted,
                Priority = Priority.Medium,
                Deadline = DateTime.UtcNow.AddDays(14),
                ProjectId = Project2Id,
                ListId = MobileBacklogId,
                CreatedDate = DateTime.UtcNow,
                Assignees = new List<TaskAssigneeDto> {
                    new() {
                        Id = new Guid("99999999-aaaa-bbbb-cccc-dddddddddddd"),
                        TaskId = MobileTask2Id,
                        UserId = User2Id
                    }
                }
            });

            _taskCollection.Add(MobileTask3Id, new TaskDto {
                Id = MobileTask3Id,
                Name = "Login Screen",
                Description = "Implement user authentication UI",
                Status = CurrentTaskStatus.Completed,
                Priority = Priority.Low,
                Deadline = DateTime.UtcNow.AddDays(-1),
                ProjectId = Project2Id,
                ListId = MobileDoneId,
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                Assignees = new List<TaskAssigneeDto> {
                    new() {
                        Id = new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                        TaskId = MobileTask3Id,
                        UserId = User2Id
                    }
                }
            });

            // Add mobile task comments
            _commentCollection.Add(Comment2Id, new CommentDto {
                Id = Comment2Id,
                TaskId = MobileTask1Id,
                UserId = User2Id,
                Content = "Should we use Material Design?",
                CreatedDate = DateTime.UtcNow.AddHours(-2)
            });

            _commentCollection.Add(Comment3Id, new CommentDto {
                Id = Comment3Id,
                TaskId = MobileTask1Id,
                UserId = User1Id,
                Content = "Yes, let's follow Material Design guidelines",
                CreatedDate = DateTime.UtcNow.AddHours(-1)
            });
        }
    }
}
