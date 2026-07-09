---
name: clean-architecture
description: BusStop Clean Architecture layer rules (Ardalis template). Use when implementing features following Clean Architecture patterns.
---

# Clean Architecture Conventions

Full spec: `harness/specs/clean-architecture-conventions.md`.

## Dependency Flow
Core ← UseCases ← Infrastructure
Core ← UseCases ← Web (Web references Infrastructure for DI only)

## Projects
`BusStop.Core`, `BusStop.UseCases`, `BusStop.Infrastructure`, `BusStop.Web`, `BusStop.AspireHost`, `BusStop.ServiceDefaults`
Tests: `BusStop.UnitTests`, `BusStop.IntegrationTests`, `BusStop.FunctionalTests`

## Core
- One `{Entity}Aggregate/` folder per aggregate root.
- `EntityBase<TId>` + `IAggregateRoot`.
- `Ardalis.SharedKernel.ValueObject` for IDs.
- Value objects co-located in aggregate folder with `From()` factories.
- Specifications in `{Aggregate}/Specifications/`.
- Domain events in `{Aggregate}/Events/`, handlers in `{Aggregate}/Handlers/`.
- No EF Core, HTTP, or ASP.NET Core references.

## UseCases
- Feature slices: `Routes/Create/`, not `Commands/`.
- Handlers: `ICommandHandler<,>` via **Mediator** (not MediatR).
- Return `Result` / `Result<T>` for expected failures.
- Handlers must NOT contain try-catch for domain exceptions — the `DomainExceptionBehavior` Mediator pipeline catches `DomainValidationException` and wraps in `Result<T>.Error()`. See `harness/specs/SPEC-Architecture-ExceptionHandling.md`.
- No validators here — Web layer owns FastEndpoints validators.

## Infrastructure
- `EfRepository<T>` with Specifications, no inline LINQ.
- `Data/Config/`, `Data/Queries/`, `MediatorDomainEventDispatcher`.

## Web
- FastEndpoints REPR only (not Controllers).
- One endpoint per file: `Create.cs`, `GetById.cs`, `List.cs`.
- Co-located DTOs and `Validator<TRequest>`.
- Map `Result` via `ResultExtensions`.
- Primary constructors assign to `_privateFields`.

## Non-Negotiables
- Never use MediatR — use Mediator source generator.
- Never skip FastEndpoints validators on input endpoints.
- Every use case: command/query + handler + endpoint + tests.
