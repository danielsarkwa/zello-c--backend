namespace Zello.Application.Exceptions;

public class WorkspaceErrorMessages {
    public const string WorkspaceNotFound = "Workspace not found";
    public const string InsufficientPermissions = "Insufficient permissions to perform this action";
    public const string UserNotFound = "User not found";
    public const string MemberNotFound = "Workspace member not found";
    public const string UserAlreadyMember = "User is already a member of this workspace";
    public const string InvalidAccessLevel = "Cannot assign access level higher than your own";
    public const string UserIdMissing = "User ID is missing or invalid";
}

public class WorkspaceServiceException : Exception {
    public WorkspaceServiceException(string message) : base(message) { }

    public WorkspaceServiceException(string message, Exception innerException)
        : base(message, innerException) {
    }
}

public class WorkspaceNotFoundException : WorkspaceServiceException {
    public WorkspaceNotFoundException() : base(WorkspaceErrorMessages.WorkspaceNotFound) { }
}

public class InsufficientPermissionsException : WorkspaceServiceException {
    public InsufficientPermissionsException() :
        base(WorkspaceErrorMessages.InsufficientPermissions) {
    }
}

public class WorkspaceMemberNotFoundException : WorkspaceServiceException {
    public WorkspaceMemberNotFoundException() : base(WorkspaceErrorMessages.MemberNotFound) { }
}
