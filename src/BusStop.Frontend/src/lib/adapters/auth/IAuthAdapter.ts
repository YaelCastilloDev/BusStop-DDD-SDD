import type { DirectRegisterRequest, UserProfile } from './types'

export interface IAuthAdapter {
  init(): Promise<boolean>
  login(): Promise<void>
  directLogin(username: string, password: string): Promise<void>
  logout(): Promise<void>
  register(): Promise<void>
  directRegister(userData: DirectRegisterRequest): Promise<void>
  getToken(): Promise<string | undefined>
  isAuthenticated(): boolean
  hasRole(role: string): boolean
  getUserProfile(): UserProfile | null
  onTokenExpired(callback: () => void): void
  onAuthRefreshSuccess(callback: () => void): void
  onAuthRefreshError(callback: () => void): void
}
