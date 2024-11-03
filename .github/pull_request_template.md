# Pull Request Description

## Changes Made
*Please provide a clear and concise description of your changes*

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Clean Architecture Layer Impact
- [ ] **Domain Layer** (`Domain` project)
  - [ ] Entities/DTOs modified
  - [ ] Domain events added/modified
  - [ ] Domain exceptions added/modified
  - [ ] Enums modified
  - [ ] Value objects modified

- [ ] **Application Layer** (`Application` project)
  - [ ] CQRS commands/queries modified
  - [ ] Interfaces modified
  - [ ] Behaviors modified
  - [ ] Mappings updated (AutoMapper)
  - [ ] Validation rules updated (FluentValidation)
  - [ ] Event handlers modified

- [ ] **Infrastructure Layer** (`Infrastructure` project)
  - [ ] Database context modified
  - [ ] Migrations added
  - [ ] External service integrations modified
  - [ ] Repository implementations changed
  - [ ] Identity services modified

- [ ] **Presentation Layer** (`API` project)
  - [ ] Controllers modified
  - [ ] API endpoints changed
  - [ ] Middleware added/modified
  - [ ] Frontend changes (if applicable)

## Common Project Standards
- [ ] **SOLID Principles**
  - [ ] Single Responsibility: Classes have one reason to change
  - [ ] Open/Closed: New features added through extension
  - [ ] Liskov Substitution: Inheritance used properly
  - [ ] Interface Segregation: Interfaces are focused
  - [ ] Dependency Inversion: Depending on abstractions

- [ ] **Mediator Pattern**
  - [ ] Commands/Queries follow naming convention
  - [ ] Handlers properly implement IRequestHandler
  - [ ] Notifications used appropriately
  - [ ] Pipeline behaviors applied correctly

- [ ] **Code Quality**
  - [ ] Code formatted using `dotnet format`
  - [ ] XML documentation for public APIs
  - [ ] Following project file structure conventions
  - [ ] Using shared kernel where appropriate
  - [ ] Exception handling follows project patterns
  - [ ] Logging using provided extensions
  - [ ] No hardcoded strings (using constants/resources)

## Testing
- [ ] Unit tests added
  - [ ] Command/Query handler tests
  - [ ] Domain logic tests
  - [ ] Controller tests
- [ ] Integration tests if applicable
- [ ] Testing database used appropriately
- [ ] Respects testing patterns in solution

## Database Changes
- [ ] Entity configurations added/modified
- [ ] Migration script generated
- [ ] Migration tested
- [ ] Seeding data updated if needed

## Documentation
- [ ] XML comments added for new public APIs
- [ ] README.md updated if needed
- [ ] API documentation updated (if applicable)
- [ ] Migration guidelines added (if applicable)

## Breaking Changes
*List any breaking changes and migration steps if applicable*

## Related Issues
*Reference any related issues using # followed by the issue number*

## Additional Context
*Add any other context about the PR here*

## Deployment Notes
- [ ] Configuration changes required
- [ ] Environment variables added
- [ ] Database migration required
- [ ] External service configuration needed
