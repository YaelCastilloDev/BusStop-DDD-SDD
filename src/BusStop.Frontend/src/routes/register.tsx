import { createFileRoute } from '@tanstack/react-router'
import { AuthRedirectPage } from '@/features/auth/components/auth-redirect-page'

export const Route = createFileRoute('/register')({
  component: RegisterPage,
})

function RegisterPage() {
  return <AuthRedirectPage mode='register' />
}
