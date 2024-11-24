# API Documentation

## Authentication

### Register User
```http
POST /api/v1/auth/register
```
```json
{
  "username": "string",    // max 50 characters
  "email": "string",      // max 255 characters, must be valid email
  "name": "string",       // max 100 characters
  "password": "string"    // will be hashed before storage
}
```

### Login
```http
POST /api/v1/auth/login
```
```json
{
  "username": "string",
  "password": "string"
}
```

### Logout
```http
POST /api/v1/auth/logout
```

## User Management

### Get Current User
```http
GET /api/v1/user/me
```
- Requires authentication
- Returns user details or 401 if not authenticated

### Get User
```http
GET /api/v1/user/{userId}
```
Path Parameters:
- `userId`: Guid

### Get All Users
```http
GET /api/v1/user
```

### Update User
```http
PUT /api/v1/user/{userId}
```
```json
{
  "username": "string",    // max 50 characters
  "email": "string",      // max 255 characters, must be valid email
  "name": "string"        // max 100 characters
}
```

### Delete User
```http
DELETE /api/v1/user/{userId}
```

### Update User System Role
```http
PUT /api/v1/system/users/{userId}/role
```
```json
{
  "role": "Admin"
}
```

## Workspace Management

### Create Workspace
```http
POST /api/v1/workspaces
```
```json
{
  "name": "string",
  "description": "string",
  "ownerId": "guid"
}
```

### Get All Workspaces
```http
GET /api/v1/workspaces
```

### Get Workspace
```http
GET /api/v1/workspaces/{workspaceId}
```

### Update Workspace
```http
PUT /api/v1/workspaces/{workspaceId}
```
```json
{
  "name": "string",
  "description": "string"
}
```

### Delete Workspace
```http
DELETE /api/v1/workspaces/{workspaceId}
```

### Get Workspace Projects
```http
GET /api/v1/workspaces/{workspaceId}/projects
```

### Get Workspace Members
```http
GET /api/v1/workspaces/{workspaceId}/members
```

### Add Workspace Member
```http
POST /api/v1/workspaces/{workspaceId}/members
```
```json
{
  "userId": "guid",
  "roleId": "guid"
}
```

### Get Workspace Roles
```http
GET /api/v1/workspaces/{workspaceId}/roles
```

### Create Workspace Role
```http
POST /api/v1/workspaces/{workspaceId}/roles
```
```json
{
  "name": "string",
  "description": "string"
}
```

## Project Management

### Create Project
```http
POST /api/v1/project
```
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

### Get Project
```http
GET /api/v1/project/{projectId}
```

### Get All Projects
```http
GET /api/v1/project
```
Query Parameters:
- `workspaceId` (optional): Guid - Filter projects for a specific workspace

### Update Project
```http
PUT /api/v1/project/{projectId}
```
```json
{
  "name": "string",
  "description": "string",
  "startDate": "datetime",
  "endDate": "datetime",
  "status": "enum"
}
```

### Delete Project
```http
DELETE /api/v1/project/{projectId}
```

### Add Project Member
```http
POST /api/v1/project/{projectId}/members
```
```json
{
  "workspaceMemberId": "guid",
  "roleId": "guid"
}
```

### Create Project List
```http
POST /api/v1/project/{projectId}/lists
```
```json
{
  "name": "string"
}
```

### Get Project Lists
```http
GET /api/v1/project/{projectId}/lists
```

## List Management

### Get List
```http
GET /api/v1/list/{listId}
```

### Get All Lists
```http
GET /api/v1/list
```
Query Parameters:
- `projectId` (optional): Guid - Filter lists for a specific project

### Update List
```http
PUT /api/v1/list/{listId}
```
```json
{
  "name": "string",
  "position": "integer"
}
```

### Delete List
```http
DELETE /api/v1/list/{listId}
```

### Update List Position
```http
PUT /api/v1/list/{listId}/position
```
```json
2  // New position value
```

### Create Task in List
```http
POST /api/v1/list/{listId}/tasks
```
```json
{
  "name": "string",
  "description": "string",
  "status": "enum",      // Task status (NotStarted, InProgress, etc.)
  "priority": "enum",    // Task priority (High, Medium, Low)
  "deadline": "datetime"
}
```

### Get List Tasks
```http
GET /api/v1/list/{listId}/tasks
```

## Task Management

### Get Task
```http
GET /api/v1/task/{taskId}
```

### Update Task
```http
PUT /api/v1/task/{taskId}
```
```json
{
  "name": "string",
  "description": "string",
  "status": "enum",
  "priority": "enum",
  "deadline": "datetime"
}
```

### Delete Task
```http
DELETE /api/v1/task/{taskId}
```

### Move Task
```http
POST /api/v1/task/{taskId}/move
```
```json
{
  "targetListId": "guid"
}
```

### Get Task Comments
```http
GET /api/v1/task/{taskId}/comments
```

### Add Task Assignees
```http
POST /api/v1/task/{taskId}/assignees
```
```json
[
  "guid",    // Array of user IDs to assign
  "guid"
]
```

### Add Task Labels
```http
POST /api/v1/task/{taskId}/labels
```
```json
[
  {
    "name": "string",
    "color": "string"    // Color code
  }
]
```

## Comments

### Create Comment
```http
POST /api/v1/task/{taskId}/comments
```
```json
{
  "content": "string"    // max 500 chars
}
```

### Get All Comments
```http
GET /api/v1/comments
```
Query Parameters:
- `taskId` (optional): Guid - Filter comments for a specific task

### Get Comment
```http
GET /api/v1/comments/{commentId}
```

### Update Comment
```http
PUT /api/v1/comments/{commentId}
```
```json
{
  "content": "string"    // max 500 chars
}
```

### Delete Comment
```http
DELETE /api/v1/comments/{commentId}
```
