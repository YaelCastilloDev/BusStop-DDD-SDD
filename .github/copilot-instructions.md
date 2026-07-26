# GitHub Copilot Instructions for BusStop

## Project Overview
This is the **BusStop** project for .NET 10, built on Clean Architecture and Domain-Driven Design (DDD) patterns.

## Architecture & Project Structure

### C# Conventions
- Use standard Microsoft naming conventions
- Use `PascalCase` for types and methods, `camelCase` for parameters and private fields
- Use `I` prefix for interfaces (e.g., `IRepository`)
- Use `Async` suffix for async methods (e.g., `GetByIdAsync`)
- Prefix private fields with `_` (e.g., `_repository`)
- Always use {} for blocks except single-line exits (e.g. `return`, `throw`)
- Always keep single line blocks on one line (e.g., `if (x) return y;`)
- Prefer primary constructors for required dependencies
- Never use primary constructor parameters directly - always assign to private fields for clarity and testability

### Core Dependencies Flow
- **Core** ← UseCases ← Infrastructure 
- **Core** ← UseCases ← Web
- Never allow Core to depend on outer layers

### Key Projects
- **Core**: Domain entities, aggregates, value objects, specifications, interfaces
- **UseCases**: Commands/queries (CQRS), Mediator handlers, application logic  
- **Infrastructure**: EF Core, external services, email, file access
- **Web**: FastEndpoints API, REPR pattern, validation

## Development Patterns

### API Endpoints (FastEndpoints + REPR)
- One endpoint per file: `Create.cs`, `Update.cs`, `Delete.cs`, `GetById.cs`
- Separate request/response/validator files: `Create.CreateRequest.cs`, `Create.CreateValidator.cs`
- Use `Endpoint<TRequest, TResponse>` base class
- Example: `src/BusStop.Web/Routes/Create.cs`

### Domain Model (Core)
- Entities use encapsulation - minimize public setters
- Group related entities into Aggregates
- Use Value Objects (e.g., `ContributorName.From()`)
- Domain Events for cross-aggregate communication
- Repository interfaces defined in Core, implemented in Infrastructure

### Use Cases (CQRS)
- Commands for mutations, Queries for reads
- Queries can bypass repository pattern for performance
- Use Mediator (source generator) for command/query handling
- Chain of responsibility for cross-cutting concerns (logging, validation)

### Validation Strategy
- **API Level**: FluentValidation on request DTOs (FastEndpoints integration)
- **Use Case Level**: Validate commands/queries (defensive coding)
- **Domain Level**: Business invariants throw exceptions, assume pre-validated input

## Essential Commands

### Build & Test
```bash
dotnet build BusStop.slnx
dotnet test BusStop.slnx
```

### Entity Framework Migrations
```bash
# From Web project directory
dotnet ef migrations add MigrationName -c AppDbContext -p ../BusStop.Infrastructure/BusStop.Infrastructure.csproj -s BusStop.Web.csproj -o Data/Migrations

dotnet ef database update -c AppDbContext -p ../BusStop.Infrastructure/BusStop.Infrastructure.csproj -s BusStop.Web.csproj
```


## Key Dependencies & Patterns

### Primary Libraries
- **FastEndpoints**: API endpoints (replaced Controllers/Minimal APIs)
- **Mediator**: Command/query handling in UseCases
- **EF Core**: Data access (PostgreSQL with PostGIS)
- **Ardalis.Specification**: Repository query specifications
- **Ardalis.Result**: Error handling pattern
- **Serilog**: Structured logging

### Central Package Management
- All package versions in `Directory.Packages.props`
- Use `<PackageReference Include="..." />` without Version attribute

### Test Organization
- **UnitTests**: Core business logic, use cases
- **IntegrationTests**: Database, infrastructure components  
- **FunctionalTests**: API endpoints (subcutaneous testing)
- Use `Microsoft.AspNetCore.Mvc.Testing` for API tests

## File Organization Conventions

### Web Project Structure
```
Routes/
  Create.cs                    # Endpoint
  Create.CreateRequest.cs      # Request DTO
  Create.CreateResponse.cs     # Response DTO  
  Create.CreateValidator.cs    # FluentValidation
  Update.cs, Delete.cs, etc.
```


## Common Gotchas

- Use absolute paths in EF migration commands
- FastEndpoints uses different validation approach than Controller-based APIs

## VS Code Tasks
Use the predefined tasks: `build`, `publish`, `watch` instead of manual `dotnet` commands when possible.
