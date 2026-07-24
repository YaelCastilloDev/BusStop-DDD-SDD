---
name: react-frontend
description: General React frontend patterns and conventions. Use when building components, structuring features, or integrating with the BusStop backend API.
---

# React Frontend General

React 19 + TypeScript 6 + Vite 8 project at `src/BusStop.Frontend/`.

## Project Structure
```
src/
├── components/
│   ├── auth/           # Auth-specific components (form-field, submit-button, etc.)
│   └── ui/             # shadcn/ui primitives (button, input, form, dialog, etc.)
├── config/             # App configuration (fonts, etc.)
├── context/            # React context providers (direction, font)
├── features/
│   └── map/            # Feature slice per domain
│       ├── components/ # Feature-specific components
│       ├── data/       # Mock data / API types (transitional)
│       ├── hooks/      # Feature-level hooks
│       ├── index.ts    # Barrel export
│       ├── map-page.tsx
│       └── types.ts    # Feature types
├── hooks/              # Shared hooks (use-auth-redirect, use-mobile)
├── lib/
│   ├── adapters/       # Adapter pattern: auth/ (Keycloak), maps/ (MapLibre)
│   ├── i18n/           # i18next configuration + resources
│   ├── cookies.ts      # Cookie utilities
│   ├── logger.ts       # Logging utilities
│   └── utils.ts        # cn() helper, general utilities
├── routes/             # TanStack Router file-based routes
├── stores/             # Zustand global stores (map-ui-store)
└── styles/             # Tailwind CSS entry point
```

## Component Conventions
- **shadcn/ui components** in `@/components/ui/` — use `<Button>`, `<Input>`, `<Label>`, etc. Never raw HTML for styled elements.
- Auth components in `@/components/auth/` (form-field, submit-button, section-divider).
- **Default export** for page-level components (e.g., `MapPage`).
- **Named exports** for shared components (e.g., `TopBar`, `MainSidebar`).
- **Barrel exports** via `index.ts` per feature: `export { MapLayout } from './components/map-layout'`.

## Styling
- **Tailwind CSS v4** via `@tailwindcss/vite`.
- **shadcn/ui** components with `class-variance-authority`.
- `cn()` from `@/lib/utils` for className merging (`clsx` + `tailwind-merge`).
- Tailwind classes in JSX: `className='flex h-svh flex-col items-center gap-4'`.
- Data attributes for state-driven styles: `data-slot='form-item'`, `data-[error=true]:text-destructive`.

## Adapter Pattern
External services use an adapter interface for testability and swappability:
```
lib/adapters/auth/
├── IAuthAdapter.ts          # Interface
├── KeycloakAdapter.ts       # Concrete implementation
├── adapter-instance.ts      # Singleton accessor + auth token
├── auth-store.ts            # Zustand store for auth state
├── useAuth.ts               # Hook wrapping adapter + store
├── types.ts                 # Auth-specific types
└── index.ts                 # Barrel export
```

## State Management
- **Server state**: TanStack Query (via `QueryClientProvider`).
- **Client state**: Zustand stores (`create<T>()`).
  - `auth-store.ts` — auth user, loading, error state.
  - `map-ui-store.ts` — selected entity, panel open, sidebar, interaction mode.

### Zustand Pattern
```ts
import { create } from 'zustand'

interface MyStore {
  value: boolean
  setValue: (v: boolean) => void
}

export const useMyStore = create<MyStore>()((set) => ({
  value: false,
  setValue: (v) => set({ value: v }),
}))
```
- Prefer selector pattern: `useMyStore((s) => s.value)` — avoids unnecessary re-renders.
- Cookie persistence for UI preferences (sidebar collapsed).

## Axios / HTTP
- Axios configured with request interceptors in `src/main.tsx`.
- Auth token attached automatically: `config.headers.Authorization = 'Bearer ${token}'`.
- All API calls go through Axios, not `fetch`.
- Error handling: `import { AxiosError, type InternalAxiosRequestConfig } from 'axios'`.

## ESLint Conventions
```js
// eslint.config.js
'no-console': 'error',
'@typescript-eslint/no-unused-vars': ['error', {
  argsIgnorePattern: '^_',
  caughtErrorsIgnorePattern: '^_',
  varsIgnorePattern: '^_',
}],
'@typescript-eslint/consistent-type-imports': ['error', {
  prefer: 'type-imports',
  fixStyle: 'inline-type-imports',
}],
'no-duplicate-imports': 'error',
```
- **No `console.log`** — use `@/lib/logger` or Sonner toasts.
- **Unused vars** prefixed with `_`.
- **Type imports**: `import type { Foo } from '...'` enforced.

## Prettier / Code Style
```jsonc
// .prettierrc
"semi": false,              // No semicolons
"singleQuote": true,        // Single quotes
"jsxSingleQuote": true,     // Single quotes in JSX
"tabWidth": 2,
"printWidth": 80,
"trailingComma": "es5",
```
- Import order: `path` → `vite` → `react` → `zod` → `axios` → `@radix-ui/*` → `@tanstack/*` → `@/` aliases → relative.
- Tailwind class sorting via `prettier-plugin-tailwindcss`.

## Testing
- **Vitest** with browser mode (Playwright / Chromium).
- Test files: `*.test.ts` co-located with source.
- `src/test-utils/` for shared test utilities.
- Coverage excludes: `components/ui/`, `routeTree.gen.ts`, `routes/`.

## Forbidden
- `console.log` / `console.error` — use `logger` or Sonner toasts.
- Direct DOM manipulation.
- `useEffect` for data fetching — use `useQuery`.
- Hardcoded text strings — use `t()`.
- Redux or other state libraries for server state — TanStack Query only.
- Mixing React Router with TanStack Router.
