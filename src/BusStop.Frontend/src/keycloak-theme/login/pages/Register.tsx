import type { PageProps } from 'keycloakify/login/pages/PageProps'
import type { KcContext } from '../KcContext'
import type { I18n } from '../i18n'
import { AuthCardLayout } from '../components/AuthCardLayout'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'

type RegisterProps = PageProps<
  Extract<KcContext, { pageId: 'register.ftl' }>,
  I18n
>

export default function Register(props: RegisterProps) {
  const { kcContext, i18n } = props
  const { msg, msgStr, advancedMsg } = i18n
  const { url, messagesPerField, messageHeader } = kcContext

  return (
    <AuthCardLayout
      title={
        messageHeader !== undefined
          ? advancedMsg(messageHeader)
          : msgStr('registerTitle')
      }
      footer={
        <div className='text-center text-label'>
          <span className='text-muted-foreground'>
            {msgStr('alreadyHaveAccount')}{' '}
          </span>
          <a
            href={url.loginUrl}
            className='font-medium text-primary hover:underline'
          >
            {msgStr('doLogIn')}
          </a>
        </div>
      }
    >
      <form
        id='kc-register-form'
        action={url.registrationAction}
        method='post'
        className='flex flex-col gap-4'
      >
        <div className='space-y-2'>
          <Label htmlFor='firstName'>{msgStr('firstName')}</Label>
          <Input
            id='firstName'
            name='firstName'
            type='text'
            autoFocus
            autoComplete='given-name'
            aria-invalid={
              messagesPerField.existsError('firstName') || undefined
            }
          />
          {messagesPerField.existsError('firstName') ? (
            <p className='text-sm text-destructive' role='alert'>
              {msgStr(
                messagesPerField.getFirstError('firstName') ?? ''
              )}
            </p>
          ) : null}
        </div>

        <div className='space-y-2'>
          <Label htmlFor='lastName'>{msgStr('lastName')}</Label>
          <Input
            id='lastName'
            name='lastName'
            type='text'
            autoComplete='family-name'
            aria-invalid={
              messagesPerField.existsError('lastName') || undefined
            }
          />
          {messagesPerField.existsError('lastName') ? (
            <p className='text-sm text-destructive' role='alert'>
              {msgStr(
                messagesPerField.getFirstError('lastName') ?? ''
              )}
            </p>
          ) : null}
        </div>

        <div className='space-y-2'>
          <Label htmlFor='email'>{msgStr('email')}</Label>
          <Input
            id='email'
            name='email'
            type='email'
            autoComplete='email'
            aria-invalid={
              messagesPerField.existsError('email') || undefined
            }
          />
          {messagesPerField.existsError('email') ? (
            <p className='text-sm text-destructive' role='alert'>
              {msgStr(messagesPerField.getFirstError('email') ?? '')}
            </p>
          ) : null}
        </div>

        <div className='space-y-2'>
          <Label htmlFor='username'>{msgStr('username')}</Label>
          <Input
            id='username'
            name='username'
            type='text'
            autoComplete='username'
            aria-invalid={
              messagesPerField.existsError('username') || undefined
            }
          />
          {messagesPerField.existsError('username') ? (
            <p className='text-sm text-destructive' role='alert'>
              {msgStr(
                messagesPerField.getFirstError('username') ?? ''
              )}
            </p>
          ) : null}
        </div>

        <div className='space-y-2'>
          <Label htmlFor='password'>{msgStr('password')}</Label>
          <Input
            id='password'
            name='password'
            type='password'
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

        <div id='kc-form-buttons' className='pt-2'>
          <Button type='submit' className='w-full' size='lg'>
            {msgStr('doRegister')}
          </Button>
        </div>
      </form>
    </AuthCardLayout>
  )
}
