using Zello.Application.Dtos;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.RepositoryInterfaces;

public class WorkTaskService : IWorkTaskService {
    private readonly IWorkTaskRepository _workTaskRepository;
    private readonly ITaskListRepository _taskListRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    private readonly ITaskAssigneeRepository _taskAssigneeRepository;
    private readonly ICommentService _commentService;
    private readonly IUserService _userService;
    private readonly IProjectRepository _projectRepository;

    public WorkTaskService(IWorkTaskRepository workTaskRepository,
        ITaskListRepository taskListRepository,
        IWorkspaceMemberRepository workspaceMemberRepository,
        ITaskAssigneeRepository taskAssigneeRepository,
        IProjectRepository projectRepository,
        ICommentService commentService, IUserService userService) {
        _workTaskRepository = workTaskRepository;
        _taskListRepository = taskListRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
        _taskAssigneeRepository = taskAssigneeRepository;
        _commentService = commentService;
        _userService = userService;
        _workTaskRepository = workTaskRepository;
        _projectRepository = projectRepository;
    }


    public async Task<TaskReadDto> GetTaskByIdAsync(Guid taskId, Guid userId,
        AccessLevel userAccess) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        await EnsureProjectAccessAsync(task.ProjectId, userId, userAccess);
        return TaskReadDto.FromEntity(task);
    }

    public async Task<IEnumerable<TaskReadDto>> GetAllTasksAsync(Guid userId,
        AccessLevel userAccess) {
        var tasks = await _workTaskRepository.GetAllTasksAsync();
        if (userAccess != AccessLevel.Admin) {
            tasks = tasks.Where(t => t.List.Project.Members
                .Any(pm => pm.WorkspaceMember.UserId == userId));
        }

        return tasks.Select(TaskReadDto.FromEntity);
    }

    public async Task<TaskReadDto> UpdateTaskAsync(Guid taskId, TaskUpdateDto updateDto,
        Guid userId, AccessLevel userAccess) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        await EnsureProjectAccessAsync(task.ProjectId, userId, userAccess, AccessLevel.Member);

        updateDto.UpdateEntity(task);
        await _workTaskRepository.UpdateTaskAsync(task);
        return TaskReadDto.FromEntity(task);
    }

    public async Task DeleteTaskAsync(Guid taskId, Guid userId, AccessLevel userAccess) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        await EnsureProjectAccessAsync(task.ProjectId, userId, userAccess, AccessLevel.Member);
        await _workTaskRepository.DeleteTaskAsync(taskId);
    }

    public async Task<TaskReadDto> MoveTaskAsync(Guid taskId, Guid targetListId, Guid userId,
        AccessLevel userAccess) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        await EnsureProjectAccessAsync(task.ProjectId, userId, userAccess, AccessLevel.Member);

        var project = await _projectRepository.GetByIdAsync(task.ProjectId);
        if (project == null || !project.Lists.Any(l => l.Id == targetListId)) {
            throw new InvalidOperationException(
                "Cannot move task to a list in a different project");
        }

        await _workTaskRepository.MoveTaskAsync(task, targetListId);
        return TaskReadDto.FromEntity(task);
    }

    public async Task<TaskAssigneeReadDto> AssignUserToTaskAsync(Guid taskId, Guid userToAssignId,
        Guid userId, AccessLevel userAccess) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        await EnsureProjectAccessAsync(task.ProjectId, userId, userAccess, AccessLevel.Member);

        if (task.Assignees.Any(a => a.UserId == userToAssignId)) {
            throw new InvalidOperationException("User is already assigned to this task");
        }

        var project = await _projectRepository.GetByIdAsync(task.ProjectId);
        if (project == null) {
            throw new KeyNotFoundException("Project not found");
        }

        if (project.Members?.Any(m => m.WorkspaceMember?.UserId == userToAssignId) != true &&
            userAccess != AccessLevel.Admin) {
            throw new InvalidOperationException(
                "User must be a member of the project to be assigned");
        }

        var assignee = new TaskAssignee {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = userToAssignId,
            AssignedDate = DateTime.UtcNow
        };

        task.Assignees.Add(assignee);
        await _workTaskRepository.UpdateTaskAsync(task);

        return TaskAssigneeReadDto.FromEntity(assignee);
    }

    public async Task RemoveTaskAssigneeAsync(Guid taskId, Guid userToRemoveId, Guid userId,
        AccessLevel userAccess) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);

        // Allow users to unassign themselves, otherwise require Member access
        if (userId != userToRemoveId) {
            await EnsureProjectAccessAsync(task.ProjectId, userId, userAccess, AccessLevel.Member);
        }

        var assignee = task.Assignees.FirstOrDefault(a => a.UserId == userToRemoveId)
                       ?? throw new KeyNotFoundException("User is not assigned to this task");

        task.Assignees.Remove(assignee);
        await _workTaskRepository.UpdateTaskAsync(task);
    }

    public async Task<IEnumerable<TaskAssigneeReadDto>> GetTaskAssigneesAsync(Guid taskId,
        Guid userId, AccessLevel userAccess) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        await EnsureProjectAccessAsync(task.ProjectId, userId, userAccess);
        return task.Assignees.Select(TaskAssigneeReadDto.FromEntity);
    }

    public async Task<IEnumerable<CommentReadDto>> GetTaskCommentsAsync(Guid taskId, Guid userId,
        AccessLevel userAccess) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        await EnsureProjectAccessAsync(task.ProjectId, userId, userAccess);
        return task.Comments.Select(CommentReadDto.FromEntity);
    }

    public async Task<CommentReadDto> AddTaskCommentAsync(Guid taskId, string content, Guid userId,
        AccessLevel userAccess) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        await EnsureProjectAccessAsync(task.ProjectId, userId, userAccess);

        var comment = new Comment {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = userId,
            Content = content,
            CreatedDate = DateTime.UtcNow
        };

        task.Comments.Add(comment);
        await _workTaskRepository.UpdateTaskAsync(task);

        return CommentReadDto.FromEntity(comment);
    }

    private async Task EnsureProjectAccessAsync(Guid projectId, Guid userId, AccessLevel userAccess,
        AccessLevel requiredAccess = AccessLevel.Guest) {
        var project = await _projectRepository.GetProjectByIdWithDetailsAsync(projectId);
        if (project == null) {
            throw new KeyNotFoundException("Project not found");
        }

        var projectMember = project.Members.FirstOrDefault(m => m.WorkspaceMember.UserId == userId);
        if (userAccess != AccessLevel.Admin &&
            (projectMember == null || projectMember.AccessLevel < requiredAccess)) {
            throw new UnauthorizedAccessException(
                $"User does not have required access level: {requiredAccess}");
        }
    }

    public async Task<TaskReadDto> GetTaskByIdAsync(Guid taskId) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        if (task == null)
            throw new Exception($"Task with ID {taskId} not found");

        return MapToTaskReadDto(task);
    }

    public async Task<IEnumerable<TaskReadDto>> GetAllTasksAsync() {
        var tasks = await _workTaskRepository.GetAllTasksAsync();
        return tasks.Select(MapToTaskReadDto);
    }

    public async Task<TaskReadDto> AddTaskAsync(TaskCreateDto taskCreateDto) {
        var task = new WorkTask {
            Id = Guid.NewGuid(),
            Name = taskCreateDto.Name,
            Description = taskCreateDto.Description ?? "",
            Status = taskCreateDto.Status,
            Priority = taskCreateDto.Priority,
            Deadline = taskCreateDto.Deadline,
            ListId = taskCreateDto.ListId,
            ProjectId = taskCreateDto.ProjectId,
            CreatedDate = DateTime.UtcNow
        };

        await _workTaskRepository.AddTaskAsync(task);
        return MapToTaskReadDto(task);
    }

    public async Task<TaskReadDto> UpdateTaskAsync(Guid taskId, TaskCreateDto taskUpdateDto) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        if (task == null)
            throw new Exception($"Task with ID {taskId} not found");

        task.Name = taskUpdateDto.Name;
        task.Description = taskUpdateDto.Description ?? task.Description;
        task.Status = taskUpdateDto.Status;
        task.Priority = taskUpdateDto.Priority;
        task.Deadline = taskUpdateDto.Deadline;
        task.ListId = taskUpdateDto.ListId;
        task.ProjectId = taskUpdateDto.ProjectId;

        await _workTaskRepository.UpdateTaskAsync(task);
        return MapToTaskReadDto(task);
    }

    public async Task DeleteTaskAsync(Guid taskId) {
        await _workTaskRepository.DeleteTaskAsync(taskId);
    }

    public async Task<TaskReadDto> MoveTaskAsync(Guid taskId, Guid targetListId) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        if (task == null)
            throw new Exception($"Task with ID {taskId} not found");

        var targetList = await _taskListRepository.GetByIdWithRelationsAsync(targetListId);
        if (targetList == null)
            throw new Exception($"Target list with ID {targetListId} not found");

        if (task.ProjectId != targetList.ProjectId)
            throw new Exception("Cannot move task to a list in a different project");

        await _workTaskRepository.MoveTaskAsync(task, targetListId);
        return MapToTaskReadDto(task);
    }

    public async Task RemoveTaskAssigneeAsync(Guid taskId, Guid userId, Guid requestingUserId) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        if (task == null)
            throw new Exception($"Task with ID {taskId} not found");

        var assignee = await _taskAssigneeRepository.GetTaskAssigneeAsync(taskId, userId);
        if (assignee == null)
            throw new Exception($"User {userId} is not assigned to task {taskId}");

        var workspaceMember =
            await _workspaceMemberRepository.GetWorkspaceMemberAsync(task.Project.WorkspaceId,
                requestingUserId);
        if (workspaceMember == null)
            throw new Exception("User must be a member of the workspace");

        await _taskAssigneeRepository.DeleteAsync(assignee);
    }

    public async Task<TaskAssigneeReadDto> AssignUserToTaskAsync(Guid taskId, Guid userId,
        Guid requestingUserId) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        if (task == null)
            throw new Exception($"Task with ID {taskId} not found");

        var userToAssign = await _userService.GetUserByIdAsync(userId);
        if (userToAssign == null)
            throw new Exception($"User with ID {userId} not found");

        var workspaceMembers =
            await _workspaceMemberRepository.GetMembersByWorkspaceIdAsync(task.Project.WorkspaceId);

        var requestingUserMember =
            workspaceMembers.FirstOrDefault(m => m.UserId == requestingUserId);
        var assignedUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

        if (requestingUserMember == null || assignedUserMember == null)
            throw new Exception("Both users must be members of the workspace");

        if (task.Assignees.Any(a => a.UserId == userId))
            throw new Exception("User is already assigned to this task");

        var taskAssignee = new TaskAssignee {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = userId,
            AssignedDate = DateTime.UtcNow
        };

        var createdAssignee = await _taskAssigneeRepository.AddAssigneeAsync(taskAssignee);

        return TaskAssigneeReadDto.FromEntity(createdAssignee);
    }

    public async Task<CommentReadDto> AddTaskCommentAsync(Guid taskId, Guid userId,
        CommentCreateDto commentContent) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        if (task == null)
            throw new Exception($"Task with ID {taskId} not found");

        var workspaceMember =
            await _workspaceMemberRepository.GetWorkspaceMemberAsync(task.Project.WorkspaceId,
                userId);
        if (workspaceMember == null)
            throw new Exception("User must be a member of the workspace");

        // Delegate comment creation to CommentService
        return await _commentService.CreateCommentAsync(commentContent, userId);
    }

    private TaskReadDto MapToTaskReadDto(WorkTask task) {
        return new TaskReadDto {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            Deadline = task.Deadline,
            CreatedDate = task.CreatedDate,
            ListId = task.ListId,
            ProjectId = task.ProjectId,
            Assignees = task.Assignees.Select(a => new TaskAssigneeReadDto {
                Id = a.Id,
                TaskId = a.TaskId,
                UserId = a.UserId,
                AssignedDate = a.AssignedDate,
                User = new UserReadDto {
                    Id = a.User.Id,
                    Name = a.User.Name,
                    Email = a.User.Email,
                    CreatedDate = a.User.CreatedDate
                }
            }).ToList(),
            Comments = task.Comments.Select(c => new CommentReadDto {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                Content = c.Content,
                CreatedDate = c.CreatedDate
            }).ToList(),
            List = task.List != null
                ? new ListReadDto {
                    Id = task.List.Id,
                    Name = task.List.Name,
                    ProjectId = task.List.ProjectId,
                    CreatedDate = task.List.CreatedDate
                }
                : null,
            Project = task.Project != null
                ? new ProjectReadDto {
                    Id = task.Project.Id,
                    Name = task.Project.Name,
                    WorkspaceId = task.Project.WorkspaceId,
                    CreatedDate = task.Project.CreatedDate
                }
                : null
        };
    }
}
