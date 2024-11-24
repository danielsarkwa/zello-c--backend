using Microsoft.AspNetCore.Mvc;
using Zello.Application.Features.Projects;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.TestingDataStorage;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class ProjectController : ControllerBase {
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateProject([FromBody] ProjectDto project) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        // Validate workspace exists
        if (!TestData.TestWorkspaceCollection.ContainsKey(project.WorkspaceId)) {
            return BadRequest("Invalid workspace ID");
        }

        project.Id = Guid.NewGuid();
        project.CreatedDate = DateTime.UtcNow;

        TestData.TestProjectCollection.Add(project.Id, project);

        return CreatedAtAction(
            nameof(GetProjectById),
            new { projectId = project.Id },
            project
        );
    }

    [HttpGet("{projectId}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetProjectById(Guid projectId) {
        if (!TestData.TestProjectCollection.TryGetValue(projectId, out var project)) {
            return NotFound($"Project with ID {projectId} not found");
        }

        return Ok(project);
    }

    [HttpGet]
    public IActionResult GetAllProjects([FromQuery] Guid? workspaceId = null) {
        var projects = TestData.TestProjectCollection.Values
            .Where(p => !workspaceId.HasValue || p.WorkspaceId == workspaceId.Value)
            .ToList();

        foreach (var project in projects) {
            // Populate Lists with Tasks
            project.Lists = TestData.TestListCollection.Values
                .Where(l => l.ProjectId == project.Id)
                .OrderBy(l => l.Position)
                .ToList();

            foreach (var list in project.Lists) {
                list.Tasks = TestData.TestTaskCollection.Values
                    .Where(t => t.ListId == list.Id)
                    .ToList();
            }

            // Populate Members
            project.Members = TestData.TestProjectMemberCollection.Values
                .Where(m => m.ProjectId == project.Id)
                .ToList();
        }

        return Ok(projects);
    }

    [HttpPut("{projectId}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateProject(Guid projectId, [FromBody] ProjectDto updatedProject) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        if (!TestData.TestProjectCollection.ContainsKey(projectId)) {
            return NotFound($"Project with ID {projectId} not found");
        }

        updatedProject.Id = projectId;
        updatedProject.UpdatedDate = DateTime.UtcNow;

        TestData.TestProjectCollection[projectId] = updatedProject;

        return Ok(updatedProject);
    }

    [HttpDelete("{projectId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteProject(Guid projectId) {
        if (!TestData.TestProjectCollection.ContainsKey(projectId)) {
            return NotFound($"Project with ID {projectId} not found");
        }

        TestData.TestProjectCollection.Remove(projectId);

        // Clean up related lists
        var listsToRemove = TestData.TestListCollection.Values
            .Where(l => l.ProjectId == projectId)
            .Select(l => l.Id)
            .ToList();

        foreach (var listId in listsToRemove) {
            TestData.TestListCollection.Remove(listId);
        }

        return NoContent();
    }

    [HttpPost("{projectId}/members")]
    [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AddProjectMember(Guid projectId,
        [FromBody] CreateProjectMemberDto createMember) {
        if (!TestData.TestProjectCollection.ContainsKey(projectId)) {
            return NotFound($"Project with ID {projectId} not found");
        }

        // Validate workspace member exists
        if (!TestData.TestWorkspaceMemberCollection.ContainsKey(createMember.WorkspaceMemberId)) {
            return BadRequest("Invalid workspace member ID");
        }

        // Validate that the workspace member belongs to the same workspace as the project
        var project = TestData.TestProjectCollection[projectId];
        var workspaceMember =
            TestData.TestWorkspaceMemberCollection[createMember.WorkspaceMemberId];

        if (workspaceMember.WorkspaceId != project.WorkspaceId) {
            return BadRequest("Workspace member does not belong to the project's workspace");
        }

        var member = new ProjectMemberDto {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            WorkspaceMemberId = createMember.WorkspaceMemberId,
            CreatedDate = DateTime.UtcNow
        };

        if (project.Members == null) {
            project.Members = new List<ProjectMemberDto>();
        }

        // Check if member already exists in project
        if (project.Members.Any(m => m.WorkspaceMemberId == member.WorkspaceMemberId)) {
            return BadRequest("Member already exists in project");
        }

        TestData.TestProjectMemberCollection.Add(member.Id, member);
        project.Members.Add(member);

        return CreatedAtAction(
            nameof(GetProjectById),
            new { projectId },
            member
        );
    }

    [HttpPost("{projectId}/lists")]
    [ProducesResponseType(typeof(ListDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CreateList(Guid projectId, [FromBody] ListDto list) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        if (!TestData.TestProjectCollection.ContainsKey(projectId)) {
            return NotFound($"Project with ID {projectId} not found");
        }

        list.Id = Guid.NewGuid();
        list.ProjectId = projectId;
        list.CreatedDate = DateTime.UtcNow;

        // Set position to be last in current lists
        var currentMaxPosition = TestData.TestListCollection.Values
            .Where(l => l.ProjectId == projectId)
            .Select(l => l.Position)
            .DefaultIfEmpty(-1)
            .Max();
        list.Position = currentMaxPosition + 1;

        TestData.TestListCollection.Add(list.Id, list);

        var project = TestData.TestProjectCollection[projectId];
        if (project.Lists == null) {
            project.Lists = new List<ListDto>();
        }

        project.Lists.Add(list);

        return CreatedAtAction(
            nameof(GetProjectById),
            new { projectId },
            list
        );
    }

    [HttpGet("{projectId}/lists")]
    public IActionResult GetProjectLists(Guid projectId) {
        if (!TestData.TestProjectCollection.ContainsKey(projectId))
            return NotFound($"Project with ID {projectId} not found");

        var lists = TestData.TestListCollection.Values
            .Where(l => l.ProjectId == projectId)
            .OrderBy(l => l.Position)
            .ToList();

        foreach (var list in lists) {
            list.Tasks = TestData.TestTaskCollection.Values
                .Where(t => t.ListId == list.Id)
                .ToList();
        }

        return Ok(lists);
    }
}
