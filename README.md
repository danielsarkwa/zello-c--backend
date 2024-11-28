# Zello

A task management system built with .NET 8 and PostgreSQL. Inspired by Trello, Zello allows teams to organize projects using boards, lists, and cards.

For additional documentation visit: [Zello Documentation](https://kelo221.github.io/fs19_CSharp_Teamwork/)
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
- Admin: username: `admin`, password: `admin`
- Owner: username: `john`, password: `john`
- Member: username: `jane`, password: `jane`