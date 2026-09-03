import { useInitialize } from 'keycloakify/login/Template.useInitialize'
import type { PageProps } from 'keycloakify/login/pages/PageProps'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import type { KcContext } from '../KcContext'
import { AuthCardLayout } from '../components/AuthCardLayout'
import type { I18n } from '../i18n'

type LoginResetPasswordProps = PageProps<
  Extract<KcContext, { pageId: 'login-reset-password.ftl' }>,
  I18n
>

export default function LoginResetPassword(props: LoginResetPasswordProps) {
  const { kcContext, i18n } = props
  const { isReadyToRender } = useInitialize({
    kcContext,
    doUseDefaultCss: props.doUseDefaultCss,
  })
  const { advancedMsgStr, msgStr } = i18n
  const { url, realm, messagesPerField, message } = kcContext

  if (!isReadyToRender) return null

  return (
    <AuthCardLayout
      title={msgStr('emailForgotTitle')}
      description={msgStr('emailInstruction')}
      footer={
        <div className='text-center text-label'>
          <a
            href={url.loginUrl}
            className='font-medium text-primary hover:underline'
          >
            {msgStr('backToLogin')}
          </a>
        </div>
      }
    >
      {message ? (
        <p
          className={
            message.type === 'error'
              ? 'text-sm text-destructive'
              : 'text-sm text-muted-foreground'
          }
          role={message.type === 'error' ? 'alert' : 'status'}
        >
          {message.summary}
        </p>
      ) : null}

      <form
        id='kc-reset-password-form'
        action={url.loginAction}
        method='post'
        className='flex flex-col gap-4'
      >
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
            aria-invalid={messagesPerField.existsError('username') || undefined}
          />
          {messagesPerField.existsError('username') ? (
            <p className='text-sm text-destructive' role='alert'>
              {advancedMsgStr(messagesPerField.getFirstError('username') ?? '')}
            </p>
          ) : null}
        </div>

        <div className='pt-2'>
          <Button type='submit' className='w-full' size='lg'>
            {msgStr('doSubmit')}
          </Button>
        </div>
      </form>
    </AuthCardLayout>
  )
}
