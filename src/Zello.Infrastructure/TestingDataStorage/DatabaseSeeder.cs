using Isopoh.Cryptography.Argon2;
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
            Username = "john",
            Email = "john@example.com",
            Name = "John Doe",
            PasswordHash = Argon2.Hash("john"),
            AccessLevel = AccessLevel.Owner
        };

        var user2 = new User {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            Username = "jane",
            Email = "jane@example.com",
            Name = "Jane Smith",
            PasswordHash = Argon2.Hash("jane"),
            AccessLevel = AccessLevel.Member
        };

        var admin = new User {
            Id = new Guid("11111112-1111-1111-1111-111111111111"),
            Username = "admin",
            Email = "admin",
            Name = "admin",
            PasswordHash = Argon2.Hash("admin"),
            AccessLevel = AccessLevel.Admin
        };

        var user3 = new User {
            Id = new Guid("33333333-3333-3333-3333-333333333334"),
            Username = "sarah",
            Email = "sarah@example.com",
            Name = "Sarah Johnson",
            PasswordHash = Argon2.Hash("sarah"),
            AccessLevel = AccessLevel.Member
        };

        var user4 = new User {
            Id = new Guid("44444444-4444-4444-4444-444444444445"),
            Username = "mike",
            Email = "mike@example.com",
            Name = "Mike Wilson",
            PasswordHash = Argon2.Hash("mike"),
            AccessLevel = AccessLevel.Member
        };

        await context.Users.AddRangeAsync(user1, user2, admin, user3, user4);
        await context.SaveChangesAsync();

        // Workspaces
        var workspaces = new[] {
            new Workspace {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Name = "Development Team",
                OwnerId = user1.Id,
                CreatedDate = DateTime.UtcNow
            },
            new Workspace {
                Id = new Guid("33333333-3333-3333-3333-333333333334"),
                Name = "Marketing Team",
                OwnerId = user2.Id,
                CreatedDate = DateTime.UtcNow
            },
            new Workspace {
                Id = new Guid("33333333-3333-3333-3333-333333333335"),
                Name = "Design Team",
                OwnerId = user3.Id,
                CreatedDate = DateTime.UtcNow
            },
            new Workspace {
                Id = new Guid("33333333-3333-3333-3333-333333333336"),
                Name = "Sales Team",
                OwnerId = user4.Id,
                CreatedDate = DateTime.UtcNow
            },
            new Workspace {
                Id = new Guid("33333333-3333-3333-3333-333333333337"),
                Name = "Product Team",
                OwnerId = user1.Id,
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.Workspaces.AddRangeAsync(workspaces);
        await context.SaveChangesAsync();

        // Workspace Members
        var workspaceMembers = new[] {
            new WorkspaceMember {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                WorkspaceId = workspaces[0].Id,
                UserId = user1.Id,
                CreatedDate = DateTime.UtcNow
            },
            new WorkspaceMember {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                WorkspaceId = workspaces[0].Id,
                UserId = user2.Id,
                CreatedDate = DateTime.UtcNow
            },
            // Additional members
            new WorkspaceMember {
                Id = new Guid("55555555-5555-5555-5555-555555555556"),
                WorkspaceId = workspaces[1].Id,
                UserId = user3.Id,
                CreatedDate = DateTime.UtcNow
            },
            new WorkspaceMember {
                Id = new Guid("55555555-5555-5555-5555-555555555557"),
                WorkspaceId = workspaces[2].Id,
                UserId = user4.Id,
                CreatedDate = DateTime.UtcNow
            },
            new WorkspaceMember {
                Id = new Guid("55555555-5555-5555-5555-555555555558"),
                WorkspaceId = workspaces[3].Id,
                UserId = user1.Id,
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.WorkspaceMembers.AddRangeAsync(workspaceMembers);
        await context.SaveChangesAsync();

        // Projects
        var projects = new[] {
            // Existing projects
            new Project {
                Id = new Guid("66666666-6666-6666-6666-666666666666"),
                WorkspaceId = workspaces[0].Id,
                Name = "Website Redesign",
                Description = "Redesign company website",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                Status = ProjectStatus.InProgress,
                CreatedDate = DateTime.UtcNow
            },
            new Project {
                Id = new Guid("77777777-7777-7777-7777-777777777777"),
                WorkspaceId = workspaces[0].Id,
                Name = "Mobile App Development",
                Description = "Develop mobile application",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                Status = ProjectStatus.InProgress,
                CreatedDate = DateTime.UtcNow
            },
            // Additional projects
            new Project {
                Id = new Guid("77777777-7777-7777-7777-777777777778"),
                WorkspaceId = workspaces[1].Id,
                Name = "Marketing Campaign",
                Description = "Q4 Marketing Campaign",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(4),
                Status = ProjectStatus.NotStarted,
                CreatedDate = DateTime.UtcNow
            },
            new Project {
                Id = new Guid("77777777-7777-7777-7777-777777777779"),
                WorkspaceId = workspaces[2].Id,
                Name = "Brand Refresh",
                Description = "Company brand refresh",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                Status = ProjectStatus.InProgress,
                CreatedDate = DateTime.UtcNow
            },
            new Project {
                Id = new Guid("77777777-7777-7777-7777-777777777780"),
                WorkspaceId = workspaces[3].Id,
                Name = "Sales Strategy",
                Description = "New sales strategy implementation",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                Status = ProjectStatus.NotStarted,
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        // Project Members
        var projectMembers = new[] {
            // Existing members
            new ProjectMember {
                Id = new Guid("88888888-8888-8888-8888-888888888888"),
                ProjectId = projects[1].Id,
                WorkspaceMemberId = workspaceMembers[0].Id,
                CreatedDate = DateTime.UtcNow
            },
            new ProjectMember {
                Id = new Guid("99999999-9999-9999-9999-999999999999"),
                ProjectId = projects[1].Id,
                WorkspaceMemberId = workspaceMembers[1].Id,
                CreatedDate = DateTime.UtcNow
            },
            // Additional members
            new ProjectMember {
                Id = new Guid("99999999-9999-9999-9999-999999999990"),
                ProjectId = projects[2].Id,
                WorkspaceMemberId = workspaceMembers[2].Id,
                CreatedDate = DateTime.UtcNow
            },
            new ProjectMember {
                Id = new Guid("99999999-9999-9999-9999-999999999991"),
                ProjectId = projects[3].Id,
                WorkspaceMemberId = workspaceMembers[3].Id,
                CreatedDate = DateTime.UtcNow
            },
            new ProjectMember {
                Id = new Guid("99999999-9999-9999-9999-999999999992"),
                ProjectId = projects[4].Id,
                WorkspaceMemberId = workspaceMembers[4].Id,
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.ProjectMembers.AddRangeAsync(projectMembers);
        await context.SaveChangesAsync();

        // Task Lists (already meets minimum requirement)
        var lists = new[] {
            new TaskList {
                Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProjectId = projects[0].Id,
                Name = "Backlog",
                Position = 0,
                CreatedDate = DateTime.UtcNow
            },
            new TaskList {
                Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                ProjectId = projects[0].Id,
                Name = "In Progress",
                Position = 1,
                CreatedDate = DateTime.UtcNow
            },
            new TaskList {
                Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                ProjectId = projects[1].Id,
                Name = "Mobile Backlog",
                Position = 0,
                CreatedDate = DateTime.UtcNow
            },
            new TaskList {
                Id = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                ProjectId = projects[1].Id,
                Name = "Mobile In Progress",
                Position = 1,
                CreatedDate = DateTime.UtcNow
            },
            new TaskList {
                Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                ProjectId = projects[1].Id,
                Name = "Mobile Done",
                Position = 2,
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.Lists.AddRangeAsync(lists);
        await context.SaveChangesAsync();

        // Tasks (expanded to 20 records)
        var tasks = new List<WorkTask> {
            // Existing tasks
            new WorkTask {
                Id = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Name = "Design Homepage",
                Description = "Create new homepage design",
                Status = CurrentTaskStatus.NotStarted,
                Priority = Priority.High,
                Deadline = DateTime.UtcNow.AddDays(14),
                ProjectId = projects[0].Id,
                ListId = lists[1].Id,
                CreatedDate = DateTime.UtcNow
            },
            new WorkTask {
                Id = new Guid("11111111-2222-3333-4444-555555555555"),
                Name = "Backend API",
                Description = "Implement REST API",
                Status = CurrentTaskStatus.NotStarted,
                Priority = Priority.Medium,
                Deadline = DateTime.UtcNow.AddDays(30),
                ProjectId = projects[0].Id,
                ListId = lists[0].Id,
                CreatedDate = DateTime.UtcNow
            },
            new WorkTask {
                Id = new Guid("22222222-3333-4444-5555-666666666666"),
                Name = "UI Design",
                Description = "Create mobile app UI mockups",
                Status = CurrentTaskStatus.InProgress,
                Priority = Priority.High,
                Deadline = DateTime.UtcNow.AddDays(7),
                ProjectId = projects[1].Id,
                ListId = lists[3].Id,
                CreatedDate = DateTime.UtcNow
            }
        };

        // Additional tasks to reach 20
        var taskNames = new[] {
            "Database Schema Design", "User Authentication", "Payment Integration",
            "Email Notifications", "Search Functionality", "Analytics Dashboard",
            "Performance Optimization", "Security Audit", "Documentation",
            "Unit Testing", "Integration Testing", "Deployment Pipeline",
            "Error Handling", "Logging System", "Mobile Responsiveness",
            "API Documentation", "Code Review"
        };

        for (int i = 0; i < taskNames.Length; i++) {
            var task = new WorkTask {
                Id = Guid.NewGuid(),
                Name = taskNames[i],
                Description = $"Task description for {taskNames[i]}",
                Status = i % 2 == 0 ? CurrentTaskStatus.NotStarted : CurrentTaskStatus.InProgress,
                Priority = i % 3 == 0
                    ? Priority.High
                    : (i % 3 == 1 ? Priority.Medium : Priority.Low),
                Deadline = DateTime.UtcNow.AddDays(7 + i),
                ProjectId = projects[i % projects.Length].Id,
                ListId = lists[i % lists.Length].Id,
                CreatedDate = DateTime.UtcNow
            };
            tasks.Add(task);
        }

        await context.Tasks.AddRangeAsync(tasks);
        await context.SaveChangesAsync();

        // Comments (expanded to 5 records)
        var comments = new[] {
                // Existing comments
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
                },
                // Additional comments
                new Comment {
                    Id = new Guid("88888888-8888-9999-aaaa-bbbbbbbbbbbb"),
                    TaskId = tasks[3].Id,
                    UserId = user3.Id,
                    Content = "We should consider accessibility requirements",
                    CreatedDate = DateTime.UtcNow.AddHours(-3)
                },
                new Comment {
                    Id = new Guid("99999999-8888-9999-aaaa-bbbbbbbbbbbb"),
                    TaskId = tasks[4].Id,
                    UserId = user4.Id,
                    Content = "Adding performance monitoring metrics",
                    CreatedDate = DateTime.UtcNow.AddHours(-4)
                }
            };

        await context.Comments.AddRangeAsync(comments);
        await context.SaveChangesAsync();

        // Task Assignees (expanded to 5 records)
        var taskAssignees = new[] {
                // Existing assignee
                new TaskAssignee {
                    Id = new Guid("88888888-9999-aaaa-bbbb-cccccccccccc"),
                    TaskId = tasks[2].Id,
                    UserId = user1.Id,
                    AssignedDate = DateTime.UtcNow
                },
                // Additional assignees
                new TaskAssignee {
                    Id = new Guid("99999999-9999-aaaa-bbbb-cccccccccccc"),
                    TaskId = tasks[0].Id,
                    UserId = user2.Id,
                    AssignedDate = DateTime.UtcNow
                },
                new TaskAssignee {
                    Id = new Guid("aaaaaaaa-9999-aaaa-bbbb-cccccccccccc"),
                    TaskId = tasks[1].Id,
                    UserId = user3.Id,
                    AssignedDate = DateTime.UtcNow
                },
                new TaskAssignee {
                    Id = new Guid("bbbbbbbb-9999-aaaa-bbbb-cccccccccccc"),
                    TaskId = tasks[3].Id,
                    UserId = user4.Id,
                    AssignedDate = DateTime.UtcNow
                },
                new TaskAssignee {
                    Id = new Guid("cccccccc-9999-aaaa-bbbb-cccccccccccc"),
                    TaskId = tasks[4].Id,
                    UserId = user1.Id,
                    AssignedDate = DateTime.UtcNow
                }
            };

        await context.TaskAssignees.AddRangeAsync(taskAssignees);
        await context.SaveChangesAsync();

        Console.WriteLine("Database seeded successfully!");
    }
}
