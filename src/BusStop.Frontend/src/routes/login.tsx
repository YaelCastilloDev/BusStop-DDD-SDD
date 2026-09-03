import { createFileRoute } from '@tanstack/react-router'
import { AuthRedirectPage } from '@/features/auth/components/auth-redirect-page'

export const Route = createFileRoute('/login')({
  component: LoginPage,
})

function LoginPage() {
  return <AuthRedirectPage mode='login' />
}
