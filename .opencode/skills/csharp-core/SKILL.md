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
- Inherit `EntityBase<TId>` and `IAggregateRoot`.
- Minimize public setters; use methods for state changes.
- Call `RegisterDomainEvent()` on meaningful state transitions.

## IDs
Use `Ardalis.SharedKernel.ValueObject` for IDs.
```csharp
public sealed class RouteId(long value) : ValueObject
{
  if (value <= 0)
      throw new DomainValidationException("Id must be positive.", nameof(value));
  public long Value { get; } = value;

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
```

## Value Objects
- Co-located in aggregate folder.
- Immutable; equality by properties.
- Use `From()` factory methods (parse, don't validate).

## Specifications
- In `{Aggregate}/Specifications/`, inherit `Specification<T>`.
- All conditional queries go here, never inline in repositories.

## Domain Events
- Past-tense names: `RouteCreatedEvent`, inherit `DomainEventBase`.
- Handlers in `{Aggregate}/Handlers/`, implement `INotificationHandler<TEvent>`.

## Forbidden in Core
- EF Core attributes, DbContext, HTTP, ASP.NET Core.
- Use-case orchestration (domain event handlers are allowed).

## Invariants
- Enforce domain business rules with `DomainValidationException` in constructors and factory methods.
- Use `Ardalis.GuardClauses` for defensive null/range checks on non-domain code only (configuration, pipeline behaviors).
- Business rule violations throw `DomainValidationException`, not return Results.
- The Mediator `DomainExceptionBehavior` pipeline catches `DomainValidationException` and converts to `Result.Error()`.
- See `harness/specs/SPEC-Architecture-ExceptionHandling.md` for the full error handling strategy.
