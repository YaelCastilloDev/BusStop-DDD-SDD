---
name: busstop-frontend-i18n
description: BusStop frontend internationalization rules using i18next. Use when adding translations, locale files, or new languages.
---

# BusStop Frontend Internationalization (i18n)

## 1. i18next & Namespaces
- Use `react-i18next` with namespace-based organization for scalability.
- Each feature domain gets its own namespace (e.g., `map`, `auth`, `common`).
- No hardcoded strings in components — always use `useTranslation('namespace')` + `t('key')`.
- The `useTranslation` hook must specify the primary namespace: `useTranslation('map')`.
- Resources are **bundled JSON imports** (not lazy loaded from `/public`). Each language/namespace combo is imported in `src/lib/i18n/index.ts`.
- Type-safe keys via `declare module 'i18next'` and `CustomTypeOptions` in `index.ts`.

## 2. Language Detection
- Auto-detect via `i18next-browser-languagedetector`.
- Detection order: localStorage, then navigator.
- Default: English (`en`), fallback: English (`en`).
- Store user preference in localStorage.

## 3. File Convention
- Locale files live in `src/lib/i18n/resources/{lang}/{namespace}.json`.
- New features must add locale keys to at minimum `en/` first.
- JSON structure must be flat or one level deep — no deep nesting.

## 4. Scalability
- Add new languages by creating a new folder under `resources/`.
- Always mirror the keys from `en/` in any new language.
- The i18n init module (`src/lib/i18n/index.ts`) is the single source of truth for configuration.
