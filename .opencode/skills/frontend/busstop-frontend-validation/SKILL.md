---
name: busstop-frontend-validation
description: BusStop frontend validation and quality rules (TypeScript strict mode, linting, error handling). Use when ensuring code quality, handling errors, or setting up linting.
---

# BusStop Frontend Validation & Quality

## 1. TypeScript Strict Mode
- Strict mode MUST be enabled.
- Absolute prohibition of the `any` type. Use `unknown` if truly dynamic, but prefer specific types, generics, or Zod schemas.
- Interfaces and types should be strictly defined for all API responses and domain entities.

## 2. Linting & Formatting
- ESLint and Prettier MUST pass on all files.
- Formatting is enforced via Prettier (and its Tailwind plugin for class sorting).
- Pre-commit and pre-merge validation gates will block code that fails linting or type-checking.

## 3. Error & Loading States
- Components must handle loading and error states gracefully.
- No blank screens during data fetching (use Skeletons or Spinners).
- Use TanStack Router's built-in `errorComponent` on the root route for unhandled render errors. Route-level `errorComponent` for feature isolation.
- Global query/mutation errors are handled via `QueryCache.onError` and `mutation.onError` in `QueryClient` config — surfaced as Sonner toasts.
- Do NOT use `react-error-boundary` — TanStack Router boundaries replace it.

## 4. ESLint Enforcement
- `no-console: error` — no `console.log`. Use Sonner toasts or `@/lib/logger`.
- `@typescript-eslint/consistent-type-imports` — types must use `import type { ... }`.
- `@typescript-eslint/no-unused-vars` — unused variables must be prefixed with `_`.
