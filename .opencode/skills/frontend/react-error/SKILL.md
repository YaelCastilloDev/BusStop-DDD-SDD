---
name: react-error
description: Error handling patterns using TanStack Router error boundaries and TanStack Query global error handlers. Use when implementing fault isolation, error recovery, or error toasts.
---

# React Error Handling

This project does **not** use `react-error-boundary`. Error handling is done through:

## 1. TanStack Router `errorComponent`

The root route defines a native error boundary:

```tsx
// routes/__root.tsx
function ErrorFallback() {
  return (
    <div className='flex h-svh flex-col items-center justify-center gap-4'>
      <h1 className='text-4xl font-bold'>Error</h1>
      <p className='text-muted-foreground'>Something went wrong</p>
      <Button asChild variant='outline'>
        <Link to='/'>Go Home</Link>
      </Button>
    </div>
  )
}

export const Route = createRootRouteWithContext<{
  queryClient: QueryClient
}>()({
  errorComponent: ErrorFallback,
  notFoundComponent: NotFound,
})
```

- `errorComponent` catches unhandled render errors and shows a fallback UI.
- `notFoundComponent` handles 404s via TanStack Router's built-in not-found detection.

## 2. TanStack Query Global `onError`

Configured in `src/main.tsx` on the `QueryClient`:

### QueryCache (query errors)
```tsx
queryCache: new QueryCache({
  onError: (error) => {
    if (error instanceof AxiosError) {
      if (error.response?.status === 401) toast.error('Session expired!')
      if (error.response?.status === 500) toast.error('Internal Server Error!')
    }
  },
})
```

### Mutations (default mutation error handler)
```tsx
defaultOptions: {
  mutations: {
    onError: (error) => {
      if (error instanceof AxiosError) {
        if (error.response?.status === 304) toast.error('Content not modified!')
      }
    },
  },
}
```

## 3. Query Retry Strategy
```tsx
queries: {
  retry: (_failureCount, error) => {
    if (import.meta.env.DEV) return false
    return !(error instanceof AxiosError && [401, 403].includes(error.response?.status ?? 0))
  },
}
```
- No retries in development.
- No retries on auth errors (401/403) in production.
- All other errors get default retry behavior.

## 4. Axios Error Handling Pattern
All HTTP errors are `AxiosError` instances. Use type guards:
```tsx
if (error instanceof AxiosError) {
  error.response?.status // number
  error.message // string
}
```

## Conventions
- Toast errors via Sonner (`toast.error()`), not inline error messages.
- `errorComponent` on root route catches unhandled render failures.
- Individual query/mutation `onError` handlers for feature-specific recovery.
- Never silently swallow errors — always surface via toast or fallback UI.
