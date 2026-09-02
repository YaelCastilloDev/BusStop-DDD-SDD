import { i18nBuilder } from 'keycloakify/login'

const { useI18n, ofTypeI18n: _ofTypeI18n } = i18nBuilder.build()

type I18n = typeof _ofTypeI18n

export { useI18n, type I18n }
