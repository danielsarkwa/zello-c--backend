// using Zello.Application.Dtos;
// using Zello.Domain.Entities;
// using Zello.Domain.Entities.Api.User;
// using Zello.Domain.Enums;
//
// namespace Zello.Infrastructure.TestingDataStorage;
//
// public static class TestData {
//     public static readonly object _lock = new();
//
//     // Fixed GUIDs for Users
//     private static readonly Guid User1Id = new("11111111-1111-1111-1111-111111111111");
//     private static readonly Guid User2Id = new("22222222-2222-2222-2222-222222222222");
//     private static readonly Guid WorkspaceId = new("33333333-3333-3333-3333-333333333333");
//     private static readonly Guid WorkspaceMember1Id = new("44444444-4444-4444-4444-444444444444");
//     private static readonly Guid WorkspaceMember2Id = new("55555555-5555-5555-5555-555555555555");
//     private static readonly Guid Project1Id = new("66666666-6666-6666-6666-666666666666");
//     private static readonly Guid Project2Id = new("77777777-7777-7777-7777-777777777777");
//     private static readonly Guid ProjectMember1Id = new("88888888-8888-8888-8888-888888888888");
//     private static readonly Guid ProjectMember2Id = new("99999999-9999-9999-9999-999999999999");
//     private static readonly Guid BacklogId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
//     private static readonly Guid InProgressId = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
//     private static readonly Guid MobileBacklogId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");
//     private static readonly Guid MobileInProgressId = new("dddddddd-dddd-dddd-dddd-dddddddddddd");
//     private static readonly Guid MobileDoneId = new("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
//     private static readonly Guid Task1Id = new("ffffffff-ffff-ffff-ffff-ffffffffffff");
//     private static readonly Guid Task2Id = new("11111111-2222-3333-4444-555555555555");
//     private static readonly Guid MobileTask1Id = new("22222222-3333-4444-5555-666666666666");
//     private static readonly Guid MobileTask2Id = new("33333333-4444-5555-6666-777777777777");
//     private static readonly Guid MobileTask3Id = new("44444444-5555-6666-7777-888888888888");
//     private static readonly Guid Comment1Id = new("55555555-6666-7777-8888-999999999999");
//     private static readonly Guid Comment2Id = new("66666666-7777-8888-9999-aaaaaaaaaaaa");
//     private static readonly Guid Comment3Id = new("77777777-8888-9999-aaaa-bbbbbbbbbbbb");
//
//     private static readonly Dictionary<Guid, User> _userCollection = new();
//     private static readonly Dictionary<Guid, TaskList> _listCollection = new();
//     private static readonly Dictionary<Guid, Project> _projectCollection = new();
//     private static readonly Dictionary<Guid, TaskReadDto> _taskCollection = new();
//     private static readonly Dictionary<Guid, Workspace> _workspaceCollection = new();
//     private static readonly Dictionary<Guid, Comment> _commentCollection = new();
//     private static readonly Dictionary<Guid, WorkspaceMember> _workspaceMemberCollection = new();
//     private static readonly Dictionary<Guid, ProjectMember> _projectMemberCollection = new();
//
//     // Public properties with thread-safe access
//     public static Dictionary<Guid, User> TestUserCollection {
//         get {
//             lock (_lock) {
//                 return _userCollection;
//             }
//         }
//     }
//
//     public static Dictionary<Guid, TaskList> TestListCollection {
//         get {
//             lock (_lock) {
//                 return _listCollection;
//             }
//         }
//     }
//
//     public static Dictionary<Guid, Project> TestProjectCollection {
//         get {
//             lock (_lock) {
//                 return _projectCollection;
//             }
//         }
//     }
//
//     public static Dictionary<Guid, TaskReadDto> TestTaskCollection {
//         get {
//             lock (_lock) {
//                 return _taskCollection;
//             }
//         }
//     }
//
//     public static Dictionary<Guid, Workspace> TestWorkspaceCollection {
//         get {
//             lock (_lock) {
//                 return _workspaceCollection;
//             }
//         }
//     }
//
//     public static Dictionary<Guid, Comment> TestCommentCollection {
//         get {
//             lock (_lock) {
//                 return _commentCollection;
//             }
//         }
//     }
//
//     public static Dictionary<Guid, WorkspaceMember> TestWorkspaceMemberCollection {
//         get {
//             lock (_lock) {
//                 return _workspaceMemberCollection;
//             }
//         }
//     }
//
//     public static Dictionary<Guid, ProjectMember> TestProjectMemberCollection {
//         get {
//             lock (_lock) {
//                 return _projectMemberCollection;
//             }
//         }
//     }
//
//     public static void AddWorkspace(Workspace workspace) {
//         lock (_lock) {
//             _workspaceCollection.Add(workspace.Id, workspace);
//         }
//     }
//
//     public static void AddProject(Project project) {
//         lock (_lock) {
//             _projectCollection.Add(project.Id, project);
//         }
//     }
//
//     public static void AddProjectMember(ProjectMember member) {
//         lock (_lock) {
//             _projectMemberCollection.Add(member.Id, member);
//         }
//     }
//
//     public static void AddWorkspaceMember(WorkspaceMember member) {
//         lock (_lock) {
//             _workspaceMemberCollection.Add(member.Id, member);
//         }
//     }
//
//     public static void AddList(TaskList list) {
//         lock (_lock) {
//             _listCollection.Add(list.Id, list);
//         }
//     }
//
//     public static void AddUser(User user) {
//         lock (_lock) {
//             _userCollection.Add(user.Id, user);
//         }
//     }
//
//     public static bool TryGetUser(Guid id, out User? user) {
//         lock (_lock) {
//             return _userCollection.TryGetValue(id, out user);
//         }
//     }
//
//     public static User? FindUserByUsername(string username) {
//         lock (_lock) {
//             return _userCollection.Values.FirstOrDefault(u => u.Username == username);
//         }
//     }
//
//     public static bool UpdateUser(User user) {
//         lock (_lock) {
//             if (_userCollection.ContainsKey(user.Id)) {
//                 _userCollection[user.Id] = user;
//                 return true;
//             }
//
//             return false;
//         }
//     }
//
//     public static bool DeleteWorkspace(Guid workspaceId) {
//         lock (_lock) {
//             // Delete related data first
//             var workspaceMembers = _workspaceMemberCollection.Values
//                 .Where(m => m.WorkspaceId == workspaceId)
//                 .ToList();
//
//             foreach (var member in workspaceMembers) {
//                 _workspaceMemberCollection.Remove(member.Id);
//             }
//
//             var projects = _projectCollection.Values
//                 .Where(p => p.WorkspaceId == workspaceId)
//                 .ToList();
//
//             foreach (var project in projects) {
//                 // Remove project members
//                 var projectMembers = _projectMemberCollection.Values
//                     .Where(pm => pm.ProjectId == project.Id)
//                     .ToList();
//
//                 foreach (var projectMember in projectMembers) {
//                     _projectMemberCollection.Remove(projectMember.Id);
//                 }
//
//                 // Remove lists and tasks
//                 var lists = _listCollection.Values
//                     .Where(l => l.ProjectId == project.Id)
//                     .ToList();
//
//                 foreach (var list in lists) {
//                     var tasks = _taskCollection.Values
//                         .Where(t => t.ListId == list.Id)
//                         .ToList();
//
//                     foreach (var task in tasks) {
//                         var comments = _commentCollection.Values
//                             .Where(c => c.TaskId == task.Id)
//                             .ToList();
//
//                         foreach (var comment in comments) {
//                             _commentCollection.Remove(comment.Id);
//                         }
//
//                         _taskCollection.Remove(task.Id);
//                     }
//
//                     _listCollection.Remove(list.Id);
//                 }
//
//                 _projectCollection.Remove(project.Id);
//             }
//
//             // Finally remove the workspace
//             return _workspaceCollection.Remove(workspaceId);
//         }
//     }
//
//     static TestData() {
//         SeedTestData();
//     }
//
//     public static void ResetTestData() {
//         lock (_lock) {
//             _userCollection.Clear();
//             _listCollection.Clear();
//             _projectCollection.Clear();
//             _taskCollection.Clear();
//             _workspaceCollection.Clear();
//             _commentCollection.Clear();
//             _workspaceMemberCollection.Clear();
//             _projectMemberCollection.Clear();
//             SeedTestData();
//         }
//     }
//
//     private static void SeedTestData() {
//         lock (_lock) {
//             // Create test users
//             var user1 = new User {
//                 Id = User1Id,
//                 Username = "john.doe",
//                 Email = "john@example.com",
//                 Name = "John Doe",
//                 PasswordHash = "hashed_password_1",
//                 AccessLevel = AccessLevel.Admin
//             };
//             _userCollection.Add(User1Id, user1);
//
//             var user2 = new User {
//                 Id = User2Id,
//                 Username = "jane.smith",
//                 Email = "jane@example.com",
//                 Name = "Jane Smith",
//                 PasswordHash = "hashed_password_2",
//                 AccessLevel = AccessLevel.Member
//             };
//             _userCollection.Add(User2Id, user2);
//
//             // Create test workspace
//             var workspace = new Workspace {
//                 Id = WorkspaceId,
//                 Name = "Development Team",
//                 OwnerId = User1Id,
//                 CreatedDate = DateTime.UtcNow
//             };
//             _workspaceCollection.Add(WorkspaceId, workspace);
//
//             // Add workspace members
//             _workspaceMemberCollection.Add(WorkspaceMember1Id, new WorkspaceMember {
//                 Id = WorkspaceMember1Id,
//                 WorkspaceId = WorkspaceId,
//                 UserId = User1Id,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             _workspaceMemberCollection.Add(WorkspaceMember2Id, new WorkspaceMember {
//                 Id = WorkspaceMember2Id,
//                 WorkspaceId = WorkspaceId,
//                 UserId = User2Id,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             // Create projects
//             var project2 = new Project {
//                 Id = Project2Id,
//                 WorkspaceId = WorkspaceId,
//                 Name = "Mobile App Development",
//                 Description = "Develop mobile application",
//                 StartDate = DateTime.UtcNow,
//                 EndDate = DateTime.UtcNow.AddMonths(6),
//                 Status = ProjectStatus.InProgress,
//                 CreatedDate = DateTime.UtcNow
//             };
//             _projectCollection.Add(Project2Id, project2);
//
//             // Add project members
//             _projectMemberCollection.Add(ProjectMember1Id, new ProjectMember {
//                 Id = ProjectMember1Id,
//                 ProjectId = Project2Id,
//                 WorkspaceMemberId = WorkspaceMember1Id,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             _projectMemberCollection.Add(ProjectMember2Id, new ProjectMember {
//                 Id = ProjectMember2Id,
//                 ProjectId = Project2Id,
//                 WorkspaceMemberId = WorkspaceMember2Id,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             var project1 = new Project {
//                 Id = Project1Id,
//                 WorkspaceId = WorkspaceId,
//                 Name = "Website Redesign",
//                 Description = "Redesign company website",
//                 StartDate = DateTime.UtcNow,
//                 EndDate = DateTime.UtcNow.AddMonths(3),
//                 Status = ProjectStatus.InProgress,
//                 CreatedDate = DateTime.UtcNow
//             };
//             _projectCollection.Add(Project1Id, project1);
//
//             // Create lists
//             _listCollection.Add(BacklogId, new TaskList {
//                 Id = BacklogId,
//                 ProjectId = Project1Id,
//                 Name = "Backlog",
//                 Position = 0,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             _listCollection.Add(InProgressId, new TaskList {
//                 Id = InProgressId,
//                 ProjectId = Project1Id,
//                 Name = "In Progress",
//                 Position = 1,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             // Create tasks
//             _taskCollection.Add(Task1Id, new TaskReadDto {
//                 Id = Task1Id,
//                 Name = "Design Homepage",
//                 Description = "Create new homepage design",
//                 Status = CurrentTaskStatus.NotStarted,
//                 Priority = Priority.High,
//                 Deadline = DateTime.UtcNow.AddDays(14),
//                 ProjectId = Project1Id,
//                 ListId = InProgressId,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             _taskCollection.Add(Task2Id, new TaskReadDto {
//                 Id = Task2Id,
//                 Name = "Backend API",
//                 Description = "Implement REST API",
//                 Status = CurrentTaskStatus.NotStarted,
//                 Priority = Priority.Medium,
//                 Deadline = DateTime.UtcNow.AddDays(30),
//                 ProjectId = Project1Id,
//                 ListId = BacklogId,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             // Add comment
//             _commentCollection.Add(Comment1Id, new Comment {
//                 Id = Comment1Id,
//                 TaskId = Task1Id,
//                 UserId = User1Id,
//                 Content = "Initial design review needed",
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             // Create Mobile Lists
//             _listCollection.Add(MobileBacklogId, new TaskList {
//                 Id = MobileBacklogId,
//                 ProjectId = Project2Id,
//                 Name = "Mobile Backlog",
//                 Position = 0,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             _listCollection.Add(MobileInProgressId, new TaskList {
//                 Id = MobileInProgressId,
//                 ProjectId = Project2Id,
//                 Name = "Mobile In Progress",
//                 Position = 1,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             _listCollection.Add(MobileDoneId, new TaskList {
//                 Id = MobileDoneId,
//                 ProjectId = Project2Id,
//                 Name = "Mobile Done",
//                 Position = 2,
//                 CreatedDate = DateTime.UtcNow
//             });
//
//             // Add Mobile Tasks
//             var mobileTask1 = new TaskReadDto {
//                 Id = MobileTask1Id,
//                 Name = "UI Design",
//                 Description = "Create mobile app UI mockups",
//                 Status = CurrentTaskStatus.InProgress,
//                 Priority = Priority.High,
//                 Deadline = DateTime.UtcNow.AddDays(7),
//                 ProjectId = Project2Id,
//                 ListId = MobileInProgressId,
//                 CreatedDate = DateTime.UtcNow,
//                 Assignees = new List<TaskAssigneeReadDto> {
//                     new() {
//                         Id = new Guid("88888888-9999-aaaa-bbbb-cccccccccccc"),
//                         TaskId = MobileTask1Id,
//                         UserId = User1Id,
//                         AssignedDate = DateTime.UtcNow
//                     }
//                 }
//             };
//             _taskCollection.Add(MobileTask1Id, mobileTask1);
//
//             var mobileTask2 = new TaskReadDto {
//                 Id = MobileTask2Id,
//                 Name = "API Integration",
//                 Description = "Implement REST API integration",
//                 Status = CurrentTaskStatus.NotStarted,
//                 Priority = Priority.Medium,
//                 Deadline = DateTime.UtcNow.AddDays(14),
//                 ProjectId = Project2Id,
//                 ListId = MobileBacklogId,
//                 CreatedDate = DateTime.UtcNow,
//                 Assignees = new List<TaskAssigneeReadDto> {
//                     new() {
//                         Id = new Guid("99999999-aaaa-bbbb-cccc-dddddddddddd"),
//                         TaskId = MobileTask2Id,
//                         UserId = User2Id,
//                         AssignedDate = DateTime.UtcNow
//                     }
//                 }
//             };
//             _taskCollection.Add(MobileTask2Id, mobileTask2);
//
//             var mobileTask3 = new TaskReadDto {
//                 Id = MobileTask3Id,
//                 Name = "Login Screen",
//                 Description = "Implement user authentication UI",
//                 Status = CurrentTaskStatus.Completed,
//                 Priority = Priority.Low,
//                 Deadline = DateTime.UtcNow.AddDays(-1),
//                 ProjectId = Project2Id,
//                 ListId = MobileDoneId,
//                 CreatedDate = DateTime.UtcNow.AddDays(-2),
//                 Assignees = new List<TaskAssigneeReadDto> {
//                     new() {
//                         Id = new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
//                         TaskId = MobileTask3Id,
//                         UserId = User2Id,
//                         AssignedDate = DateTime.UtcNow.AddDays(-2)
//                     }
//                 }
//             };
//             _taskCollection.Add(MobileTask3Id, mobileTask3);
//
//             // Add mobile task comments
//             _commentCollection.Add(Comment2Id, new Comment {
//                 Id = Comment2Id,
//                 TaskId = MobileTask1Id,
//                 UserId = User2Id,
//                 Content = "Should we use Material Design?",
//                 CreatedDate = DateTime.UtcNow.AddHours(-2)
//             });
//
//             _commentCollection.Add(Comment3Id, new Comment {
//                 Id = Comment3Id,
//                 TaskId = MobileTask1Id,
//                 UserId = User1Id,
//                 Content = "Yes, let's follow Material Design guidelines",
//                 CreatedDate = DateTime.UtcNow.AddHours(-1)
//             });
//         }
//     }
// }
