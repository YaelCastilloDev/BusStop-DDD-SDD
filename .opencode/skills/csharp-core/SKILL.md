---
name: csharp-core
description: BusStop.Core domain model patterns. Use when working in BusStop.Core project or implementing domain aggregates.
---

# BusStop.Core Patterns

## Aggregate Structure
```
{Entity}Aggregate/
├── {Entity}.cs          # aggregate root
├── {Entity}Id.cs        # Ardalis.ValueObject
├── {Entity}Name.cs      # value object
├── Events/
├── Handlers/
└── Specifications/
```

## Aggregate Root
- Inherit `EntityBase<long>` and `IAggregateRoot`.
- Minimize public setters; use methods for state changes.
- Call `RegisterDomainEvent()` on meaningful state transitions.

## IDs
`Ardalis.SharedKernel.ValueObject` subclasses. Constructors use `Guard.Against` for defensive checks.
```csharp
public sealed class RouteId : ValueObject
{
    public long Value { get; }

    public RouteId(long value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

## Value Objects
- Co-located in aggregate folder.
- Immutable; equality by properties.
- Constructors use `Guard.Against.*` (Null, NullOrWhiteSpace, NegativeOrZero, OutOfRange).
- Use `From()` factory methods (parse, don't validate).

## Specifications
- In `{Aggregate}/Specifications/`, inherit `Specification<T>`.
- All conditional queries go here, never inline in repositories.

## Domain Events
- Past-tense names: `RouteCreatedEvent`, inherit `DomainEventBase`.
- Handlers in `{Aggregate}/Handlers/`, implement `INotificationHandler<TEvent>`.

## Error Types
- Per-aggregate `const string` in `Errors/{Entity}Errors.cs`.
- Used in factory methods to build validation error lists.

## Forbidden in Core
- EF Core attributes, DbContext, HTTP, ASP.NET Core.
- Use-case orchestration (domain event handlers are allowed).

## Two-Tier Error Strategy

| Pattern | Handles | Frequency | Example |
|---------|---------|-----------|---------|
| **Result Pattern** | Expected failures (business rules). User input is wrong, state transition not allowed. | High | Empty comment, archived route, already deleted. |
| **Guard Clauses** | Impossible failures (developer/system errors). Invalid data that should have been caught earlier. | Low / Never in production | Null or negative ID passed to internal constructor. |

### Factories → Result<T>
```csharp
public static Result<Comment> Create(string content, long userId, long routeId)
{
    var errors = new List<string>();
    if (string.IsNullOrWhiteSpace(content)) errors.Add(CommentErrors.EmptyContent);
    if (userId <= 0) errors.Add(CommentErrors.InvalidUser);
    if (routeId <= 0) errors.Add(CommentErrors.InvalidRoute);
    if (errors.Count > 0) return Result<Comment>.Error(new ErrorList(errors));
    return Result<Comment>.Success(new Comment(
        new CommentContent(content), new UserId(userId), new RouteId(routeId)));
}
```

### Constructors → Guard.Against
```csharp
private Comment(CommentContent content, UserId userId, RouteId routeId)
{
    Guard.Against.Null(content, nameof(content));
    Guard.Against.Null(userId, nameof(userId));
    Guard.Against.Null(routeId, nameof(routeId));
    Content = content; UserId = userId; RouteId = routeId;
}
```

### State-Changing Methods
```csharp
public Result Delete(UserId deletedBy)
{
    Guard.Against.Null(deletedBy, nameof(deletedBy));   // bug → throw
    if (IsDeleted)
        return Result.Error(new ErrorList([Errors.AlreadyDeleted]));  // business rule → Result
    DeletedAt = DateTime.UtcNow;
    return Result.Success();
}
```

### Handlers
```csharp
var routeResult = Route.Create(request.Name, user.Id);
if (!routeResult.IsSuccess)
    return Result<RouteResponse>.Error(new ErrorList(routeResult.Errors));
var route = routeResult.Value;
```

### Shortcut: CreateEntityPipeline for create flows
When handlers follow the factory → validate → AddAsync → map pattern, use `CreateEntityPipeline.CreateAsync`:
```csharp
return await repository.CreateAsync(Route.Create(request.Name, user.Id), r => r.ToResponse(), ct);
```

### Shortcut: FindRequiredAsync for entity lookups
Replace `FirstOrDefaultAsync(spec) + null check + NotFound` with `FindRequiredAsync<T>`:
```csharp
var routeResult = await routeRepository.FindRequiredAsync(
    new RouteByIdSpec(new RouteId(id)), "Route not found.", ct);
if (!routeResult.IsSuccess)
    return Result<TResponse>.NotFound(routeResult.Errors);
var route = routeResult.Value;
```

### Shortcut: ToResponse() mapper extensions
Map entities to response DTOs via extension methods (defined in UseCases layer, co-located with DTOs):
```csharp
// RouteMapper.cs in UseCases/Routes/
public static RouteResponse ToResponse(this Route route) =>
    new(route.Id, route.Name.Value, route.CreatedById.Value, route.CreatedAt, route.IsDeleted);
```
All handlers use `entity.ToResponse()` instead of inline `new XxxResponse(...)` constructor calls.
