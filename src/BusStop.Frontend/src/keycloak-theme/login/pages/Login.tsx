import { useState } from 'react'
import type { PageProps } from 'keycloakify/login/pages/PageProps'
import type { KcContext } from '../KcContext'
import type { I18n } from '../i18n'
import { AuthCardLayout } from '../components/AuthCardLayout'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'

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
      footer={
        realm.password && realm.registrationAllowed && !registrationDisabled ? (
          <>
            <span className='text-muted-foreground'>{msgStr('noAccount')}</span>
            <a
              href={url.registrationUrl}
              className='text-primary text-sm font-medium hover:underline'
            >
              {msgStr('doRegister')}
            </a>
          </>
        ) : null
      }
    >
      <div className='flex justify-between gap-8 my-6'>
        <button
          type='button'
          className='px-4 py-2.5 border flex gap-2 items-center w-full rounded-md text-center justify-center text-foreground transition-colors hover:bg-muted'
        >
          <svg width='18' height='18' viewBox='0 0 28 28' fill='none' xmlns='http://www.w3.org/2000/svg'>
            <path d='M27.9851 14.2618C27.9851 13.1146 27.8899 12.2775 27.6837 11.4094H14.2788V16.5871H22.1472C21.9886 17.8738 21.132 19.8116 19.2283 21.1137L19.2016 21.287L23.44 24.4956L23.7336 24.5242C26.4304 22.0904 27.9851 18.5093 27.9851 14.2618Z' fill='#4285F4'/>
            <path d='M14.279 27.904C18.1338 27.904 21.37 26.6637 23.7338 24.5245L19.2285 21.114C18.0228 21.9356 16.4047 22.5092 14.279 22.5092C10.5034 22.5092 7.29894 20.0754 6.15663 16.7114L5.9892 16.7253L1.58205 20.0583L1.52441 20.2149C3.87224 24.7725 8.69486 27.904 14.279 27.904Z' fill='#34A853'/>
            <path d='M6.15656 16.7113C5.85516 15.8432 5.68072 14.913 5.68072 13.9519C5.68072 12.9907 5.85516 12.0606 6.14071 11.1925L6.13272 11.0076L1.67035 7.62109L1.52435 7.68896C0.556704 9.58024 0.00146484 11.7041 0.00146484 13.9519C0.00146484 16.1997 0.556704 18.3234 1.52435 20.2147L6.15656 16.7113Z' fill='#FBBC05'/>
            <path d='M14.279 5.3947C16.9599 5.3947 18.7683 6.52635 19.7995 7.47204L23.8289 3.6275C21.3542 1.37969 18.1338 0 14.279 0C8.69485 0 3.87223 3.1314 1.52441 7.68899L6.14077 11.1925C7.29893 7.82856 10.5034 5.3947 14.279 5.3947Z' fill='#EB4335'/>
          </svg>
          Google
        </button>
        <button
          type='button'
          className='px-4 py-2.5 border flex gap-2 items-center w-full rounded-md text-center justify-center text-foreground transition-colors hover:bg-muted'
        >
          <svg width='18' height='18' viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
            <path d='M24 12.073C24 5.405 18.627 0 12 0C5.373 0 0 5.405 0 12.073C0 18.1 4.388 23.094 10.125 24V15.562H7.078V12.073H10.125V9.414C10.125 6.41 11.916 4.75 14.656 4.75C15.969 4.75 17.344 4.985 17.344 4.985V7.938H15.83C14.339 7.938 13.875 8.863 13.875 9.816V12.073H17.203L16.67 15.562H13.875V24C19.612 23.094 24 18.1 24 12.073Z' fill='#1877F2'/>
          </svg>
          Facebook
        </button>
      </div>

      <div className='flex items-center justify-center gap-2'>
        <hr className='grow border' />
        <p className='text-base text-foreground font-medium'>
          {(() => {
            const text = msgStr('orSignInWith')
            return text !== 'orSignInWith' ? text : 'or sign in with'
          })()}
        </p>
        <hr className='grow border' />
      </div>

      {realm.password ? (
        <form
          id='kc-form-login'
          action={url.loginAction}
          method='post'
          className='flex flex-col mt-6'
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
                autoComplete='username'
                defaultValue={login.username ?? ''}
                aria-invalid={usernameError || undefined}
                className='h-10 rounded-lg py-2 shadow-none text-sm !border focus-visible:outline-0 focus-visible:ring-0 focus-visible:border-primary'
              />
              {usernameError ? (
                <p className='text-sm text-destructive mt-1' role='alert'>
                  {msgStr(messagesPerField.getFirstError('username') ?? '')}
                </p>
              ) : null}
            </div>
          ) : null}

          <div className='mb-4'>
            <div className='mb-2 block'>
              <Label htmlFor='password' className='font-semibold'>{msgStr('password')}</Label>
            </div>
            <Input
              id='password'
              name='password'
              type='password'
              autoComplete='current-password'
              aria-invalid={passwordError || undefined}
              className='h-10 rounded-lg py-2 shadow-none text-sm !border focus-visible:outline-0 focus-visible:ring-0 focus-visible:border-primary'
            />
            {passwordError ? (
              <p className='text-sm text-destructive mt-1' role='alert'>
                {msgStr(messagesPerField.getFirstError('password') ?? '')}
              </p>
            ) : null}
          </div>

          <div className='flex justify-between items-center my-5'>
            <div className='flex items-center gap-2'>
              <input
                id='rememberMe'
                name='rememberMe'
                type='checkbox'
                defaultChecked={login.rememberMe}
                className='h-4 w-4 rounded border text-primary focus:ring-primary bg-transparent'
              />
              <Label htmlFor='rememberMe' className='opacity-90 font-normal cursor-pointer text-label'>
                {msgStr('rememberMe')}
              </Label>
            </div>

            {realm.resetPasswordAllowed ? (
              <a
                href={url.loginResetCredentialsUrl}
                className='text-primary text-sm font-medium hover:underline'
              >
                {msgStr('doForgotPassword')}
              </a>
            ) : null}
          </div>

          <div id='kc-form-buttons'>
            <Button
              type='submit'
              className='w-full h-10 px-5 py-2 text-sm font-medium shadow-none transition-colors hover:bg-primary-emphasis focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 ring-offset-background'
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
