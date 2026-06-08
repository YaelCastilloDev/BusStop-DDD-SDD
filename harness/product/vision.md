# BusStop - Product Vision

## Purpose
BusStop is a community-maintained platform for discovering and curating public transportation knowledge.
It is built for regions where transit data is fragmented, outdated, or inaccessible.

## Product Thesis
If communities can update route and stop data within their permissions, and higher roles can review and undo changes,
transit information quality improves faster than closed, centrally managed systems.

## Problem
Current transit information ecosystems commonly fail because:
- Data ownership is closed or fragmented across agencies.
- Many cities lack complete digital route and stop catalogs.
- Updates arrive slowly and are hard to verify.
- Existing products rarely support transparent community governance.

## Target Users
- Riders who need reliable route and stop information.
- Contributors who add and edit transportation data.
- Curators and moderators who validate quality and resolve conflicts.
- Regional admins who maintain local governance policies.

## Product Outcomes
- Make route and stop discovery globally accessible.
- Build trust through transparent moderation and edit history.
- Support incremental expansion city by city without redesigning the platform.

## Scope For Initial Rewrite
- Collaborative data management for routes and stops.
- Role-based workflows where changes can be reviewed and undone by higher roles.
- Search and read experiences for published transportation data.
- Auditability for every contribution review and undo decision.

## Core Product Capabilities
1. Search transportation data by location and route identity.
2. Submit create/edit proposals for routes and stops.
3. Review and reverse sensitive or problematic changes through moderation queues.
4. Track contribution history, reviewer decisions, and rationale.
5. Support regional governance without forking domain rules.

## Community Governance Model
Roles:
- User
- Curator
- Moderator
- SubAdmin
- Admin

Governance principles:
- Least privilege by default.
- Every moderation action is auditable.

## DDD Product Language
This vision standardizes these domain terms:
- Route: a transport path with ordered stops.
- Stop: a boarding/alighting point.
- Contribution: a community-submitted create/edit proposal for a route or stop.
- ModerationAction: a review, undo, escalation, or rollback decision.

## Delivery Principles
- Spec-driven development for every feature slice.
- Domain-driven design for boundaries and invariants.
- Small, reviewable increments with explicit acceptance criteria.
- Architecture decisions documented before implementation.