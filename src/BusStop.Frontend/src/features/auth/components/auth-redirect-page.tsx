import { useCallback, useEffect, useRef } from 'react'
import { AuthCardLayout } from '@/keycloak-theme/login/components/AuthCardLayout'
import { LoaderCircle } from 'lucide-react'
import { useAuth } from '@/lib/adapters/auth'
import { useAuthRedirect } from '@/hooks/use-auth-redirect'
import { Button } from '@/components/ui/button'

type AuthRedirectPageProps = {
  mode: 'login' | 'register'
}

export function AuthRedirectPage({ mode }: AuthRedirectPageProps) {
  const { error, isAuthenticated, isLoading, login, register } = useAuth()
  const redirectStarted = useRef(false)
  const redirect = mode === 'login' ? login : register

  useAuthRedirect(isAuthenticated)

  const startRedirect = useCallback(() => {
    redirectStarted.current = true
    void redirect()
  }, [redirect])

  useEffect(() => {
    if (isLoading || isAuthenticated || error || redirectStarted.current) return
    startRedirect()
  }, [error, isAuthenticated, isLoading, startRedirect])

  const actionLabel = mode === 'login' ? 'sign in' : 'create your account'

  return (
    <AuthCardLayout>
      <div className='flex flex-col items-center gap-4 py-8 text-center'>
        {error ? (
          <>
            <p className='text-sm text-destructive' role='alert'>
              {error}
            </p>
            <Button type='button' onClick={startRedirect}>
              Try again
            </Button>
          </>
        ) : (
          <>
            <LoaderCircle
              aria-hidden='true'
              className='size-6 animate-spin text-primary'
            />
            <p className='text-sm text-muted-foreground' role='status'>
              Redirecting securely to {actionLabel}…
            </p>
          </>
        )}
      </div>
    </AuthCardLayout>
  )
}
