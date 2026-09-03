import { useInitialize } from 'keycloakify/login/Template.useInitialize'
import type { PageProps } from 'keycloakify/login/pages/PageProps'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import type { KcContext } from '../KcContext'
import { AuthCardLayout } from '../components/AuthCardLayout'
import type { I18n } from '../i18n'

type LoginUpdatePasswordProps = PageProps<
  Extract<KcContext, { pageId: 'login-update-password.ftl' }>,
  I18n
>

export default function LoginUpdatePassword(props: LoginUpdatePasswordProps) {
  const { kcContext, i18n } = props
  const { isReadyToRender } = useInitialize({
    kcContext,
    doUseDefaultCss: props.doUseDefaultCss,
  })
  const { advancedMsgStr, msgStr } = i18n
  const { url, messagesPerField, isAppInitiatedAction, message } = kcContext

  if (!isReadyToRender) return null

  return (
    <AuthCardLayout
      title={msgStr('updatePasswordTitle')}
      description={msgStr('updatePasswordMessage')}
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
        id='kc-passwd-update-form'
        action={url.loginAction}
        method='post'
        className='flex flex-col gap-4'
      >
        <div className='space-y-2'>
          <Label htmlFor='password-new'>{msgStr('passwordNew')}</Label>
          <Input
            id='password-new'
            name='password-new'
            type='password'
            autoFocus
            autoComplete='new-password'
            aria-invalid={messagesPerField.existsError('password') || undefined}
          />
          {messagesPerField.existsError('password') ? (
            <p className='text-sm text-destructive' role='alert'>
              {advancedMsgStr(messagesPerField.getFirstError('password') ?? '')}
            </p>
          ) : null}
        </div>

        <div className='space-y-2'>
          <Label htmlFor='password-confirm'>{msgStr('passwordConfirm')}</Label>
          <Input
            id='password-confirm'
            name='password-confirm'
            type='password'
            autoComplete='new-password'
            aria-invalid={
              messagesPerField.existsError('password-confirm') || undefined
            }
          />
          {messagesPerField.existsError('password-confirm') ? (
            <p className='text-sm text-destructive' role='alert'>
              {advancedMsgStr(
                messagesPerField.getFirstError('password-confirm') ?? ''
              )}
            </p>
          ) : null}
        </div>

        <label className='flex items-center gap-2 text-sm text-muted-foreground'>
          <input
            id='logout-sessions'
            name='logout-sessions'
            type='checkbox'
            value='on'
            defaultChecked
            className='size-4 rounded border text-primary'
          />
          {msgStr('logoutOtherSessions')}
        </label>

        <div className='flex gap-3 pt-2'>
          <Button type='submit' className='flex-1' size='lg'>
            {msgStr('doSubmit')}
          </Button>
          {isAppInitiatedAction ? (
            <Button
              type='submit'
              name='cancel-aia'
              value='true'
              variant='outline'
              size='lg'
            >
              {msgStr('doCancel')}
            </Button>
          ) : null}
        </div>
      </form>
    </AuthCardLayout>
  )
}
