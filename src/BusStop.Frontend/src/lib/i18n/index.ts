import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import LanguageDetector from 'i18next-browser-languagedetector'

import enCommon from './resources/en/common.json'
import enMap from './resources/en/map.json'
import enNavigation from './resources/en/navigation.json'
import esCommon from './resources/es/common.json'
import esMap from './resources/es/map.json'
import esNavigation from './resources/es/navigation.json'

const resources = {
  en: {
    common: enCommon,
    map: enMap,
    navigation: enNavigation,
  },
  es: {
    common: esCommon,
    map: esMap,
    navigation: esNavigation,
  },
} as const

declare module 'i18next' {
  interface CustomTypeOptions {
    resources: typeof resources['en']
  }
}

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: 'en',
    defaultNS: 'common',
    ns: ['common', 'map', 'navigation'],
    interpolation: {
      escapeValue: false,
    },
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    },
  })

export default i18n
