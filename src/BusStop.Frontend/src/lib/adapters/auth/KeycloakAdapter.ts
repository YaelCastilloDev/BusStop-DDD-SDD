import Keycloak from 'keycloak-js'
import { createLogger } from '@/lib/logger'
import type { IAuthAdapter } from './IAuthAdapter'
import type { UserProfile } from './types'

const TOKEN_MIN_VALIDITY_SECONDS = 30

function getInitTimeoutMs(): number {
  const envValue = import.meta.env.VITE_KEYCLOAK_INIT_TIMEOUT_MS
  if (envValue) {
    const parsed = Number(envValue)
    if (Number.isFinite(parsed) && parsed > 0) return parsed
  }
  return 30_000
}

const logger = createLogger('KeycloakAdapter')

export class KeycloakAdapter implements IAuthAdapter {
  private keycloak: Keycloak
  private _initialized = false
  private _initPromise: Promise<boolean> | null = null

  constructor() {
    this.keycloak = new Keycloak({
      url: import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8080',
      realm: import.meta.env.VITE_KEYCLOAK_REALM ?? 'auth-demo',
      clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'busstop-web',
    })
  }

  async init(): Promise<boolean> {
    if (this._initPromise) {
      return this._initPromise
    }

    this._initPromise = this._doInit().catch((error: unknown) => {
      this._initPromise = null
      throw error
    })
    return this._initPromise
  }

  private async _doInit(): Promise<boolean> {
    const timeoutMs = getInitTimeoutMs()
    let timeoutId: number | undefined

    try {
      const authenticated = await Promise.race([
        this.keycloak.init({
          onLoad: 'check-sso',
          flow: 'standard',
          silentCheckSsoRedirectUri:
            window.location.origin + '/silent-check-sso.html',
          checkLoginIframe: true,
          pkceMethod: 'S256',
        }),
        new Promise<false>((_, reject) => {
          timeoutId = window.setTimeout(
            () =>
              reject(new Error(`Keycloak init timed out after ${timeoutMs}ms`)),
            timeoutMs
          )
        }),
      ])
      this._initialized = true
      return authenticated
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : String(error)
      logger.error('Keycloak init failed', errorMessage)
      throw error
    } finally {
      if (timeoutId !== undefined) window.clearTimeout(timeoutId)
    }
  }

  async login(): Promise<void> {
    await this.init()
    await this.keycloak.login({
      redirectUri: window.location.origin,
    })
  }

  async logout(): Promise<void> {
    await this.init()
    await this.keycloak.logout({
      redirectUri: window.location.origin,
    })
  }

  async register(): Promise<void> {
    await this.init()
    await this.keycloak.register({
      redirectUri: window.location.origin,
    })
  }

  clearSession(): void {
    this.keycloak.clearToken()
  }

  async getToken(): Promise<string | undefined> {
    if (!this._initialized || !this.keycloak.authenticated) {
      return undefined
    }

    try {
      await this.keycloak.updateToken(TOKEN_MIN_VALIDITY_SECONDS)
      return this.keycloak.token
    } catch (error) {
      this.keycloak.clearToken()
      logger.warn(
        'token refresh failed',
        error instanceof Error ? error.message : error
      )
      return undefined
    }
  }

  isAuthenticated(): boolean {
    return this._initialized && (this.keycloak.authenticated ?? false)
  }

  hasRole(role: string): boolean {
    return this.keycloak.hasRealmRole(role)
  }

  getUserProfile(): UserProfile | null {
    const profile = this.keycloak.tokenParsed
    if (
      !profile ||
      typeof profile.sub !== 'string' ||
      profile.sub.length === 0
    ) {
      return null
    }

    const username =
      typeof profile.preferred_username === 'string'
        ? profile.preferred_username
        : ''
    const email = typeof profile.email === 'string' ? profile.email : ''

    return {
      id: profile.sub,
      username: username || email,
      email,
      firstName:
        typeof profile.given_name === 'string' ? profile.given_name : '',
      lastName:
        typeof profile.family_name === 'string' ? profile.family_name : '',
      emailVerified: profile.email_verified === true,
    }
  }

  onTokenExpired(callback: () => void): void {
    this.keycloak.onTokenExpired = () => {
      callback()
    }
  }

  onAuthRefreshSuccess(callback: () => void): void {
    this.keycloak.onAuthRefreshSuccess = () => {
      callback()
    }
  }

  onAuthRefreshError(callback: () => void): void {
    this.keycloak.onAuthRefreshError = () => {
      callback()
    }
  }

  onAuthLogout(callback: () => void): void {
    this.keycloak.onAuthLogout = callback
  }
}
