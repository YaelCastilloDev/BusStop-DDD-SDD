import { useState } from 'react'
import type { PageProps } from 'keycloakify/login/pages/PageProps'
import type { KcContext } from '../KcContext'
import type { I18n } from '../i18n'
import { AuthCardLayout } from '../components/AuthCardLayout'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'

type LoginProps = PageProps<Extract<KcContext, { pageId: 'login.ftl' }>, I18n>

export default function Login(props: LoginProps) {
  const { kcContext, i18n } = props
  const { msg, msgStr } = i18n
  const {
    realm,
    url,
    usernameHidden,
    login,
    registrationDisabled,
    messagesPerField,
  } = kcContext

  const [isLoginButtonDisabled, setIsLoginButtonDisabled] = useState(false)

  const usernameError = messagesPerField.existsError('username')
  const passwordError = messagesPerField.existsError('password')

  return (
    <AuthCardLayout
      title={msgStr('loginAccountTitle')}
      description={msgStr('loginAccountSubtitle') !== 'loginAccountSubtitle' ? msgStr('loginAccountSubtitle') : undefined}
      footer={
        realm.password && realm.registrationAllowed && !registrationDisabled ? (
          <div className='text-center text-label'>
            <span className='text-muted-foreground'>{msgStr('noAccount')} </span>
            <a
              href={url.registrationUrl}
              className='font-medium text-primary hover:underline'
            >
              {msgStr('doRegister')}
            </a>
          </div>
        ) : null
      }
    >
      {realm.password ? (
        <form
          id='kc-form-login'
          action={url.loginAction}
          method='post'
          className='flex flex-col gap-4'
          onSubmit={() => setIsLoginButtonDisabled(true)}
        >
          {!usernameHidden ? (
            <div className='space-y-2'>
              <Label htmlFor='username'>
                {!realm.loginWithEmailAllowed
                  ? msgStr('username')
                  : realm.registrationEmailAsUsername
                    ? msgStr('email')
                    : msgStr('usernameOrEmail')}
              </Label>
              <Input
                id='username'
                name='username'
                type='text'
                autoFocus
                autoComplete='username'
                defaultValue={login.username ?? ''}
                aria-invalid={usernameError || undefined}
              />
              {usernameError ? (
                <p className='text-sm text-destructive' role='alert'>
                  {msgStr(messagesPerField.getFirstError('username') ?? '')}
                </p>
              ) : null}
            </div>
          ) : null}

          <div className='space-y-2'>
            <Label htmlFor='password'>{msgStr('password')}</Label>
            <Input
              id='password'
              name='password'
              type='password'
              autoComplete='current-password'
              aria-invalid={passwordError || undefined}
            />
            {passwordError ? (
              <p className='text-sm text-destructive' role='alert'>
                {msgStr(messagesPerField.getFirstError('password') ?? '')}
              </p>
            ) : null}
          </div>

          <div className='flex items-center justify-between'>
            <div className='flex items-center gap-2'>
              <Checkbox id='rememberMe' name='rememberMe' defaultChecked={login.rememberMe} />
              <Label htmlFor='rememberMe' className='text-label'>
                {msgStr('rememberMe')}
              </Label>
            </div>

            {realm.resetPasswordAllowed ? (
              <a
                href={url.loginResetCredentialsUrl}
                className='text-label text-primary hover:underline'
              >
                {msgStr('doForgotPassword')}
              </a>
            ) : null}
          </div>

          <div id='kc-form-buttons' className='pt-2'>
            <Button
              type='submit'
              className='w-full'
              size='lg'
              disabled={isLoginButtonDisabled}
            >
              {msgStr('doLogIn')}
            </Button>
          </div>
        </form>
      ) : null}
    </AuthCardLayout>
  )
}
