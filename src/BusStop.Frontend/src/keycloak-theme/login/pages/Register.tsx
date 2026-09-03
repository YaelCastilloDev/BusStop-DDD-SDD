import { useLayoutEffect, useState, type ReactElement } from 'react'
import { useInitialize } from 'keycloakify/login/Template.useInitialize'
import type { UserProfileFormFieldsProps } from 'keycloakify/login/UserProfileFormFieldsProps'
import { getKcClsx } from 'keycloakify/login/lib/kcClsx'
import type { PageProps } from 'keycloakify/login/pages/PageProps'
import type { LazyOrNot } from 'keycloakify/tools/LazyOrNot'
import { Button } from '@/components/ui/button'
import type { KcContext } from '../KcContext'
import { AuthCardLayout } from '../components/AuthCardLayout'
import type { I18n } from '../i18n'

type RegisterKcContext = Extract<KcContext, { pageId: 'register.ftl' }>

type RegisterProps = PageProps<RegisterKcContext, I18n> & {
  UserProfileFormFields: LazyOrNot<
    (props: UserProfileFormFieldsProps) => ReactElement
  >
  doMakeUserConfirmPassword: boolean
}

type RecaptchaWindow = Window & {
  onSubmitRecaptcha?: () => void
}

export default function Register(props: RegisterProps) {
  const {
    kcContext,
    i18n,
    doUseDefaultCss,
    classes,
    UserProfileFormFields,
    doMakeUserConfirmPassword,
  } = props
  const { isReadyToRender } = useInitialize({ kcContext, doUseDefaultCss })
  const {
    url,
    messagesPerField,
    message,
    messageHeader,
    recaptchaRequired,
    recaptchaVisible,
    recaptchaSiteKey,
    recaptchaAction,
    termsAcceptanceRequired,
  } = kcContext
  const { msg, msgStr, advancedMsg } = i18n
  const { kcClsx } = getKcClsx({ doUseDefaultCss, classes })
  const [isFormSubmittable, setIsFormSubmittable] = useState(false)
  const [areTermsAccepted, setAreTermsAccepted] = useState(false)

  useLayoutEffect(() => {
    const recaptchaWindow = window as RecaptchaWindow
    recaptchaWindow.onSubmitRecaptcha = () => {
      const form = document.getElementById('kc-register-form')
      if (form instanceof HTMLFormElement) form.requestSubmit()
    }

    return () => {
      delete recaptchaWindow.onSubmitRecaptcha
    }
  }, [])

  const usesInvisibleRecaptcha =
    recaptchaRequired && !recaptchaVisible && recaptchaAction !== undefined
  const isSubmitDisabled =
    !isFormSubmittable || (termsAcceptanceRequired && !areTermsAccepted)

  if (!isReadyToRender) return null

  return (
    <AuthCardLayout
      footer={
        <>
          <span className='text-muted-foreground'>
            {msgStr('alreadyHaveAccount')}
          </span>
          <a
            href={url.loginUrl}
            className='text-sm font-medium text-primary hover:underline'
          >
            {msgStr('doLogIn')}
          </a>
        </>
      }
    >
      {messageHeader !== undefined ? (
        <p className='mb-4 text-center text-sm font-medium text-primary'>
          {advancedMsg(messageHeader)}
        </p>
      ) : null}

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

      <form
        id='kc-register-form'
        action={url.registrationAction}
        method='post'
        className='mt-6 flex flex-col'
      >
        <UserProfileFormFields
          kcContext={kcContext}
          i18n={i18n}
          kcClsx={kcClsx}
          onIsFormSubmittableValueChange={setIsFormSubmittable}
          doMakeUserConfirmPassword={doMakeUserConfirmPassword}
        />

        {termsAcceptanceRequired ? (
          <div className='mb-4'>
            <div className='mb-3 text-sm text-muted-foreground'>
              {msg('termsText')}
            </div>
            <label className='flex items-center gap-2 text-sm'>
              <input
                type='checkbox'
                id='termsAccepted'
                name='termsAccepted'
                checked={areTermsAccepted}
                onChange={(event) => setAreTermsAccepted(event.target.checked)}
                aria-invalid={
                  messagesPerField.existsError('termsAccepted') || undefined
                }
                className='size-4 rounded border text-primary'
              />
              {msg('acceptTerms')}
            </label>
            {messagesPerField.existsError('termsAccepted') ? (
              <p className='mt-1 text-sm text-destructive' role='alert'>
                {advancedMsg(
                  messagesPerField.getFirstError('termsAccepted') ?? ''
                )}
              </p>
            ) : null}
          </div>
        ) : null}

        {recaptchaRequired &&
        (recaptchaVisible || recaptchaAction === undefined) ? (
          <div
            className='g-recaptcha mb-4'
            data-size='compact'
            data-sitekey={recaptchaSiteKey}
            data-action={recaptchaAction}
          />
        ) : null}

        <Button
          type='submit'
          className={
            usesInvisibleRecaptcha ? 'g-recaptcha h-10 w-full' : 'h-10 w-full'
          }
          data-sitekey={usesInvisibleRecaptcha ? recaptchaSiteKey : undefined}
          data-callback={
            usesInvisibleRecaptcha ? 'onSubmitRecaptcha' : undefined
          }
          data-action={usesInvisibleRecaptcha ? recaptchaAction : undefined}
          disabled={isSubmitDisabled}
        >
          {msgStr('doRegister')}
        </Button>
      </form>
    </AuthCardLayout>
  )
}
