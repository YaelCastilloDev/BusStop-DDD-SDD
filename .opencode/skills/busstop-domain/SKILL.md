---
name: busstop-domain
description: BusStop domain glossary and bounded contexts. Use when working with BusStop domain concepts, entities, or bounded contexts.
---

# BusStop Domain

Full specs: `harness/product/vision.md`, `harness/system-design.md`.

## Domain Glossary (canonical terms)
- **Route:** a transport path with ordered stops.
- **Stop:** a boarding/alighting point.
- **ModerationAction:** a review, undo, escalation, or rollback decision.

Do not use synonyms that alter meaning unless the glossary is updated first.

## Bounded Contexts
- **TransitCatalog:** route/stop lifecycle, validation, and moderation.
- **IdentityAccess:** authentication, role claims, policy enforcement.
- **SearchReadModel:** optimized querying and indexing projections.
- **AuditObservability:** immutable action logs and operational signals.

## Context Rules
- Each feature belongs to exactly one primary bounded context.
- No direct cross-context data writes.
- Contexts start as folders/modules inside `BusStop.*` projects.
- TransitCatalog publishes events consumed by SearchReadModel and AuditObservability.

## Governance Roles
User, Curator, Moderator, SubAdmin, Admin — least privilege by default.
