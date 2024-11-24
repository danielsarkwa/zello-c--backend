# API Documentation

## CommentsController
**Base URL**: `/api/v1/comments`

### GET /api/v1/comments
Retrieves all comments, optionally filtered by task ID
Query Parameters:
- `taskId` (optional): Guid - Filter comments for a specific task

### GET /api/v1/comments/{commentId}
Retrieves a specific comment by its ID
Path Parameters:
- `commentId`: Guid

### POST /api/v1/comments
Creates a new comment on a task
Request Body:
```json
{
    "taskId": "guid",  // Task to attach comment to
    "content": "string (max 500 chars)"  // Comment text
}
```

### PUT /api/v1/comments/{commentId}
Updates an existing comment's content
Path Parameters:
- `commentId`: Guid
  Request Body:
```json
{
    "content": "string"  // Updated comment text
}
```

### DELETE /api/v1/comments/{commentId}
Removes a comment from the system
Path Parameters:
- `commentId`: Guid

## ListController
**Base URL**: `/api/v1/list`

### GET /api/v1/list/{listId}
Retrieves a specific list with its details
Path Parameters:
- `listId`: Guid

### GET /api/v1/list
Gets all lists, optionally filtered by project
Query Parameters:
- `projectId` (optional): Guid - Filter lists for a specific project

### PUT /api/v1/list/{listId}
Updates a list's properties
Path Parameters:
- `listId`: Guid
  Request Body:
```json
{
    "name": "string",     // New list name
    "position": "integer" // New position in project
}
```

### PUT /api/v1/list/{listId}/position
Updates only the position of a list within its project
Path Parameters:
- `listId`: Guid
  Request Body: integer (new position)

### DELETE /api/v1/list/{listId}
Removes a list and all its tasks
Path Parameters:
- `listId`: Guid

### POST /api/v1/list/{listId}/tasks
Creates a new task in the specified list
Path Parameters:
- `listId`: Guid
  Request Body:
```json
{
    "name": "string",
    "description": "string",
    "status": "enum",      // Task status (NotStarted, InProgress, etc.)
    "priority": "enum",    // Task priority (High, Medium, Low)
    "deadline": "datetime" // Task due date
}
```

### GET /api/v1/list/{listId}/tasks
Retrieves all tasks in a specific list
Path Parameters:
- `listId`: Guid

## ProjectController
**Base URL**: `/api/v1/project`

### POST /api/v1/project
Creates a new project in a workspace
Request Body:
```json
{
    "workspaceId": "guid",
    "name": "string",
    "description": "string",
    "startDate": "datetime",
    "endDate": "datetime",
    "status": "enum"      // Project status (InProgress, Completed, etc.)
}
```

### GET /api/v1/project/{projectId}
Retrieves detailed information about a specific project
Path Parameters:
- `projectId`: Guid

### GET /api/v1/project
Gets all projects, optionally filtered by workspace
Query Parameters:
- `workspaceId` (optional): Guid - Filter projects for a specific workspace

### PUT /api/v1/project/{projectId}
Updates project details
Path Parameters:
- `projectId`: Guid
  Request Body:
```json
{
    "name": "string",
    "description": "string",
    "startDate": "datetime",
    "endDate": "datetime",
    "status": "enum"
}
```

### DELETE /api/v1/project/{projectId}
Removes a project and all its associated lists and tasks
Path Parameters:
- `projectId`: Guid

### POST /api/v1/project/{projectId}/members
Adds a new member to the project
Path Parameters:
- `projectId`: Guid
  Request Body:
```json
{
    "workspaceMemberId": "guid", // Member from workspace to add
    "roleId": "guid"             // Role to assign to member
}
```

### POST /api/v1/project/{projectId}/lists
Creates a new list in the project
Path Parameters:
- `projectId`: Guid
  Request Body:
```json
{
    "name": "string" // Name of the new list
}
```

### GET /api/v1/project/{projectId}/lists
Retrieves all lists in a project, ordered by position
Path Parameters:
- `projectId`: Guid

## TaskController
**Base URL**: `/api/v1/task`

### GET /api/v1/task/{taskId}
Retrieves detailed information about a specific task
Path Parameters:
- `taskId`: Guid

