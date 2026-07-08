import Keycloak from 'keycloak-js'
import type { IAuthAdapter } from './IAuthAdapter'
import type { UserProfile } from './types'
import { createLogger } from '@/lib/logger'

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
      clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'busstop-api',
    })
    logger.debug('adapter created', {
      url: import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8080',
      realm: import.meta.env.VITE_KEYCLOAK_REALM ?? 'auth-demo',
      clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'busstop-api',
    })
  }

  async init(): Promise<boolean> {
    if (this._initPromise) {
      logger.debug('init already in progress, reusing existing promise')
      return this._initPromise
    }

    logger.info('starting Keycloak initialization')
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
      logger.info('Keycloak initialized successfully', { authenticated })
      return authenticated
    } catch (error) {
      this._initialized = true
      const errorMessage = error instanceof Error ? error.message : String(error)
      logger.error('Keycloak init failed', errorMessage)
      return false
    }
  }

  async login(): Promise<void> {
    logger.info('redirecting to Keycloak login')
    await this.keycloak.login({
      redirectUri: window.location.origin,
    })
  }

  async directLogin(username: string, password: string): Promise<void> {
    logger.info('attempting direct grant login', { username })

    const tokenUrl = `${this.keycloak.authServerUrl}/realms/${this.keycloak.realm}/protocol/openid-connect/token`

    const body = new URLSearchParams({
      grant_type: 'password',
      client_id: this.keycloak.clientId!,
      username,
      password,
    })

    let response: Response
    try {
      response = await fetch(tokenUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: body.toString(),
      })
    } catch (networkError) {
      const message = networkError instanceof Error ? networkError.message : 'Network error'
      logger.error('direct login network error', message)
      throw new Error('Unable to reach the authentication server. Please try again.', { cause: networkError })
    }

    if (!response.ok) {
      let errorMessage = 'Login failed. Please check your credentials.'
      try {
        const errorData = await response.json()
        if (errorData.error_description) {
          errorMessage = errorData.error_description
        } else if (errorData.error) {
          errorMessage = errorData.error
        }
      } catch {
        if (response.status === 401) {
          errorMessage = 'Invalid username or password.'
        }
      }

      logger.error('direct login rejected', {
        username,
        status: response.status,
        error: errorMessage,
      })
      throw new Error(errorMessage)
    }

    const data: {
      access_token: string
      refresh_token: string
      id_token: string
      expires_in: number
      refresh_expires_in: number
      token_type: string
    } = await response.json()

    this.keycloak.token = data.access_token
    this.keycloak.refreshToken = data.refresh_token
    this.keycloak.idToken = data.id_token
    this.keycloak.authenticated = true
    this._initialized = true

    logger.info('direct login successful', { username, expiresIn: data.expires_in })
  }

  async logout(): Promise<void> {
    logger.info('redirecting to Keycloak logout')
    await this.keycloak.logout({
      redirectUri: window.location.origin,
    })
  }

  async register(): Promise<void> {
    logger.info('redirecting to Keycloak registration')
    await this.keycloak.register({
      redirectUri: window.location.origin,
    })
  }

  async getToken(): Promise<string | undefined> {
    if (!this._initialized) {
      logger.debug('getToken skipped: adapter not initialized')
      return undefined
    }

    try {
      await this.keycloak.updateToken(TOKEN_MIN_VALIDITY_SECONDS)
      logger.debug('token refreshed successfully')
      return this.keycloak.token
    } catch (error) {
      logger.warn('token refresh failed', error instanceof Error ? error.message : error)
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
    }
  }

  onTokenExpired(callback: () => void): void {
    this.keycloak.onTokenExpired = () => {
      logger.warn('token expired, attempting refresh')
      callback()
    }
  }

  onAuthRefreshSuccess(callback: () => void): void {
    this.keycloak.onAuthRefreshSuccess = () => {
      logger.info('auth refresh succeeded')
      callback()
    }
  }

  onAuthRefreshError(callback: () => void): void {
    this.keycloak.onAuthRefreshError = () => {
      logger.error('auth refresh failed, logging out')
      this.keycloak.logout()
      callback()
    }
  }
}
