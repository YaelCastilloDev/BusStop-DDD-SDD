# Exception Handling (Retired)

The `DomainValidationException` + `DomainExceptionBehavior` pipeline pattern has been replaced by a two-tier Result/Guard strategy.

## Current Pattern
- **Canonical reference:** `.opencode/skills/csharp-core/SKILL.md` — Two-Tier Error Strategy section
- **Web layer context:** `.opencode/skills/csharp-web/SKILL.md` — Error Handling Context section

## Safety Net
The `DomainExceptionBehavior` pipeline remains in `src/BusStop.Web/Configurations/DomainExceptionBehavior.cs` as a safety net. It catches any remaining `DomainValidationException` throws and converts to `Result.Error()`. No Core code should emit these.

## What Changed
| Before | After |
|--------|-------|
| Factory methods `throw DomainValidationException` | Factory methods `return Result<T>.Error()` |
| Mediator pipeline catches exceptions | Handlers match on `Result<T>` directly |
| Constructors `throw DomainValidationException` | Constructors use `Guard.Against.*` |
| `Guard.Against` only in non-domain code | `Guard.Against` in all internal constructors |
