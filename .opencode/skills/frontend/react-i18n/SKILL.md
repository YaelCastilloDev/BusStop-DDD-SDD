---
name: react-i18n
description: i18next internationalization with React. Use when adding translations, localizing UI text, or configuring language detection.
---

# React i18n

Use **i18next** with `react-i18next`. Configuration is at `src/lib/i18n/index.ts`.

## Setup
```tsx
import { useTranslation } from 'react-i18next'

function Component() {
  const { t } = useTranslation('common')
  return <p>{t('save')}</p>
}
```

## Resource Structure
Translation JSON files are **bundled imports** (not lazy loaded), co-located at:
```
src/lib/i18n/resources/
├── en/
│   ├── common.json
│   ├── map.json
│   └── navigation.json
└── es/
    ├── common.json
    ├── map.json
    └── navigation.json
```

### Type Safety
Resources are typed via `declare module 'i18next'`:
```ts
declare module 'i18next' {
  interface CustomTypeOptions {
    resources: typeof resources['en']
  }
}
```

## i18next Configuration
```ts
import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import LanguageDetector from 'i18next-browser-languagedetector'

import enCommon from './resources/en/common.json'
import enMap from './resources/en/map.json'
// ...

const resources = {
  en: { common: enCommon, map: enMap },
  es: { common: esCommon, map: esMap },
} as const

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: 'en',
    defaultNS: 'common',
    ns: ['common', 'map', 'navigation'],
    interpolation: { escapeValue: false },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    },
  })
```

## Namespaces
- `common` — shared UI: buttons, labels, generic text.
- `map` — map-specific translations.
- `navigation` — nav labels, breadcrumbs.
- Always use a namespace: `useTranslation('namespace')`.

## Patterns
- `t('key')` for plain strings.
- `t('key', { count, name })` for interpolation.
- Do **not** use `<Trans>` — plain `t()` is preferred.

## Provider
The `<I18nextProvider>` wraps the app in `src/main.tsx`:
```tsx
<I18nextProvider i18n={i18n}>
  <RouterProvider router={router} />
</I18nextProvider>
```

## Conventions
- All user-facing strings go through `t()`. No hardcoded text.
- Translation key format: `camelCase` or `dot.notation`.
- New namespaces require adding to `ns` array and `CustomTypeOptions` declaration.
- Language detection order: `localStorage` → `navigator`, cached in `localStorage`.
