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

export const Route = createFileRoute('/register')({
  component: RegisterPage,
})

function RegisterPage() {
  const { directRegister, isLoading, isAuthenticated, error } = useAuth()
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)

  useAuthRedirect(isAuthenticated)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!firstName.trim() || !lastName.trim() || !email.trim() || !username.trim() || !password || !confirmPassword) return
    if (password !== confirmPassword) return

    setSubmitting(true)
    try {
      await directRegister({ firstName: firstName.trim(), lastName: lastName.trim(), email: email.trim(), username: username.trim(), password })
    } catch {
      setSubmitting(false)
    }
  }

  return (
    <AuthCardLayout
      footer={
        <>
          <span className='text-muted-foreground'>Already have an account?</span>
          <a href='/login' className='text-primary text-sm font-medium hover:underline'>
            Sign in
          </a>
        </>
      }
    >
      <SocialAuthButtons />

      <SectionDivider label='Sign Up' />

      <form onSubmit={handleSubmit} className='flex flex-col mt-6'>
        <AuthFormError error={error} />

        <div className='grid grid-cols-2 gap-4 mb-4'>
          <FormField
            id='firstName'
            label='First Name'
            autoComplete='given-name'
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            disabled={submitting || isLoading}
            className=''
          />
          <FormField
            id='lastName'
            label='Last Name'
            autoComplete='family-name'
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            disabled={submitting || isLoading}
            className=''
          />
        </div>

        <FormField
          id='email'
          label='Email'
          type='email'
          autoComplete='email'
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          disabled={submitting || isLoading}
        />

        <FormField
          id='username'
          label='Username'
          autoComplete='username'
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          disabled={submitting || isLoading}
        />

        <FormField
          id='password'
          label='Password'
          type='password'
          autoComplete='new-password'
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={submitting || isLoading}
        />

        <FormField
          id='confirm-password'
          label='Confirm Password'
          type='password'
          autoComplete='new-password'
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          disabled={submitting || isLoading}
        />

        <SubmitButton
          loading={submitting || isLoading}
          loadingText='Creating account...'
          text='Sign Up'
          disabled={!firstName.trim() || !lastName.trim() || !email.trim() || !username.trim() || !password || !confirmPassword || password !== confirmPassword}
        />
      </form>
    </AuthCardLayout>
  )
}
