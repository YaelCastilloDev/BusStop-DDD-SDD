---
name: busstop-frontend-components
description: BusStop frontend component guidelines using shadcn/ui. Use when building UI components, smart/dumb separation, typography roles, accessibility, or GIS components.
---

# BusStop Frontend Component Guidelines

## 1. Component Library (shadcn/ui)
- Use `shadcn/ui` as the base component library.
- Place all primitive UI components in `src/components/ui/`.
- Extend or modify these components strictly through Tailwind classes and the `cn` utility.

## 2. Separation of Concerns (Smart vs. Dumb)
- **Dumb (Presentational) Components:** Receive data via props, emit events via callbacks. No knowledge of React Query, Zustand, or business logic. Highly reusable.
- **Smart (Container/Feature) Components:** Connect to React Query, read from Zustand, and pass data down to Dumb components. Located in `src/features/`.

## 3. Typography Roles
Map component roles to semantic type tokens from `harness/specs/frontend-design-system.md`:

| Component Role | Type Token |
|---|---|
| Page title | `text-h1` |
| Section heading | `text-h2` |
| Panel heading, card title, dialog title | `text-h3` |
| Sub-section heading | `text-h4` |
| Body copy, paragraphs, descriptions | `text-body` or `text-body-sm` |
| Labels, button text, nav items, form labels | `text-label` |
| Captions, footnotes, metadata, version info | `text-caption` |

Rules:
- `<h1>` elements must use `text-h1`. `<h2>` must use `text-h2`.
- Sidebar navigation items use `text-label`.
- Form labels and input placeholders use `text-label`.
- Version or metadata text uses `text-caption`.
- Never compose raw `text-sm font-medium` — use the semantic token instead.

## 4. Accessibility (a11y) & Responsiveness
- All components MUST be accessible. Use Radix UI primitives (which back shadcn) to ensure keyboard navigation and ARIA attributes.
- Enforce a mobile-first approach. Design the default view for mobile and use Tailwind's `md:`, `lg:`, `xl:` breakpoints to scale up.

## 5. GIS & Map Components
- All map interactions go through `src/lib/adapters/maps/IMapAdapter` — never import MapLibre GL (or any map SDK) directly in React components.
- Markers and popups must use domain types (Stop, Route) from `src/features/map/types.ts`, not map-provider-specific types.
- Responsive layout for details panels: side panel on desktop (>= md breakpoint), bottom sheet (vaul Drawer) on mobile.
- Map container must fill available viewport space using `absolute inset-0` or flex layout with `flex-1`.
- Z-index hierarchy: Sidebar (z-70) > TopBar (z-60) > EntityDetailsPanel (z-40) > Content (z-10) > Map (z-0).
