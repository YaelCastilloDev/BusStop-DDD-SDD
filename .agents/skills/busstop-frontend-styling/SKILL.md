---
name: busstop-frontend-styling
description: BusStop frontend styling and theming rules using Tailwind CSS and design tokens. Use when styling components, working with themes, or defining colors/typography.
---

# BusStop Frontend Styling & Theming

Canonical source: `harness/specs/frontend-design-system.md`
Implementation: `src/BusStop.Frontend/src/styles/tokens.css`

## 1. Utility-First CSS (Tailwind)
- Strict use of Tailwind CSS utility classes.
- Avoid writing custom CSS. Prohibition of custom CSS files unless absolutely necessary (e.g., complex animations not supported by Tailwind).
- Use `tailwind-merge` and `clsx` (or `cn` utility) for dynamic class name merging.

## 2. Design Tokens (Non-Negotiable)
- All typography MUST use the semantic utility classes defined in `tokens.css` (`text-h1`, `text-h2`, `text-h3`, `text-h4`, `text-body`, `text-body-sm`, `text-caption`, `text-label`).
- Do NOT compose ad-hoc size+weight+line-height combinations (`text-xl font-semibold leading-7`). Use the token instead (`text-h3`).
- Z-index values MUST reference the layer constants from `tokens.css` (`z-10`, `z-40`, `z-60`, `z-70`, `z-100`, `z-110`, `z-120`). No arbitrary `z-[N]` values.
- Layout spacing uses named tokens where defined (`p-section`, `gap-container`). Fall back to Tailwind's standard scale for fine-grained spacing.

## 3. Theming & Dark Mode
- Use CSS variables (`var(--color-...)`) defined in global CSS for colors.
- Support Light and Dark mode seamlessly via these variables and Tailwind's dark mode strategies.
- Do not hardcode specific hex colors in components; use semantic Tailwind classes (e.g., `bg-primary`, `text-muted-foreground`).
- Color semantics defined in `harness/specs/frontend-design-system.md`.

## 4. Typography
- Typography rules and fonts MUST be centrally defined.
- Fonts must be loaded and configured in `src/config/fonts.ts`.
- Use the semantic typography utility classes from `tokens.css`. Never use raw `text-`, `font-`, `leading-` combinations.
