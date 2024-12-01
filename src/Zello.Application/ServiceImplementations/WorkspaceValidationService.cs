using Microsoft.EntityFrameworkCore;
using Zello.Application.Exceptions;
using Zello.Application.ServiceInterfaces.ExceptionInterfaces;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Data;

namespace Zello.Application.ServiceImplementations;

public class WorkspaceValidationService : IWorkspaceValidationService {
    private readonly ApplicationDbContext _context;

    public WorkspaceValidationService(ApplicationDbContext context) {
        _context = context;
    }

    public async Task ValidateWorkspaceAccess(Guid workspaceId, Guid userId,
        AccessLevel? userAccess) {
        if (userAccess == AccessLevel.Admin) return;

        var hasAccess = await _context.WorkspaceMembers
            .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);

        if (!hasAccess)
            throw new InsufficientPermissionsException();
    }

    public async Task ValidateManagePermissions(Guid workspaceId, Guid userId,
        AccessLevel? userAccess) {
        if (userAccess == AccessLevel.Admin) return;

        var memberAccess = await _context.WorkspaceMembers
            .Where(m => m.WorkspaceId == workspaceId && m.UserId == userId)
            .Select(m => m.AccessLevel)
            .FirstOrDefaultAsync();

        if (memberAccess < AccessLevel.Owner)
            throw new InsufficientPermissionsException();
    }

    public async Task ValidateAccessLevelAssignment(Guid workspaceId, Guid userId,
        AccessLevel newLevel, AccessLevel? adminAccess) {
        if (adminAccess == AccessLevel.Admin) return;

        var currentUserAccess = await _context.WorkspaceMembers
            .Where(m => m.WorkspaceId == workspaceId && m.UserId == userId)
            .Select(m => m.AccessLevel)
            .FirstOrDefaultAsync();

        if (newLevel > currentUserAccess)
            throw new WorkspaceServiceException(WorkspaceErrorMessages.InvalidAccessLevel);
    }

    public async Task EnsureWorkspaceExists(Guid workspaceId) {
        var exists = await _context.Workspaces.AnyAsync(w => w.Id == workspaceId);
        if (!exists) throw new WorkspaceNotFoundException();
    }

    public async Task EnsureUserExists(Guid userId) {
        var exists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!exists) throw new WorkspaceServiceException(WorkspaceErrorMessages.UserNotFound);
    }

    public async Task EnsureNotExistingMember(Guid workspaceId, Guid userId) {
        var exists = await _context.WorkspaceMembers
            .AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);
        if (exists) throw new WorkspaceServiceException(WorkspaceErrorMessages.UserAlreadyMember);
    }
}
