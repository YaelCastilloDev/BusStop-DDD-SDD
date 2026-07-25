import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { AuthCardLayout } from '@/keycloak-theme/login/components/AuthCardLayout'
import { useAuth } from '@/lib/adapters/auth'
import { signup, ApiError } from '@/lib/api/auth-api'
import { useAuthRedirect } from '@/hooks/use-auth-redirect'
import { AuthFormError } from '@/components/auth/auth-form-error'
import { FormField } from '@/components/auth/form-field'
import { SectionDivider } from '@/components/auth/section-divider'
import { SocialAuthButtons } from '@/components/auth/social-auth-buttons'
import { SubmitButton } from '@/components/auth/submit-button'
import { VerifyEmailNotice } from '@/components/auth/verify-email-notice'
import {
  registerSchema,
  type RegisterFormValues,
} from '@/features/auth/schemas/auth-schemas'

export const Route = createFileRoute('/register')({
  component: RegisterPage,
})

function RegisterPage() {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const [registeredEmail, setRegisteredEmail] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    setError,
    clearErrors,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { email: '', password: '', confirmPassword: '' },
  })

  useAuthRedirect(isAuthenticated)

  const onSubmit = async (values: RegisterFormValues) => {
    clearErrors('root')
    try {
      await signup({ email: values.email.trim(), password: values.password })
      setRegisteredEmail(values.email.trim())
    } catch (error) {
      const message =
        error instanceof ApiError
          ? error.message
          : 'Registration failed. Please try again.'
      setError('root', { message })
    }
  }

  if (registeredEmail) {
    return (
      <AuthCardLayout>
        <VerifyEmailNotice
          email={registeredEmail}
          onBackToSignIn={() => navigate({ to: '/login' })}
        />
      </AuthCardLayout>
    )
  }

  return (
    <AuthCardLayout
      footer={
        <>
          <span className='text-muted-foreground'>
            Already have an account?
          </span>
          <a
            href='/login'
            className='text-sm font-medium text-primary hover:underline'
          >
            Sign in
          </a>
        </>
      }
    >
      <SocialAuthButtons />

      <SectionDivider label='Sign Up' />

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
          autoComplete='email'
          autoFocus
          disabled={isSubmitting}
          error={errors.email?.message}
          {...register('email')}
        />

        <FormField
          id='password'
          label='Password'
          type='password'
          autoComplete='new-password'
          disabled={isSubmitting}
          error={errors.password?.message}
          {...register('password')}
        />

        <FormField
          id='confirm-password'
          label='Confirm Password'
          type='password'
          autoComplete='new-password'
          disabled={isSubmitting}
          error={errors.confirmPassword?.message}
          {...register('confirmPassword')}
        />

        <SubmitButton
          loading={isSubmitting}
          loadingText='Creating account...'
          text='Sign Up'
          disabled={false}
        />
      </form>
    </AuthCardLayout>
  )
}
