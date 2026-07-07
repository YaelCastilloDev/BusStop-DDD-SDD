import type { PageProps } from 'keycloakify/login/pages/PageProps'
import type { KcContext } from '../KcContext'
import type { I18n } from '../i18n'
import { AuthCardLayout } from '../components/AuthCardLayout'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'

type LoginUpdatePasswordProps = PageProps<
  Extract<KcContext, { pageId: 'login-update-password.ftl' }>,
  I18n
>

export default function LoginUpdatePassword(props: LoginUpdatePasswordProps) {
  const { kcContext, i18n } = props
  const { msg, msgStr } = i18n
  const { url, messagesPerField, username } = kcContext

  return (
    <AuthCardLayout
      title={msgStr('updatePasswordTitle')}
      description={msgStr('updatePasswordMessage')}
    >
      <form
        id='kc-passwd-update-form'
        action={url.loginAction}
        method='post'
        className='flex flex-col gap-4'
      >
        <Input
          id='username'
          name='username'
          type='hidden'
          value={username}
          readOnly
        />

        <div className='space-y-2'>
          <Label htmlFor='password-new'>{msgStr('passwordNew')}</Label>
          <Input
            id='password-new'
            name='password-new'
            type='password'
            autoFocus
            autoComplete='new-password'
            aria-invalid={
              messagesPerField.existsError('password') || undefined
            }
          />
          {messagesPerField.existsError('password') ? (
            <p className='text-sm text-destructive' role='alert'>
              {msgStr(
                messagesPerField.getFirstError('password') ?? ''
              )}
            </p>
          ) : null}
        </div>

        <div className='space-y-2'>
          <Label htmlFor='password-confirm'>
            {msgStr('passwordConfirm')}
          </Label>
          <Input
            id='password-confirm'
            name='password-confirm'
            type='password'
            autoComplete='new-password'
            aria-invalid={
              messagesPerField.existsError('password-confirm') ||
              undefined
            }
          />
          {messagesPerField.existsError('password-confirm') ? (
            <p className='text-sm text-destructive' role='alert'>
              {msgStr(
                messagesPerField.getFirstError('password-confirm') ??
                  ''
              )}
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