### PUT /api/v1/task/{taskId}
Updates task details
Path Parameters:
- `taskId`: Guid
  Request Body:
```json
{
    "name": "string",
    "description": "string",
    "status": "enum",
    "priority": "enum",
    "deadline": "datetime"
}
```

### DELETE /api/v1/task/{taskId}
Removes a task and its associated comments
Path Parameters:
- `taskId`: Guid

### POST /api/v1/task/{taskId}/move
Moves a task to a different list within the same project
Path Parameters:
- `taskId`: Guid
  Request Body:
```json
{
    "targetListId": "guid" // Destination list ID
}
```

### GET /api/v1/task/{taskId}/comments
Retrieves all comments on a specific task
Path Parameters:
- `taskId`: Guid

### POST /api/v1/task/{taskId}/comments
Adds a new comment to a task
Path Parameters:
- `taskId`: Guid
  Request Body:
```json
{
    "content": "string (max 500 chars)" // Comment text
}
```

### POST /api/v1/task/{taskId}/labels
Adds labels to a task
Path Parameters:
- `taskId`: Guid
  Request Body:
```json
[
    {
        "name": "string",  // Label name
        "color": "string" // Label color code
    }
]
```

### POST /api/v1/task/{taskId}/assignees
Assigns users to a task
Path Parameters:
- `taskId`: Guid
  Request Body:
```json
["guid", "guid"] // Array of user IDs to assign
```

## WorkspacesController
**Base URL**: `/api/v1/workspaces`


### GET /api/v1/workspaces/{workspaceId}/roles
Lists all roles in a workspace
Path Parameters:
- `workspaceId`: Guid

### POST /api/v1/workspaces
Creates a new workspace
Request Body:
```json
{
    "name": "string",
    "ownerId": "guid",
    "description": "string"
}
```

### GET /api/v1/workspaces/{workspaceId}
Retrieves workspace details
Path Parameters:
- `workspaceId`: Guid

### GET /api/v1/workspaces
Retrieves all workspaces

### PUT /api/v1/workspaces/{workspaceId}
Updates workspace properties
Path Parameters:
- `workspaceId`: Guid
  Request Body:
```json
{
    "name": "string",
    "description": "string"
}
```

### DELETE /api/v1/workspaces/{workspaceId}
Removes a workspace and all associated projects
Path Parameters:
- `workspaceId`: Guid

### GET /api/v1/workspaces/{workspaceId}/projects
Lists all projects in a workspace
Path Parameters:
- `workspaceId`: Guid

### GET /api/v1/workspaces/{workspaceId}/members
Lists all members in a workspace
Path Parameters:
- `workspaceId`: Guid

### POST /api/v1/workspaces/{workspaceId}/members
Adds a new member to the workspace
Path Parameters:
- `workspaceId`: Guid
  Request Body:
```json
{
    "userId": "guid",
    "roleId": "guid"
}
```

### POST /api/v1/workspaces/{workspaceId}/roles
Creates a custom role for the workspace
Path Parameters:
- `workspaceId`: Guid
  Request Body:
```json
{
    "name": "string",
    "description": "string"
}
```


## UserController
**Base URL**: `/api/v1/user`

### GET /api/v1/user/me
Retrieves the current authenticated user's information
- Requires authentication
- Returns user details or 401 if not authenticated

### POST /api/v1/user/register
Registers a new user
Request Body:
```json
{
    "username": "string",  // max 50 characters
    "email": "string",     // max 255 characters, must be valid email
    "name": "string",      // max 100 characters
    "password": "string"   // will be hashed before storage
}
```
Response: Returns created UserDto (without password hash)

### GET /api/v1/user/{userId}
Retrieves a specific user by their ID
Path Parameters:
- `userId`: Guid

### GET /api/v1/user
Retrieves all users in the system

### PUT /api/v1/user/{userId}
Updates a user's information
Path Parameters:
- `userId`: Guid
  Request Body:
```json
{
    "username": "string",  // max 50 characters
    "email": "string",     // max 255 characters, must be valid email
    "name": "string"      // max 100 characters
}
```

### DELETE /api/v1/user/{userId}
Removes a user and their associated data (workspace memberships, etc.)
Path Parameters:
- `userId`: Guid