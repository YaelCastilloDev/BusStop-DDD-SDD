import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute } from '@tanstack/react-router'
import { AuthCardLayout } from '@/keycloak-theme/login/components/AuthCardLayout'
import { useAuth } from '@/lib/adapters/auth'
import { useAuthRedirect } from '@/hooks/use-auth-redirect'
import { AuthFormError } from '@/components/auth/auth-form-error'
import { FormField } from '@/components/auth/form-field'
import { SectionDivider } from '@/components/auth/section-divider'
import { SocialAuthButtons } from '@/components/auth/social-auth-buttons'
import { SubmitButton } from '@/components/auth/submit-button'
import { VerifyEmailNotice } from '@/components/auth/verify-email-notice'
import {
  loginSchema,
  type LoginFormValues,
} from '@/features/auth/schemas/auth-schemas'

export const Route = createFileRoute('/login')({
  component: LoginPage,
})

function isUnverifiedAccountError(message: string): boolean {
  const normalized = message.toLowerCase()
  return (
    normalized.includes('not fully set up') || normalized.includes('verify')
  )
}

function LoginPage() {
  const { directLogin, discardSession, isAuthenticated } = useAuth()
  const [unverifiedEmail, setUnverifiedEmail] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    setError,
    clearErrors,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })

  useAuthRedirect(isAuthenticated)

  const onSubmit = async (values: LoginFormValues) => {
    clearErrors('root')
    const email = values.email.trim()

    try {
      const profile = await directLogin(email, values.password)

      if (!profile?.emailVerified) {
        discardSession()
        setUnverifiedEmail(email)
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Login failed'
      if (isUnverifiedAccountError(message)) {
        discardSession()
        setUnverifiedEmail(email)
        return
      }
      setError('root', { message })
    }
  }

  if (unverifiedEmail) {
    return (
      <AuthCardLayout>
        <VerifyEmailNotice
          email={unverifiedEmail}
          onBackToSignIn={() => setUnverifiedEmail(null)}
        />
      </AuthCardLayout>
    )
  }

  return (
    <AuthCardLayout
      footer={
        <>
          <span className='text-muted-foreground'>Don't have an account?</span>
          <a
            href='/register'
            className='text-sm font-medium text-primary hover:underline'
          >
            Create an account
          </a>
        </>
      }
    >
      <SocialAuthButtons />

      <SectionDivider label='Sign In' />

      <form
        onSubmit={handleSubmit(onSubmit)}
        className='mt-6 flex flex-col'
        noValidate
      >
        <AuthFormError error={errors.root?.message ?? null} />

        <FormField
          id='email'
          label='Email'
          type='email'
          autoFocus
          autoComplete='username'
          disabled={isSubmitting}
          error={errors.email?.message}
          {...register('email')}
        />

        <FormField
          id='password'
          label='Password'
          type='password'
          autoComplete='current-password'
          disabled={isSubmitting}
          error={errors.password?.message}
          {...register('password')}
        />

        <SubmitButton
          loading={isSubmitting}
          loadingText='Signing in...'
          text='Sign In'
          disabled={false}
        />
      </form>
    </AuthCardLayout>
  )
}
