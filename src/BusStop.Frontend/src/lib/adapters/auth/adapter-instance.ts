import type { IAuthAdapter } from './IAuthAdapter'
import { KeycloakAdapter } from './KeycloakAdapter'

let adapterInstance: IAuthAdapter | null = null
let initStarted = false

export function getAuthAdapter(): IAuthAdapter {
  if (!adapterInstance) {
    adapterInstance = new KeycloakAdapter()
  }
  return adapterInstance
}

export function isInitStarted(): boolean {
  return initStarted
}

export function markInitStarted(): void {
  initStarted = true
}

export function getAuthToken(): Promise<string | undefined> {
  return getAuthAdapter().getToken()
}
