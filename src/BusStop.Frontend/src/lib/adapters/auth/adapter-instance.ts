import type { IAuthAdapter } from './IAuthAdapter'
import { KeycloakAdapter } from './KeycloakAdapter'

let adapterInstance: IAuthAdapter | null = null

export function getAuthAdapter(): IAuthAdapter {
  if (!adapterInstance) {
    adapterInstance = new KeycloakAdapter()
  }
  return adapterInstance
}

export function getAuthToken(): Promise<string | undefined> {
  return getAuthAdapter().getToken()
}
