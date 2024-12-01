using Zello.Application.Dtos;
using Zello.Application.Exceptions;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.RepositoryInterfaces;

public class ProjectService : IProjectService {
    private readonly IProjectRepository _projectRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;

    public ProjectService(IProjectRepository projectRepository,
        IWorkspaceRepository workspaceRepository,
        IWorkspaceMemberRepository workspaceMemberRepository) {
        _projectRepository = projectRepository;
        _workspaceRepository = workspaceRepository;
        _workspaceMemberRepository = workspaceMemberRepository;
    }


    public async Task<ProjectReadDto> CreateProjectAsync(ProjectCreateDto projectDto, Guid userId) {
        var workspace = await _workspaceRepository.GetByIdAsync(projectDto.WorkspaceId);
        if (workspace == null)
            throw new InvalidOperationException("Invalid workspace ID");

        var workspaceMember =
            await _workspaceMemberRepository.GetWorkspaceMemberAsync(projectDto.WorkspaceId,
                userId);
        if (workspaceMember == null)
            throw new UnauthorizedAccessException("User is not a member of the workspace");

        var project = projectDto.ToEntity();
        var projectMember = new ProjectMember {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            WorkspaceMemberId = workspaceMember.Id,
            WorkspaceMember = workspaceMember, // Add this line
            AccessLevel = AccessLevel.Owner,
            CreatedDate = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);
        await _projectRepository.AddProjectMemberAsync(projectMember);
        await _projectRepository.SaveChangesAsync();

        var createdProject = await _projectRepository.GetProjectByIdWithDetailsAsync(project.Id);
        return ProjectReadDto.FromEntity(createdProject!);
    }


    public async Task<ProjectReadDto> GetProjectByIdAsync(Guid projectId) {
        var project = await _projectRepository.GetProjectByIdWithDetailsAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found");
        return ProjectReadDto.FromEntity(project);
    }

    public async Task<List<Project>> GetAllProjectsAsync(Guid? workspaceId) {
        return await _projectRepository.GetProjectsByWorkspaceAsync(workspaceId);
    }

    public async Task<ProjectReadDto> UpdateProjectAsync(Guid projectId,
        ProjectUpdateDto updatedProject) {
        var project = await _projectRepository.GetProjectByIdWithDetailsAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found");

        project = updatedProject.ToEntity(project);
        await _projectRepository.SaveChangesAsync();
        return ProjectReadDto.FromEntity(project);
    }

    public async Task DeleteProjectAsync(Guid projectId) {
        var project = await _projectRepository.GetProjectByIdWithDetailsAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found");

        _projectRepository.Remove(project);
        await _projectRepository.SaveChangesAsync();
    }

    public async Task<ProjectMember> AddProjectMemberAsync(ProjectMemberCreateDto createMember,
        Guid currentUserId) {
        var project = await _projectRepository.GetProjectWithMembersAsync(createMember.ProjectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {createMember.ProjectId} not found");

        var currentUserWorkspaceMember =
            await _workspaceMemberRepository.GetWorkspaceMemberAsync(project.WorkspaceId,
                currentUserId);
        if (currentUserWorkspaceMember == null)
            throw new UnauthorizedAccessException("Current user is not a member of the workspace");

        if (createMember.AccessLevel > currentUserWorkspaceMember.AccessLevel)
            throw new UnauthorizedAccessException(
                "Cannot assign access level higher than your own");

        var targetWorkspaceMember =
            await _workspaceMemberRepository.GetByIdAsync(createMember.WorkspaceMemberId);
        if (targetWorkspaceMember == null)
            throw new InvalidOperationException("Invalid workspace member ID");

        if (createMember.AccessLevel > targetWorkspaceMember.AccessLevel)
            throw new UnauthorizedAccessException(
                "Cannot assign project access level higher than user's workspace access level");

        if (targetWorkspaceMember.WorkspaceId != project.WorkspaceId)
            throw new InvalidOperationException(
                "Workspace member does not belong to the project's workspace");

        if (project.Members.Any(m => m.WorkspaceMemberId == createMember.WorkspaceMemberId))
            throw new InvalidOperationException("Member already exists in project");

        var projectMember = new ProjectMember {
            Id = Guid.NewGuid(),
            ProjectId = createMember.ProjectId,
            WorkspaceMemberId = createMember.WorkspaceMemberId,
            AccessLevel = createMember.AccessLevel,
            CreatedDate = DateTime.UtcNow
        };

        await _projectRepository.AddProjectMemberAsync(projectMember);
        await _projectRepository.SaveChangesAsync();
        return projectMember;
    }

    public async Task<ProjectMember> UpdateMemberAccessAsync(Guid memberId,
        AccessLevel newAccessLevel, Guid currentUserId, AccessLevel userAccess) {
        var projectMember = await _projectRepository.GetProjectMemberAsync(memberId, currentUserId);
        if (projectMember == null)
            throw new KeyNotFoundException("Project member not found");

        var project = await _projectRepository.GetProjectWithMembersAsync(projectMember.ProjectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        var currentProjectMember =
            project.Members.FirstOrDefault(pm => pm.WorkspaceMemberId == currentUserId);
        bool hasAccess = userAccess == AccessLevel.Admin ||
                         (currentProjectMember?.AccessLevel >= AccessLevel.Owner);

        if (!hasAccess)
            throw new UnauthorizedAccessException("Insufficient permissions");

        if (currentProjectMember != null && newAccessLevel > currentProjectMember.AccessLevel &&
            userAccess != AccessLevel.Admin)
            throw new UnauthorizedAccessException(
                "Cannot assign higher access level than your own");

        if (newAccessLevel > projectMember.WorkspaceMember.AccessLevel)
            throw new UnauthorizedAccessException("Cannot exceed workspace access level");

        projectMember.AccessLevel = newAccessLevel;
        await _projectRepository.SaveChangesAsync();
        return projectMember;
    }

    public async Task<TaskList> CreateListAsync(Guid projectId, ListCreateDto listDto) {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {projectId} not found");

        var maxPosition = await _projectRepository.GetMaxListPositionAsync(projectId);
        var list = new TaskList {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = listDto.Name,
            Position = maxPosition + 1,
            CreatedDate = DateTime.UtcNow,
            Tasks = new List<WorkTask>()
        };

        if (listDto.Tasks != null && listDto.Tasks.Any()) {
            foreach (var taskDto in listDto.Tasks) {
                // Set the ProjectId and ListId before creating the task
                taskDto.ProjectId = projectId;
                taskDto.ListId = list.Id;

                var task = taskDto.ToEntity();
                list.Tasks.Add(task);
            }
        }

        await _projectRepository.AddListAsync(list);
        await _projectRepository.SaveChangesAsync();
        return list;
    }


    public async Task<List<TaskList>> GetProjectListsAsync(Guid projectId) {
        if (!await _projectRepository.ExistsAsync(projectId))
            throw new KeyNotFoundException($"Project with ID {projectId} not found");

        return await _projectRepository.GetProjectListsAsync(projectId);
    }
}
