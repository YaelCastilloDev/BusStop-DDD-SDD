---
name: busstop-frontend-harness
description: BusStop frontend development harness and architectural guidelines. Use first before any frontend implementation work.
---

# BusStop Frontend Harness

## Purpose
Define the architectural guidelines and strict rules for frontend development in the BusStop application. This harness applies Feature-Sliced Design (FSD) and Clean Architecture principles to the frontend.

## Core Pillars
All agents and developers MUST follow the rules outlined in the specific domain rule files:
- **Architecture:** `frontend/busstop-frontend-architecture`
- **State Management:** `frontend/busstop-frontend-state`
- **Styling & Theming:** `frontend/busstop-frontend-styling`
- **Components:** `frontend/busstop-frontend-components`
- **Routing:** `frontend/busstop-frontend-routing`
- **Validation & Quality:** `frontend/busstop-frontend-validation`
- **Authentication:** `frontend/busstop-frontend-auth`
- **Internationalization:** `frontend/busstop-frontend-i18n`

## Mandatory Directory Structure
- `src/features/`: Smart components, feature-specific logic, state, and domain models.
- `src/components/ui/`: Dumb, reusable presentation components (shadcn/ui).
- `src/lib/adapters/`: Adapters wrapping external services (Maps, Analytics, Auth) to prevent vendor lock-in.
- `src/lib/i18n/`: i18next initialization and locale resource files organized by language namespace.
- `src/routes/`: TanStack Router file-based routing.
- `src/config/`: App-wide configurations (e.g., fonts, constants).
