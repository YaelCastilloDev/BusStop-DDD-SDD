import { useState } from 'react'
import { useInitialize } from 'keycloakify/login/Template.useInitialize'
import type { PageProps } from 'keycloakify/login/pages/PageProps'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import type { KcContext } from '../KcContext'
import { AuthCardLayout } from '../components/AuthCardLayout'
import type { I18n } from '../i18n'

type LoginProps = PageProps<Extract<KcContext, { pageId: 'login.ftl' }>, I18n>

export default function Login({
  kcContext,
  i18n,
  doUseDefaultCss,
}: LoginProps) {
  const { isReadyToRender } = useInitialize({ kcContext, doUseDefaultCss })
  const { advancedMsgStr, msgStr } = i18n
  const {
    realm,
    url,
    usernameHidden,
    login,
    registrationDisabled,
    messagesPerField,
    message,
    social,
    auth,
    enableWebAuthnConditionalUI,
  } = kcContext
  const [isLoginButtonDisabled, setIsLoginButtonDisabled] = useState(false)
  const providers = social?.providers ?? []
  const usernameError = messagesPerField.existsError('username')
  const passwordError = messagesPerField.existsError('password')

  if (!isReadyToRender) return null

  return (
    <AuthCardLayout
      footer={
        realm.password && realm.registrationAllowed && !registrationDisabled ? (
          <>
            <span className='text-muted-foreground'>{msgStr('noAccount')}</span>
            <a
              href={url.registrationUrl}
              className='text-sm font-medium text-primary hover:underline'
            >
              {msgStr('doRegister')}
            </a>
          </>
        ) : null
      }
    >
      {message ? (
        <p
          className={
            message.type === 'error'
              ? 'mb-4 text-sm text-destructive'
              : 'mb-4 text-sm text-muted-foreground'
          }
          role={message.type === 'error' ? 'alert' : 'status'}
        >
          {message.summary}
        </p>
      ) : null}

      {providers.length > 0 ? (
        <>
          <div className='my-6 grid gap-3 sm:grid-cols-2'>
            {providers.map((provider) => (
              <a
                key={provider.alias}
                href={provider.loginUrl}
                className='flex w-full items-center justify-center gap-2 rounded-md border px-4 py-2.5 text-center text-foreground transition-colors hover:bg-muted'
              >
                {provider.iconClasses ? (
                  <i aria-hidden='true' className={provider.iconClasses} />
                ) : null}
                {provider.displayName}
              </a>
            ))}
          </div>
          <div className='flex items-center justify-center gap-2'>
            <hr className='grow border' />
            <p className='text-base font-medium text-foreground'>
              {msgStr('orSignInWith')}
            </p>
            <hr className='grow border' />
          </div>
        </>
      ) : null}

      {realm.password ? (
        <form
          id='kc-form-login'
          action={url.loginAction}
          method='post'
          className='mt-6 flex flex-col'
          onSubmit={() => setIsLoginButtonDisabled(true)}
        >
          {!usernameHidden ? (
            <div className='mb-4'>
              <div className='mb-2 block'>
                <Label htmlFor='username' className='font-semibold'>
                  {!realm.loginWithEmailAllowed
                    ? msgStr('username')
                    : realm.registrationEmailAsUsername
                      ? msgStr('email')
                      : msgStr('usernameOrEmail')}
                </Label>
              </div>
              <Input
                id='username'
                name='username'
                type='text'
                autoFocus
                autoComplete={
                  enableWebAuthnConditionalUI ? 'username webauthn' : 'username'
                }
                defaultValue={login.username ?? ''}
                aria-invalid={usernameError || undefined}
                className='h-10 rounded-lg !border py-2 text-sm shadow-none focus-visible:border-primary focus-visible:ring-0 focus-visible:outline-0'
              />
              {usernameError ? (
                <p className='mt-1 text-sm text-destructive' role='alert'>
                  {advancedMsgStr(
                    messagesPerField.getFirstError('username') ?? ''
                  )}
                </p>
              ) : null}
            </div>
          ) : null}

          <div className='mb-4'>
            <div className='mb-2 block'>
              <Label htmlFor='password' className='font-semibold'>
                {msgStr('password')}
              </Label>
            </div>
            <Input
              id='password'
              name='password'
              type='password'
              autoComplete='current-password'
              aria-invalid={passwordError || undefined}
              className='h-10 rounded-lg !border py-2 text-sm shadow-none focus-visible:border-primary focus-visible:ring-0 focus-visible:outline-0'
            />
            {passwordError ? (
              <p className='mt-1 text-sm text-destructive' role='alert'>
                {advancedMsgStr(
                  messagesPerField.getFirstError('password') ?? ''
                )}
              </p>
            ) : null}
          </div>

          <div className='my-5 flex items-center justify-between'>
            {realm.rememberMe ? (
              <div className='flex items-center gap-2'>
                <input
                  id='rememberMe'
                  name='rememberMe'
                  type='checkbox'
                  defaultChecked={!!login.rememberMe}
                  className='size-4 rounded border bg-transparent text-primary focus:ring-primary'
                />
                <Label
                  htmlFor='rememberMe'
                  className='cursor-pointer text-label font-normal opacity-90'
                >
                  {msgStr('rememberMe')}
                </Label>
              </div>
            ) : (
              <span />
            )}

            {realm.resetPasswordAllowed ? (
              <a
                href={url.loginResetCredentialsUrl}
                className='text-sm font-medium text-primary hover:underline'
              >
                {msgStr('doForgotPassword')}
              </a>
            ) : null}
          </div>

          <Button
            id='kc-login'
            name='login'
            type='submit'
            className='h-10 w-full px-5 py-2 text-sm font-medium shadow-none ring-offset-background transition-colors hover:bg-primary-emphasis focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2'
            disabled={isLoginButtonDisabled}
          >
            {msgStr('doLogIn')}
          </Button>
          <input
            id='id-hidden-input'
            name='credentialId'
            type='hidden'
            value={auth.selectedCredential}
          />
        </form>
      ) : null}
    </AuthCardLayout>
  )
}
