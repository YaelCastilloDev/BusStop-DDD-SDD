import { i18nBuilder } from 'keycloakify/login'

const { useI18n, ofTypeI18n: _ofTypeI18n } = i18nBuilder
  .withCustomTranslations({
    en: {
      alreadyHaveAccount: 'Already have an account?',
      orSignInWith: 'Or sign in with',
    },
    es: {
      alreadyHaveAccount: '¿Ya tienes una cuenta?',
      orSignInWith: 'O inicia sesión con',
    },
  })
  .build()

type I18n = typeof _ofTypeI18n

export { useI18n, type I18n }
