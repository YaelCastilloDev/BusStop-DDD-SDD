import Keycloak from 'keycloak-js'
import type { IAuthAdapter } from './IAuthAdapter'
import type { UserProfile } from './types'

const TOKEN_MIN_VALIDITY_SECONDS = 30
const INIT_TIMEOUT_MS = 10_000

export class KeycloakAdapter implements IAuthAdapter {
  private keycloak: Keycloak
  private _initialized = false

  constructor() {
    this.keycloak = new Keycloak({
      url: import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8080',
      realm: import.meta.env.VITE_KEYCLOAK_REALM ?? 'auth-demo',
      clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'busstop-api',
    })
  }

  async init(): Promise<boolean> {
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
            () => reject(new Error('Keycloak init timed out')),
            INIT_TIMEOUT_MS
          )
        ),
      ])
      this._initialized = true
      return authenticated
    } catch (error) {
      console.error('Keycloak init failed:', error)
      this._initialized = true
      return false
    }
  }

  async login(): Promise<void> {
    await this.keycloak.login({
      redirectUri: window.location.origin,
    })
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

  async getToken(): Promise<string | undefined> {
    if (!this._initialized) {
      return undefined
    }

    try {
      await this.keycloak.updateToken(TOKEN_MIN_VALIDITY_SECONDS)
      return this.keycloak.token
    } catch {
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
    this.keycloak.onTokenExpired = callback
  }

  onAuthRefreshSuccess(callback: () => void): void {
    this.keycloak.onAuthRefreshSuccess = callback
  }

  onAuthRefreshError(callback: () => void): void {
    this.keycloak.onAuthRefreshError = () => {
      this.keycloak.logout()
      callback()
    }
  }
}
