# BusStop Frontend Design System

## Purpose
Define the canonical design tokens and visual conventions for the BusStop frontend. Every component, feature, and page must reference these tokens. Ad-hoc size, weight, or color combinations are prohibited.

Reference: `.cursor/rules/frontend/busstop-frontend-styling.mdc`
Implementation: `src/BusStop.Frontend/src/styles/tokens.css`

---

## 1. Typography Scale

All type must use the semantic utility classes registered in `tokens.css`. Never combine raw `text-` + `font-` + `leading-` utilities.

| Token | Utility Class | Size | Weight | Line-height | Use |
|---|---|---|---|---|---|
| `--text-h1` | `text-h1` | 2.25rem (36px) | 700 | 2.5rem (40px) | Page titles |
| `--text-h2` | `text-h2` | 1.875rem (30px) | 600 | 2.25rem (36px) | Section headings |
| `--text-h3` | `text-h3` | 1.5rem (24px) | 600 | 2rem (32px) | Panel headings, card titles |
| `--text-h4` | `text-h4` | 1.25rem (20px) | 600 | 1.75rem (28px) | Sub-section headings |
| `--text-body` | `text-body` | 1rem (16px) | 400 | 1.5rem (24px) | Body copy, paragraphs |
| `--text-body-sm` | `text-body-sm` | 0.875rem (14px) | 400 | 1.25rem (20px) | Secondary body, descriptions |
| `--text-caption` | `text-caption` | 0.75rem (12px) | 400 | 1rem (16px) | Captions, footnotes, metadata |
| `--text-label` | `text-label` | 0.9375rem (15px) | 500 | 1.375rem (22px) | Form labels, button text, nav items |

### Usage Rules

- Use the semantic utility class alone: `<h2 className="text-h2">Section</h2>`.
- Do NOT compose raw utilities: `~~className="text-xl font-semibold leading-7"~~`.
- Semantic elements get the appropriate token: `<h1>` gets `text-h1`, `<h2>` gets `text-h2`.
- Labels and interactive text use `text-label`, not `text-body-sm font-medium`.

---

## 2. Spacing Scale

Based on a 4px base unit (0.25rem). Tailwind's default spacing scale is sufficient for most cases. For layout-level spacing (page gutters, section gaps), prefer the named tokens below.

| Token | Value | Use |
|---|---|---|
| `--spacing-section` | 2rem (32px) | Vertical gap between major sections |
| `--spacing-container` | 1.5rem (24px) | Horizontal padding for containers/cards |
| `--spacing-item` | 1rem (16px) | Gap between list items, form fields |
| `--spacing-tight` | 0.5rem (8px) | Gap between related inline elements |
| `--spacing-page` | 2rem (32px) | Page-level horizontal padding |

For all other spacing, use Tailwind's standard scale (`p-2`, `gap-3`, `m-4`, etc.).

---

## 3. Z-Index Layers

Every z-index value must reference one of the layers below. Never use arbitrary `z-[N]` values.

| Layer | Utility | Value | Component |
|---|---|---|---|
| Map | `z-0` | 0 | MapContainer, basemap |
| Content | `z-10` | 10 | Standard page content |
| EntityDetailsPanel | `z-40` | 40 | EntityDetailsPanel |
| TopBar | `z-60` | 60 | TopBar (sticky header) |
| Sidebar | `z-70` | 70 | Sidebar overlay |
| Modal | `z-100` | 100 | Dialog, AlertDialog, Sheet |
| Popover | `z-110` | 110 | Tooltip, DropdownMenu, Popover, Select |
| Toast | `z-120` | 120 | Sonner toasts |

### Rules

- Sidebar overlays the TopBar and EntityDetailsPanel.
- Popovers and tooltips must render above modals.
- Toasts are always the topmost layer.
- If a new component needs a z-index not listed here, the layer must be added to this spec and `tokens.css` first.

---

## 4. Color Semantics

Colors are defined in `src/BusStop.Frontend/src/styles/theme.css` via shadcn/ui CSS variables. Use semantic Tailwind classes exclusively. Never hardcode hex, rgb, or oklch values in components.

| Token | Tailwind Class | Intended Use |
|---|---|---|
| `background` | `bg-background` | Page and card backgrounds |
| `foreground` | `text-foreground` | Primary text |
| `primary` | `bg-primary`, `text-primary` | Primary actions, emphasis |
| `primary-foreground` | `text-primary-foreground` | Text on primary backgrounds |
| `secondary` | `bg-secondary` | Secondary surfaces |
| `muted` | `bg-muted`, `text-muted` | Muted backgrounds |
| `muted-foreground` | `text-muted-foreground` | Secondary/desaturated text |
| `accent` | `bg-accent` | Highlighted surfaces |
| `destructive` | `text-destructive`, `bg-destructive` | Error states, delete actions |
| `border` | `border-border` | Borders, dividers |
| `ring` | `ring-ring` | Focus rings |
| `sidebar` | `bg-sidebar` | Sidebar background |
| `sidebar-foreground` | `text-sidebar-foreground` | Sidebar text |
| `sidebar-accent` | `bg-sidebar-accent` | Active sidebar items |
| `sidebar-border` | `border-sidebar-border` | Sidebar separators |

### Rules

- Use `text-foreground` for default body text, `text-muted-foreground` for secondary text.
- Use `bg-primary text-primary-foreground` for primary buttons.
- Destructive actions use `text-destructive` or `bg-destructive`.
- Sidebar components use the `sidebar-*` prefix tokens.
- The application only supports light theme. CSS variables are defined in `:root`.

---

## 5. Icons

- Use `lucide-react` for all icons.
- Standard icon size: `size-4` (inline with text), `size-5` (standalone), `size-6` (large).
- Icons inside buttons with `size='icon'` use `size-4`.

---

## 6. Border Radius

| Token | Tailwind Class | Use |
|---|---|---|
| `--radius` (0.625rem) | `rounded-lg` | Cards, dialogs, panels |
| `--radius-sm` | `rounded-sm` | Small elements (badges) |
| `--radius-md` | `rounded-md` | Buttons, inputs |
| `--radius-xl` | `rounded-xl` | Large containers |

---

## 7. Shadows

Use Tailwind's built-in shadow scale (`shadow-sm`, `shadow`, `shadow-md`, `shadow-lg`) as defined by the shadcn/ui theme. No custom box-shadow values.

---

## 8. Breakpoints (Mobile-First)

| Breakpoint | Width | Use |
|---|---|---|
| `sm` | 640px | Small tablets |
| `md` | 768px | Tablets, sidebar toggle |
| `lg` | 1024px | Desktop |
| `xl` | 1280px | Wide desktop |

Design mobile-first: default styles target mobile, use `md:`, `lg:`, `xl:` prefixes to scale up.

---

## Non-Negotiables

1. No hardcoded font sizes, weights, or line-heights outside of typography tokens.
2. Use semantic type classes (`text-h1`, `text-body-sm`) instead of ad-hoc `text-xl font-semibold`.
3. Z-index values must reference the defined layer constants from this spec.
4. Colors must use semantic Tailwind classes. No hardcoded hex, rgb, or oklch.
5. No custom CSS files unless explicitly approved for complex animations unsupported by Tailwind.
6. Icons from `lucide-react` only. No inline SVG or icon font imports.
