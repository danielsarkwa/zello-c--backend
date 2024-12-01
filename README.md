# Zello

A task management system built with .NET 8 and PostgreSQL. Inspired by Trello, Zello allows teams to organize projects using boards, lists, and cards.

For additional documentation visit: [Zello Documentation](https://kelo221.github.io/fs19_CSharp_Teamwork/)

Server has been deployed on https://zello123-b0bracfcbsgaa5gp.northeurope-01.azurewebsites.net
## Features
- User authentication and authorization with JWT
- Workspaces for team organization
- Project management with customizable task lists
- Task tracking with comments and assignments
- Role-based access control (Guest, Member, Owner, Admin)

## Quick Start
1. Ensure PostgreSQL is installed and running
2. Update the connection string in your configuration
3. Run the application - the database will be automatically created and seeded with sample data

## Default Users
- Admin: username: `admin1234`, password: `admin1234`
- Owner: username: `johnjohn`, password: `johnjohn`
- Member: username: `janejane`, password: `janejane`

## ERD

```mermaid
erDiagram
    USER ||--o{ WORKSPACE : Owns
    WORKSPACE ||--o{ PROJECT : contains
    PROJECT ||--o{ LIST : contains
    LIST ||--o{ TASK : contains
    TASK ||--o{ COMMENTS : contains
```

```mermaid
erDiagram
    user {
        GUID Id PK "Primary Key"
        string Username "Required, MaxLength: 30"
        string Name "Required, MaxLength: 30"
        string Email "Required, MaxLength: 100, Unique"
        string PasswordHash "Required"
        GUID AccessLevel FK "Enum e.g., Admin, Member, Owner, Guest"
        datetime CreatedDate "Default: Current Date"
    }
    workspace {
        GUID Id PK "Primary Key"
        string Name "Required, MaxLength: 100"
        GUID OwnerId FK "Foreign Key referencing User"
        datetime CreatedDate "Default: Current Date"
    }
    project {
        GUID Id PK "Primary Key"
        GUID WorkspaceId FK "Foreign Key referencing Workspace"
        string Name "Required, MaxLength: 100"
        string Description "Optional, MaxLength: 500"
        datetime StartDate "Optional"
        datetime EndDate "Optional"
        string Status "Enum: NotStarted, InProgress, Completed"
        datetime CreatedDate "Default: Current Date"
    }
    list {
        GUID Id PK "Primary Key"
        GUID ProjectId FK "Foreign Key referencing Project"
        string Name "Required, MaxLength: 100"
        int Position "Position of the list on the board"
        datetime CreatedDate "Default: Current Date"
    }
    task {
        GUID Id PK "Primary Key"
        string Name "Required, MaxLength: 100"
        string Description "Optional, MaxLength: 500"
        string Status "Enum: NotStarted, InProgress, Completed"
        string Priority "Enum: Low, Medium, High, Urgent"
        datetime Deadline "Optional"
        datetime CreatedDate "Default: Current Date"
        GUID ProjectId FK "Foreign Key referencing Project"
        GUID ListId FK "Foreign Key referencing List"
    }
    comment {
        GUID Id PK "Primary Key"
        GUID TaskId FK "Optional Foreign Key referencing Task"
        GUID UserId FK "Foreign Key referencing User"
        string Content "Required, MaxLength: 500"
        datetime CreatedDate "Default: Current Date"
    }
    workspace_member {
        GUID Id PK "Primary Key"
        GUID WorkspaceId FK "Foreign Key referencing Workspace"
        GUID UserId FK "Foreign Key referencing User"
        GUID AccessLevel FK "Enum e.g., Admin, Member, Owner, Guest"
        datetime CreatedDate "Default: Current Date"
    }
    project_member {
        GUID Id PK "Primary Key"
        GUID ProjectId FK "Foreign Key referencing Project"
        GUID WorkspaceMemberId FK "Foreign Key referencing workspace_member"
        GUID AccessLevel FK "Enum e.g., Admin, Member, Owner, Guest"
        datetime CreatedDate "Default: Current Date"
    }
    task_assignee {
        GUID Id PK "Primary Key"
        GUID TaskId FK "Foreign Key referencing Task"
        GUID UserId FK "Foreign Key referencing User"
        datetime AssignedDate "Default: Current Date"
    }
    
    user ||--o{ workspace_member : "belongs to"
    user ||--o{ comment : "writes"
    user ||--o{ task_assignee : "is assigned"
    user ||--o{ workspace : "owns"
    workspace ||--o| workspace_member : "has members"
    workspace ||--o| project : "contains"
    project ||--o| project_member : "has members"
    workspace_member ||--o{ project_member: "belongs to"
    project ||--o{ list : "contains"
    list ||--o{ task : "contains"
    task ||--o{ task_assignee : "has assignees"
    task ||--o{ comment : "has comments"
```

### Database Entities Definition

| Entity               | Description                                                         |
|----------------------|---------------------------------------------------------------------|
| **user**             | Represents an individual user in the application.                  |
| **workspace**        | Represents a workspace that organizes projects and members. A workspace is owned by a single user.       |
| **project**          | Represents a project within a workspace, containing tasks and other project-related data. |
| **list**             | Represents a list within a project, where tasks can be organized. List in Zello is also a kanban column that contains tasks.              |
| **task**             | Represents an individual task within a project and list.           |
| **comment**          | Represents a comment made by a user on a task.                     |
| **workspace_member** | Represents a user's membership in a workspace with a specific role. |
| **project_member**   | Represents a user's membership in a project with a specific role. |
| **task_assignee**    | Represents the assignment of a task to a user.                     |
| **role**             | Represents a role that defines a user's permissions in a workspace or project. A role can be Admin, Member, Owner or Guest |

### Database Relationship Summary

| Entities                          | Relationship Type | Description                                                                                                   |
|-----------------------------------|-------------------|---------------------------------------------------------------------------------------------------------------|
| `user` ↔ `workspace_member`       | One-to-Many       | A user can be a member of multiple workspaces, but each workspace member corresponds to only one user.        |
| `user` ↔ `workspace`              | One-to-Many       | A user can own multiple workspaces, but each workspace is owned by a single user.                             |
| `user` ↔ `comment`                | One-to-Many       | A user can write multiple comments, but each comment is written by one user.                                  |
| `user` ↔ `task_assignee`          | One-to-Many       | A user can be assigned to multiple tasks, but each task assignment references one user.                       |
| `workspace` ↔ `workspace_member`  | One-to-Many       | A workspace can have multiple members, but each membership references one workspace.                          |
| `workspace` ↔ `project`           | One-to-Many       | A workspace can contain multiple projects, but each project belongs to one workspace.                         |
| `project` ↔ `project_member`      | One-to-Many       | A project can have multiple members, but each project membership references one project.                      |
| `workspace_member` ↔ `project_member` | One-to-Many    | A workspace member can belong to multiple projects, but each project member references one workspace member.  |
| `project` ↔ `list`                | One-to-Many       | A project can contain multiple lists, but each list belongs to one project.                                   |
| `list` ↔ `task`                   | One-to-Many       | A list can contain multiple tasks, but each task belongs to one list.                                         |
| `task` ↔ `task_assignee`          | One-to-Many       | A task can have multiple assignees, but each task assignment references one task.                             |
| `task` ↔ `comment`                | One-to-Many       | A task can have multiple comments, but each comment references one task.                                      |
