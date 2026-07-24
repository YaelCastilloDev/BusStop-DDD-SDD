---
name: busstop-frontend-architecture
description: BusStop frontend Clean Architecture and Feature-Sliced Design rules. Use when working on frontend architecture, feature slices, domain models, or adapters.
---

# BusStop Frontend Architecture

## Clean Architecture & Feature-Sliced Design (FSD)
The frontend strictly separates concerns to ensure maintainability, testability, and independence from external vendors.

## 1. Feature Slices (`src/features/`)
- Group code by business feature rather than technical type.
- Each feature must be self-contained: it can expose a public API (e.g., `index.ts`) and hide its internals.
- Example: `src/features/map/` containing its own `components/`, `hooks/`, `data/`, `types.ts`, and a barrel `index.ts`.
- **Cross-feature dependencies:** Avoid direct deep imports between features. Always import from the feature's `index.ts`.

## 2. Domain Models
- Domain models and types must be isolated.
- Do not pollute domain interfaces with UI-specific or API-specific fields without mapping.

## 3. Adapters for External Services (`src/lib/adapters/`)
- **Crucial Rule:** No direct imports of heavy external libraries (like Map SDKs, Analytics) outside their designated adapter folders.
- Wrap external services in an Adapter interface. The rest of the app consumes the Adapter, not the external SDK.
- This prevents vendor lock-in and allows swapping out underlying implementations easily.
- **Map providers** (MapLibre GL, Google Maps, Leaflet, etc.) MUST be wrapped behind `IMapAdapter` in `src/lib/adapters/maps/`.
- The app MUST NOT directly import any map-provider SDK outside the adapter folder.
- Map domain types (`Stop`, `Route`, `LatLng`) live in `src/features/map/types.ts` and are consumed by the adapter, not the other way around.
