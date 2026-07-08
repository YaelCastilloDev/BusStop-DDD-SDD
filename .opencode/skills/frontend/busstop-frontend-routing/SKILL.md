---
name: busstop-frontend-routing
description: BusStop frontend routing rules using TanStack Router. Use when defining routes, route guards, data loading, or search params.
---

# BusStop Frontend Routing

## 1. File-Based Routing (TanStack Router)
- Strict usage of TanStack Router for file-based routing.
- All route definitions must reside within `src/routes/`.
- Rely on code-generated route trees for strict type safety in links and navigation.

## 2. Route Guards & Data Loading
- Use route `beforeLoad` or loaders for authentication checks and pre-fetching critical data.
- Manage route-level error boundaries and pending states using TanStack Router's built-in `errorComponent` and `pendingComponent`.

## 3. Search Params
- Define search parameters strictly using Zod or TanStack Router's built-in validation schemas.
- Treat URL search parameters as a form of global state when appropriate (e.g., filtering, pagination).
