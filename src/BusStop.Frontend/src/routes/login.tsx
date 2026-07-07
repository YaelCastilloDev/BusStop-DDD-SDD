import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import { useAuth } from '@/lib/adapters/auth'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Bus, LogIn } from 'lucide-react'

export const Route = createFileRoute('/login')({
  component: LoginPage,
})

function LoginPage() {
  const { login, isLoading, isAuthenticated, error } = useAuth()
  const [loginTriggered, setLoginTriggered] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    if (isAuthenticated) {
      navigate({ to: '/' })
    }
  }, [isAuthenticated, navigate])

  const handleLogin = async () => {
    setLoginTriggered(true)
    try {
      await login()
    } catch {
      setLoginTriggered(false)
    }
  }

  return (
    <div className='flex min-h-svh items-center justify-center bg-background p-4'>
      <Card className='w-full max-w-sm shadow-lg'>
        <CardHeader className='space-y-1 text-center'>
          <Bus className='mx-auto size-12 text-primary' />
          <CardTitle className='text-h2'>BusStop</CardTitle>
          <CardDescription className='text-body-sm'>
            Sign in to manage transit routes and stops
          </CardDescription>
        </CardHeader>
        <CardContent>
          {error ? (
            <p className='text-sm text-destructive mb-4 text-center'>
              {error}
            </p>
          ) : null}
          <Button
            className='w-full'
            size='lg'
            onClick={handleLogin}
            disabled={loginTriggered || isLoading}
          >
            <LogIn className='size-4' />
            {loginTriggered ? 'Redirecting...' : 'Sign in with Keycloak'}
          </Button>
        </CardContent>
        <CardFooter className='justify-center'>
          <p className='text-caption text-muted-foreground'>
            You will be redirected to the Keycloak login page
          </p>
        </CardFooter>
      </Card>
    </div>
  )
}
