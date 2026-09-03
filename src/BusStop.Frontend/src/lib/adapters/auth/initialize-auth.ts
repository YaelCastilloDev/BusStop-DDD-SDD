import type { IAuthAdapter } from './IAuthAdapter'
import { getAuthAdapter } from './adapter-instance'
import { useAuthStore } from './auth-store'

let initialization: Promise<void> | null = null
let listenersRegistered = false

function synchronizeAuthenticatedUser(auth: IAuthAdapter): void {
  const store = useAuthStore.getState()
  const user = auth.getUserProfile()

  if (user) {
    store.setAuthenticated(user)
    return
  }

  auth.clearSession()
  store.setError('The identity provider returned an invalid user profile.')
}

function registerSessionListeners(auth: IAuthAdapter): void {
  if (listenersRegistered) return
  listenersRegistered = true

  auth.onTokenExpired(() => {
    void auth.getToken().then((token) => {
      if (!token) useAuthStore.getState().clear()
    })
  })

  auth.onAuthRefreshSuccess(() => synchronizeAuthenticatedUser(auth))
  auth.onAuthRefreshError(() => useAuthStore.getState().clear())
  auth.onAuthLogout(() => useAuthStore.getState().clear())
}

export function initializeAuth(): Promise<void> {
  if (initialization) return initialization

  const auth = getAuthAdapter()
  const store = useAuthStore.getState()
  registerSessionListeners(auth)
  store.setLoading(true)

  initialization = auth
    .init()
    .then((authenticated) => {
      if (authenticated) {
        synchronizeAuthenticatedUser(auth)
      } else {
        store.clear()
      }
    })
    .catch(() => {
      store.setError('Failed to initialize authentication')
    })

  return initialization
}
