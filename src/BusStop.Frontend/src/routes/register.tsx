import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useEffect, useState, type FormEvent } from 'react'
import { useAuth } from '@/lib/adapters/auth'
import { AuthCardLayout } from '@/keycloak-theme/login/components/AuthCardLayout'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'

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
  const navigate = useNavigate()

  useEffect(() => {
    if (isAuthenticated) {
      navigate({ to: '/' })
    }
  }, [isAuthenticated, navigate])

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
      <div className='flex justify-between gap-8 my-6'>
        <button
          type='button'
          className='px-4 py-2.5 border flex gap-2 items-center w-full rounded-md text-center justify-center text-foreground transition-colors hover:bg-muted'
        >
          <svg width='18' height='18' viewBox='0 0 28 28' fill='none' xmlns='http://www.w3.org/2000/svg'>
            <path d='M27.9851 14.2618C27.9851 13.1146 27.8899 12.2775 27.6837 11.4094H14.2788V16.5871H22.1472C21.9886 17.8738 21.132 19.8116 19.2283 21.1137L19.2016 21.287L23.44 24.4956L23.7336 24.5242C26.4304 22.0904 27.9851 18.5093 27.9851 14.2618Z' fill='#4285F4'/>
            <path d='M14.279 27.904C18.1338 27.904 21.37 26.6637 23.7338 24.5245L19.2285 21.114C18.0228 21.9356 16.4047 22.5092 14.279 22.5092C10.5034 22.5092 7.29894 20.0754 6.15663 16.7114L5.9892 16.7253L1.58205 20.0583L1.52441 20.2149C3.87224 24.7725 8.69486 27.904 14.279 27.904Z' fill='#34A853'/>
            <path d='M6.15656 16.7113C5.85516 15.8432 5.68072 14.913 5.68072 13.9519C5.68072 12.9907 5.85516 12.0606 6.14071 11.1925L6.13272 11.0076L1.67035 7.62109L1.52435 7.68896C0.556704 9.58024 0.00146484 11.7041 0.00146484 13.9519C0.00146484 16.1997 0.556704 18.3234 1.52435 20.2147L6.15656 16.7113Z' fill='#FBBC05'/>
            <path d='M14.279 5.3947C16.9599 5.3947 18.7683 6.52635 19.7995 7.47204L23.8289 3.6275C21.3542 1.37969 18.1338 0 14.279 0C8.69485 0 3.87223 3.1314 1.52441 7.68899L6.14077 11.1925C7.29893 7.82856 10.5034 5.3947 14.279 5.3947Z' fill='#EB4335'/>
          </svg>
          Google
        </button>
        <button
          type='button'
          className='px-4 py-2.5 border flex gap-2 items-center w-full rounded-md text-center justify-center text-foreground transition-colors hover:bg-muted'
        >
          <svg width='18' height='18' viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
            <path d='M24 12.073C24 5.405 18.627 0 12 0C5.373 0 0 5.405 0 12.073C0 18.1 4.388 23.094 10.125 24V15.562H7.078V12.073H10.125V9.414C10.125 6.41 11.916 4.75 14.656 4.75C15.969 4.75 17.344 4.985 17.344 4.985V7.938H15.83C14.339 7.938 13.875 8.863 13.875 9.816V12.073H17.203L16.67 15.562H13.875V24C19.612 23.094 24 18.1 24 12.073Z' fill='#1877F2'/>
          </svg>
          Facebook
        </button>
      </div>

      <div className='flex items-center justify-center gap-2'>
        <hr className='grow border' />
        <p className='text-base text-foreground font-medium'>Sign Up</p>
        <hr className='grow border' />
      </div>

      <form onSubmit={handleSubmit} className='flex flex-col mt-6'>
        {error ? (
          <p className='text-sm text-destructive text-center mb-4' role='alert'>
            {error}
          </p>
        ) : null}

        <div className='grid grid-cols-2 gap-4 mb-4'>
          <div>
            <div className='mb-2 block'>
              <Label htmlFor='firstName' className='font-semibold'>First Name</Label>
            </div>
            <Input
              id='firstName'
              name='firstName'
              type='text'
              autoComplete='given-name'
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              disabled={submitting || isLoading}
              className='h-10 rounded-lg py-2 shadow-none text-sm !border focus-visible:outline-0 focus-visible:ring-0 focus-visible:border-primary'
            />
          </div>
          <div>
            <div className='mb-2 block'>
              <Label htmlFor='lastName' className='font-semibold'>Last Name</Label>
            </div>
            <Input
              id='lastName'
              name='lastName'
              type='text'
              autoComplete='family-name'
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              disabled={submitting || isLoading}
              className='h-10 rounded-lg py-2 shadow-none text-sm !border focus-visible:outline-0 focus-visible:ring-0 focus-visible:border-primary'
            />
          </div>
        </div>

        <div className='mb-4'>
          <div className='mb-2 block'>
            <Label htmlFor='email' className='font-semibold'>Email</Label>
          </div>
          <Input
            id='email'
            name='email'
            type='email'
            autoComplete='email'
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            disabled={submitting || isLoading}
            className='h-10 rounded-lg py-2 shadow-none text-sm !border focus-visible:outline-0 focus-visible:ring-0 focus-visible:border-primary'
          />
        </div>

        <div className='mb-4'>
          <div className='mb-2 block'>
            <Label htmlFor='username' className='font-semibold'>Username</Label>
          </div>
          <Input
            id='username'
            name='username'
            type='text'
            autoComplete='username'
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            disabled={submitting || isLoading}
            className='h-10 rounded-lg py-2 shadow-none text-sm !border focus-visible:outline-0 focus-visible:ring-0 focus-visible:border-primary'
          />
        </div>

        <div className='mb-4'>
          <div className='mb-2 block'>
            <Label htmlFor='password' className='font-semibold'>Password</Label>
          </div>
          <Input
            id='password'
            name='password'
            type='password'
            autoComplete='new-password'
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            disabled={submitting || isLoading}
            className='h-10 rounded-lg py-2 shadow-none text-sm !border focus-visible:outline-0 focus-visible:ring-0 focus-visible:border-primary'
          />
        </div>

        <div className='mb-4'>
          <div className='mb-2 block'>
            <Label htmlFor='confirm-password' className='font-semibold'>Confirm Password</Label>
          </div>
          <Input
            id='confirm-password'
            name='confirm-password'
            type='password'
            autoComplete='new-password'
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            disabled={submitting || isLoading}
            className='h-10 rounded-lg py-2 shadow-none text-sm !border focus-visible:outline-0 focus-visible:ring-0 focus-visible:border-primary'
          />
        </div>

        <div className='my-5'>
          <Button
            type='submit'
            className='w-full h-10 px-5 py-2 text-sm font-medium shadow-none transition-colors hover:bg-primary-emphasis focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 ring-offset-background'
            disabled={submitting || isLoading || !firstName.trim() || !lastName.trim() || !email.trim() || !username.trim() || !password || !confirmPassword || password !== confirmPassword}
          >
            {submitting || isLoading ? 'Creating account...' : 'Sign Up'}
          </Button>
        </div>
      </form>
    </AuthCardLayout>
  )
}
