using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Enums;
using Zello.Infrastructure.Data;

namespace Zello.Infrastructure.TestingDataStorage;

public static class DatabaseSeeder {
    public static async Task SeedDatabase(ApplicationDbContext context) {
        if (context.Users.Any()) {
            Console.WriteLine("Database already contains data - skipping seed.");
            return;
        }

        Console.WriteLine("Database is empty. Seeding initial data...");

        // Users
        var user1 = new User {
            Id = new Guid("11111111-1111-1111-1111-111111111111"),
            Username = "john.doe",
            Email = "john@example.com",
            Name = "John Doe",
            PasswordHash = "hashed_password_1",
            AccessLevel = AccessLevel.Admin
        };

        var user2 = new User {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            Username = "jane.smith",
            Email = "jane@example.com",
            Name = "Jane Smith",
            PasswordHash = "hashed_password_2",
            AccessLevel = AccessLevel.Member
        };

        await context.Users.AddRangeAsync(user1, user2);
        await context.SaveChangesAsync();

        // Workspace
        var workspace = new Workspace {
            Id = new Guid("33333333-3333-3333-3333-333333333333"),
            Name = "Development Team",
            OwnerId = user1.Id,
            CreatedDate = DateTime.UtcNow
        };

        await context.Workspaces.AddAsync(workspace);
        await context.SaveChangesAsync();

        // Workspace Members
        var workspaceMembers = new[] {
            new WorkspaceMember {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                WorkspaceId = workspace.Id,
                UserId = user1.Id,
                CreatedDate = DateTime.UtcNow
            },
            new WorkspaceMember {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                WorkspaceId = workspace.Id,
                UserId = user2.Id,
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.WorkspaceMembers.AddRangeAsync(workspaceMembers);
        await context.SaveChangesAsync();

        // Projects
        var project1 = new Project {
            Id = new Guid("66666666-6666-6666-6666-666666666666"),
            WorkspaceId = workspace.Id,
            Name = "Website Redesign",
            Description = "Redesign company website",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(3),
            Status = ProjectStatus.InProgress,
            CreatedDate = DateTime.UtcNow
        };

        var project2 = new Project {
            Id = new Guid("77777777-7777-7777-7777-777777777777"),
            WorkspaceId = workspace.Id,
            Name = "Mobile App Development",
            Description = "Develop mobile application",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            Status = ProjectStatus.InProgress,
            CreatedDate = DateTime.UtcNow
        };

        await context.Projects.AddRangeAsync(project1, project2);
        await context.SaveChangesAsync();

        // Project Members
        var projectMembers = new[] {
            new ProjectMember {
                Id = new Guid("88888888-8888-8888-8888-888888888888"),
                ProjectId = project2.Id,
                WorkspaceMemberId = workspaceMembers[0].Id,
                CreatedDate = DateTime.UtcNow
            },
            new ProjectMember {
                Id = new Guid("99999999-9999-9999-9999-999999999999"),
                ProjectId = project2.Id,
                WorkspaceMemberId = workspaceMembers[1].Id,
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.ProjectMembers.AddRangeAsync(projectMembers);
        await context.SaveChangesAsync();

        // Task Lists
        var lists = new[] {
            new TaskList {
                Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProjectId = project1.Id,
                Name = "Backlog",
                Position = 0,
                CreatedDate = DateTime.UtcNow
            },
            new TaskList {
                Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                ProjectId = project1.Id,
                Name = "In Progress",
                Position = 1,
                CreatedDate = DateTime.UtcNow
            },
            // Mobile project lists
            new TaskList {
                Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                ProjectId = project2.Id,
                Name = "Mobile Backlog",
                Position = 0,
                CreatedDate = DateTime.UtcNow
            },
            new TaskList {
                Id = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                ProjectId = project2.Id,
                Name = "Mobile In Progress",
                Position = 1,
                CreatedDate = DateTime.UtcNow
            },
            new TaskList {
                Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                ProjectId = project2.Id,
                Name = "Mobile Done",
                Position = 2,
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.Lists.AddRangeAsync(lists);
        await context.SaveChangesAsync();

        // Tasks
        var tasks = new[] {
            new WorkTask {
                Id = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Name = "Design Homepage",
                Description = "Create new homepage design",
                Status = CurrentTaskStatus.NotStarted,
                Priority = Priority.High,
                Deadline = DateTime.UtcNow.AddDays(14),
                ProjectId = project1.Id,
                ListId = lists[1].Id, // In Progress
                CreatedDate = DateTime.UtcNow
            },
            new WorkTask {
                Id = new Guid("11111111-2222-3333-4444-555555555555"),
                Name = "Backend API",
                Description = "Implement REST API",
                Status = CurrentTaskStatus.NotStarted,
                Priority = Priority.Medium,
                Deadline = DateTime.UtcNow.AddDays(30),
                ProjectId = project1.Id,
                ListId = lists[0].Id, // Backlog
                CreatedDate = DateTime.UtcNow
            },
            // Mobile Tasks
            new WorkTask {
                Id = new Guid("22222222-3333-4444-5555-666666666666"),
                Name = "UI Design",
                Description = "Create mobile app UI mockups",
                Status = CurrentTaskStatus.InProgress,
                Priority = Priority.High,
                Deadline = DateTime.UtcNow.AddDays(7),
                ProjectId = project2.Id,
                ListId = lists[3].Id, // Mobile In Progress
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.Tasks.AddRangeAsync(tasks);
        await context.SaveChangesAsync();

        // Comments
        var comments = new[] {
            new Comment {
                Id = new Guid("55555555-6666-7777-8888-999999999999"),
                TaskId = tasks[0].Id,
                UserId = user1.Id,
                Content = "Initial design review needed",
                CreatedDate = DateTime.UtcNow
            },
            new Comment {
                Id = new Guid("66666666-7777-8888-9999-aaaaaaaaaaaa"),
                TaskId = tasks[2].Id,
                UserId = user2.Id,
                Content = "Should we use Material Design?",
                CreatedDate = DateTime.UtcNow.AddHours(-2)
            },
            new Comment {
                Id = new Guid("77777777-8888-9999-aaaa-bbbbbbbbbbbb"),
                TaskId = tasks[2].Id,
                UserId = user1.Id,
                Content = "Yes, let's follow Material Design guidelines",
                CreatedDate = DateTime.UtcNow.AddHours(-1)
            }
        };

        await context.Comments.AddRangeAsync(comments);
        await context.SaveChangesAsync();

        // Task Assignees
        var taskAssignees = new[] {
            new TaskAssignee {
                Id = new Guid("88888888-9999-aaaa-bbbb-cccccccccccc"),
                TaskId = tasks[2].Id,
                UserId = user1.Id,
                AssignedDate = DateTime.UtcNow
            }
        };

        await context.TaskAssignees.AddRangeAsync(taskAssignees);
        await context.SaveChangesAsync();

        Console.WriteLine("Database seeded successfully!");
    }
}
