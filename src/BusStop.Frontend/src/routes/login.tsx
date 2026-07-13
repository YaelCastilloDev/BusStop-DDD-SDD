import { createFileRoute } from '@tanstack/react-router'
import { useState, type FormEvent } from 'react'
import { useAuth } from '@/lib/adapters/auth'
import { AuthCardLayout } from '@/keycloak-theme/login/components/AuthCardLayout'
import { SocialAuthButtons } from '@/components/auth/social-auth-buttons'
import { SectionDivider } from '@/components/auth/section-divider'
import { AuthFormError } from '@/components/auth/auth-form-error'
import { FormField } from '@/components/auth/form-field'
import { SubmitButton } from '@/components/auth/submit-button'
import { useAuthRedirect } from '@/hooks/use-auth-redirect'

export const Route = createFileRoute('/login')({
  component: LoginPage,
})

function LoginPage() {
  const { directLogin, isLoading, isAuthenticated, error } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)

  useAuthRedirect(isAuthenticated)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!username.trim() || !password) return

    setSubmitting(true)
    try {
      await directLogin(username, password)
    } catch {
      setSubmitting(false)
    }
  }

  return (
    <AuthCardLayout
      footer={
        <>
          <span className='text-muted-foreground'>Don't have an account?</span>
          <a href='/register' className='text-primary text-sm font-medium hover:underline'>
            Create an account
          </a>
        </>
      }
    >
      <SocialAuthButtons />

      <SectionDivider label='Sign In' />

      <form onSubmit={handleSubmit} className='flex flex-col mt-6'>
        <AuthFormError error={error} />

        <FormField
          id='username'
          label='Email'
          autoFocus
          autoComplete='username'
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          disabled={submitting || isLoading}
        />

        <FormField
          id='password'
          label='Password'
          type='password'
          autoComplete='current-password'
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={submitting || isLoading}
        />

        <SubmitButton
          loading={submitting || isLoading}
          loadingText='Signing in...'
          text='Sign In'
          disabled={!username.trim() || !password}
        />
      </form>
    </AuthCardLayout>
  )
}
