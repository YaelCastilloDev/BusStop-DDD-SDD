---
name: busstop-frontend-state
description: BusStop frontend state management rules using TanStack React Query, Zustand, and React hooks. Use when managing server state, global UI state, or local state.
---

# BusStop Frontend State Management

## 1. Server State: TanStack React Query
- MUST be used for all API calls, data fetching, caching, and revalidation.
- Do not use `useEffect` + `useState` for API calls.
- Separate queries and mutations into custom hooks (e.g., `useGetRoutes`, `useCreateStop`).
- Keep query keys organized, preferably using a query key factory.

## 2. Global UI State: Zustand
- Use Zustand for application-wide UI state (e.g., sidebar open/closed, current active theme, selected map entity).
- Create small, focused stores rather than one monolithic store.
- Do NOT use Zustand for server data. Server data belongs in React Query.

## 3. Local State: React Hooks
- Use `useState` and `useReducer` for state that is strictly confined to a single component or its immediate children.
- Examples: form input values (before submission), local toggle states (dropdowns), component-specific loading spinners.
