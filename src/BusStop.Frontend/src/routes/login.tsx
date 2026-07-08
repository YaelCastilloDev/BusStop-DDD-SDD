import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useEffect, useState, type FormEvent } from 'react'
import { useAuth } from '@/lib/adapters/auth'
import { AuthCardLayout } from '@/keycloak-theme/login/components/AuthCardLayout'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'

export const Route = createFileRoute('/login')({
  component: LoginPage,
})

function LoginPage() {
  const { directLogin, isLoading, isAuthenticated, error } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    if (isAuthenticated) {
      navigate({ to: '/' })
    }
  }, [isAuthenticated, navigate])

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
      title='Sign In'
      description='Enter your credentials to access BusStop'
    >
      <form onSubmit={handleSubmit} className='flex flex-col gap-4'>
        {error ? (
          <p className='text-sm text-destructive text-center' role='alert'>
            {error}
          </p>
        ) : null}

        <div className='space-y-2'>
          <Label htmlFor='username'>Email</Label>
          <Input
            id='username'
            name='username'
            type='text'
            autoFocus
            autoComplete='username'
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            disabled={submitting || isLoading}
          />
        </div>

        <div className='space-y-2'>
          <Label htmlFor='password'>Password</Label>
          <Input
            id='password'
            name='password'
            type='password'
            autoComplete='current-password'
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            disabled={submitting || isLoading}
          />
        </div>

        <div className='pt-2'>
          <Button
            type='submit'
            className='w-full'
            size='lg'
            disabled={submitting || isLoading || !username.trim() || !password}
          >
            {submitting || isLoading ? 'Signing in...' : 'Sign In'}
          </Button>
        </div>
      </form>
    </AuthCardLayout>
  )
}
