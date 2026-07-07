import { Suspense, lazy } from 'react'
import type { ClassKey } from 'keycloakify/login'
import DefaultPage from 'keycloakify/login/DefaultPage'
import Template from 'keycloakify/login/Template'
import type { KcContext } from './KcContext'
import { useI18n } from './i18n'

const Login = lazy(() => import('./pages/Login'))
const Register = lazy(() => import('./pages/Register'))
const LoginResetPassword = lazy(() => import('./pages/LoginResetPassword'))
const LoginUpdatePassword = lazy(() => import('./pages/LoginUpdatePassword'))

export default function KcPage(props: { kcContext: KcContext }) {
  const { kcContext } = props
  const { i18n } = useI18n({ kcContext })

  return (
    <Suspense>
      {(() => {
        switch (kcContext.pageId) {
          case 'login.ftl':
            return (
              <Login
                kcContext={kcContext}
                i18n={i18n}
                Template={Template}
                doUseDefaultCss={false}
                classes={classes}
              />
            )
          case 'register.ftl':
            return (
              <Register
                kcContext={kcContext}
                i18n={i18n}
                Template={Template}
                doUseDefaultCss={false}
                classes={classes}
              />
            )
          case 'login-reset-password.ftl':
            return (
              <LoginResetPassword
                kcContext={kcContext}
                i18n={i18n}
                Template={Template}
                doUseDefaultCss={false}
                classes={classes}
              />
            )
          case 'login-update-password.ftl':
            return (
              <LoginUpdatePassword
                kcContext={kcContext}
                i18n={i18n}
                Template={Template}
                doUseDefaultCss={false}
                classes={classes}
              />
            )
          default:
            return (
              <DefaultPage
                kcContext={kcContext}
                i18n={i18n}
                classes={classes}
                Template={Template}
                doUseDefaultCss={true}
              />
            )
        }
      })()}
    </Suspense>
  )
}

const classes = {} satisfies { [key in ClassKey]?: string }
