import Keycloak from 'keycloak-js'
import { createLogger } from '@/lib/logger'
import type { IAuthAdapter } from './IAuthAdapter'
import { KeycloakHttpClient, parseTokenError } from './keycloak-http-client'
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
  private http: KeycloakHttpClient
  private _initialized = false
  private _initPromise: Promise<boolean> | null = null

  constructor() {
    this.keycloak = new Keycloak({
      url: import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8080',
      realm: import.meta.env.VITE_KEYCLOAK_REALM ?? 'auth-demo',
      clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'busstop-api',
    })

    this.http = new KeycloakHttpClient(
      this.keycloak.authServerUrl ?? 'http://localhost:8080',
      this.keycloak.realm ?? 'auth-demo',
      this.keycloak.clientId ?? 'busstop-api'
    )
  }

  async init(): Promise<boolean> {
    if (this._initPromise) {
      return this._initPromise
    }

    this._initPromise = this._doInit()
    return this._initPromise
  }

  private async _doInit(): Promise<boolean> {
    const timeoutMs = getInitTimeoutMs()

    try {
      const authenticated = await Promise.race([
        this.keycloak.init({
          onLoad: 'check-sso',
          silentCheckSsoRedirectUri:
            window.location.origin + '/silent-check-sso.html',
          checkLoginIframe: false,
          pkceMethod: 'S256',
        }),
        new Promise<false>((_, reject) =>
          setTimeout(
            () =>
              reject(new Error(`Keycloak init timed out after ${timeoutMs}ms`)),
            timeoutMs
          )
        ),
      ])
      this._initialized = true
      return authenticated
    } catch (error) {
      this._initialized = true
      const errorMessage =
        error instanceof Error ? error.message : String(error)
      logger.error('Keycloak init failed', errorMessage)
      return false
    }
  }

  async login(): Promise<void> {
    await this.keycloak.login({
      redirectUri: window.location.origin,
    })
  }

  async directLogin(username: string, password: string): Promise<void> {
    const response = await this.http.requestToken(username, password)

    if (!response.ok) {
      const errorMessage = await parseTokenError(response)
      throw new Error(errorMessage)
    }

    const data = await response.json()

    this.keycloak.token = data.access_token
    this.keycloak.refreshToken = data.refresh_token
    this.keycloak.idToken = data.id_token
    this.keycloak.authenticated = true
    this._initialized = true
  }

  async logout(): Promise<void> {
    await this.keycloak.logout({
      redirectUri: window.location.origin,
    })
  }

  async register(): Promise<void> {
    await this.keycloak.register({
      redirectUri: window.location.origin,
    })
  }

  discardSession(): void {
    this.keycloak.token = undefined
    this.keycloak.refreshToken = undefined
    this.keycloak.idToken = undefined
    this.keycloak.authenticated = false
  }

  async getToken(): Promise<string | undefined> {
    if (!this._initialized) {
      return undefined
    }

    try {
      await this.keycloak.updateToken(TOKEN_MIN_VALIDITY_SECONDS)
      return this.keycloak.token
    } catch (error) {
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
    if (!profile) return null

    return {
      id: profile.sub ?? '',
      username: (profile.preferred_username as string) ?? '',
      email: (profile.email as string) ?? '',
      firstName: (profile.given_name as string) ?? '',
      lastName: (profile.family_name as string) ?? '',
      emailVerified: (profile.email_verified as boolean) ?? false,
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
      this.keycloak.logout()
      callback()
    }
  }
}
