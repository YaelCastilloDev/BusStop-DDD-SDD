import { beforeEach, describe, expect, it, vi } from 'vitest'
import { render } from 'vitest-browser-react'
import { AuthRedirectPage } from './auth-redirect-page'

const authMock = vi.hoisted(() => ({
  error: null as string | null,
  isAuthenticated: false,
  isLoading: false,
  login: vi.fn<() => Promise<void>>(),
  register: vi.fn<() => Promise<void>>(),
}))

vi.mock('@/lib/adapters/auth', () => ({
  useAuth: () => authMock,
}))

vi.mock('@/hooks/use-auth-redirect', () => ({
  useAuthRedirect: vi.fn(),
}))

describe('AuthRedirectPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    authMock.error = null
    authMock.isAuthenticated = false
    authMock.isLoading = false
    authMock.login.mockResolvedValue()
    authMock.register.mockResolvedValue()
  })

  it('starts the Keycloak login redirect without rendering credentials', async () => {
    const screen = await render(<AuthRedirectPage mode='login' />)

    await vi.waitFor(() => expect(authMock.login).toHaveBeenCalledOnce())
    await expect
      .element(screen.getByText('Redirecting securely to sign in…'))
      .toBeVisible()
    expect(document.querySelector('input[type="password"]')).toBeNull()
  })

  it('starts the Keycloak registration redirect without rendering credentials', async () => {
    const screen = await render(<AuthRedirectPage mode='register' />)

    await vi.waitFor(() => expect(authMock.register).toHaveBeenCalledOnce())
    await expect
      .element(screen.getByText('Redirecting securely to create your account…'))
      .toBeVisible()
    expect(document.querySelector('input[type="password"]')).toBeNull()
  })
})
