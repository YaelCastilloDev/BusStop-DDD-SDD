---
name: react-data
description: TanStack Query v5 patterns for server state. Use when implementing API calls, data fetching, mutations, or cache management.
---

# React Data (Server State)

Use **TanStack Query v5** for all server state. The `QueryClient` is configured globally in `src/main.tsx`.

## QueryClient Configuration
```tsx
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: (_failureCount, error) => {
        if (import.meta.env.DEV) return false
        return !(error instanceof AxiosError && [401, 403].includes(error.response?.status ?? 0))
      },
      refetchOnWindowFocus: import.meta.env.PROD,
      staleTime: 10 * 1000,
    },
    mutations: {
      onError: (error) => {
        if (error instanceof AxiosError) {
          if (error.response?.status === 304) toast.error('Content not modified!')
        }
      },
    },
  },
  queryCache: new QueryCache({
    onError: (error) => {
      if (error instanceof AxiosError) {
        if (error.response?.status === 401) toast.error('Session expired!')
        if (error.response?.status === 500) toast.error('Internal Server Error!')
      }
    },
  }),
})
```

Key behaviors:
- **Retry disabled in dev** (`import.meta.env.DEV`).
- **No retry on 401/403** — auth errors surface immediately.
- **`refetchOnWindowFocus`** only in production.
- **`staleTime: 10s`** — data considered fresh for 10 seconds.
- **Global error toasts** via Sonner for 304, 401, 500.

## Query Pattern
```tsx
import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api'
import type { Route } from '@/features/map/types'

export function useRoutes() {
  return useQuery({
    queryKey: ['routes'],
    queryFn: () => api.get<Route[]>('/routes').then((r) => r.data),
  })
}
```

## Mutation Pattern
```tsx
import { useMutation, useQueryClient } from '@tanstack/react-query'

export function useCreateRoute() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateRoute) => api.post('/routes', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['routes'] })
    },
  })
}
```

## Query Key Convention
- Hierarchical: `['routes']`, `['routes', id]`, `['routes', id, 'stops']`.
- Keys are global — be specific to avoid collisions across features.

## Current State
- **Mock data** is used in `features/map/data/` as transitional data. Replace with `useQuery` / `useMutation` hooks in `features/{name}/hooks/queries/` and `features/{name}/hooks/mutations/`.

## Conventions
- Extract query/mutation hooks into feature-level `hooks/queries/` and `hooks/mutations/`.
- Use `AxiosError` type guards for error handling (matching global config).
- `useSuspenseQuery` preferred for route-level data loading when Suspense is adopted.
- `@/` alias for all internal imports.
- Type-only imports: `import type { ... }` for TypeScript types.
