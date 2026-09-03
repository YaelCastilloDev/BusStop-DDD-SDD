import { beforeEach, describe, expect, it, vi } from 'vitest'
import { KeycloakAdapter } from './KeycloakAdapter'

const keycloakMock = vi.hoisted(() => ({
  init: vi.fn<(options: unknown) => Promise<boolean>>(),
  login: vi.fn<(options: unknown) => Promise<void>>(),
  logout: vi.fn<(options: unknown) => Promise<void>>(),
  register: vi.fn<(options: unknown) => Promise<void>>(),
  updateToken: vi.fn<(minValidity: number) => Promise<boolean>>(),
  clearToken: vi.fn<() => void>(),
  hasRealmRole: vi.fn<(role: string) => boolean>(),
  authenticated: false,
  token: undefined as string | undefined,
  tokenParsed: undefined as Record<string, unknown> | undefined,
  onTokenExpired: undefined as (() => void) | undefined,
  onAuthRefreshSuccess: undefined as (() => void) | undefined,
  onAuthRefreshError: undefined as (() => void) | undefined,
  onAuthLogout: undefined as (() => void) | undefined,
}))

vi.mock('keycloak-js', () => ({
  default: class KeycloakMock {
    constructor() {
      return keycloakMock
    }
  },
}))

describe('KeycloakAdapter', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    keycloakMock.authenticated = false
    keycloakMock.token = undefined
    keycloakMock.tokenParsed = undefined
    keycloakMock.init.mockResolvedValue(false)
    keycloakMock.login.mockResolvedValue()
    keycloakMock.logout.mockResolvedValue()
    keycloakMock.register.mockResolvedValue()
    keycloakMock.updateToken.mockResolvedValue(false)
  })

  it('initializes standard Authorization Code flow with PKCE before use', async () => {
    const adapter = new KeycloakAdapter()

    await adapter.init()

    expect(keycloakMock.init).toHaveBeenCalledWith(
      expect.objectContaining({
        onLoad: 'check-sso',
        flow: 'standard',
        pkceMethod: 'S256',
        checkLoginIframe: true,
      })
    )
  })

  it('delegates login and registration to Keycloak-hosted browser flows', async () => {
    const adapter = new KeycloakAdapter()

    await adapter.login()
    await adapter.register()

    expect(keycloakMock.login).toHaveBeenCalledWith({
      redirectUri: window.location.origin,
    })
    expect(keycloakMock.register).toHaveBeenCalledWith({
      redirectUri: window.location.origin,
    })
    expect(keycloakMock.init).toHaveBeenCalledOnce()
  })

  it('returns an in-memory access token after refreshing it', async () => {
    keycloakMock.init.mockResolvedValue(true)
    keycloakMock.authenticated = true
    keycloakMock.token = 'access-token'
    const adapter = new KeycloakAdapter()
    await adapter.init()

    await expect(adapter.getToken()).resolves.toBe('access-token')
    expect(keycloakMock.updateToken).toHaveBeenCalledWith(30)
  })

  it('clears the local token when refresh fails', async () => {
    keycloakMock.init.mockResolvedValue(true)
    keycloakMock.authenticated = true
    keycloakMock.updateToken.mockRejectedValue(new Error('refresh failed'))
    const adapter = new KeycloakAdapter()
    await adapter.init()

    await expect(adapter.getToken()).resolves.toBeUndefined()
    expect(keycloakMock.clearToken).toHaveBeenCalledOnce()
  })

  it('maps the profile parsed and validated by keycloak-js', async () => {
    keycloakMock.tokenParsed = {
      sub: 'keycloak-subject',
      preferred_username: 'rider@example.com',
      email: 'rider@example.com',
      given_name: 'Bus',
      family_name: 'Rider',
      email_verified: true,
    }
    const adapter = new KeycloakAdapter()

    expect(adapter.getUserProfile()).toEqual({
      id: 'keycloak-subject',
      username: 'rider@example.com',
      email: 'rider@example.com',
      firstName: 'Bus',
      lastName: 'Rider',
      emailVerified: true,
    })
  })

  it('rejects a parsed profile without an immutable subject identifier', () => {
    keycloakMock.tokenParsed = {
      preferred_username: 'rider@example.com',
      email: 'rider@example.com',
    }
    const adapter = new KeycloakAdapter()

    expect(adapter.getUserProfile()).toBeNull()
  })
})
