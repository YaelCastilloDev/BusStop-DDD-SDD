import { useEffect, useCallback, useRef } from 'react'
import { useAuthStore } from './auth-store'
import { KeycloakAdapter } from './KeycloakAdapter'
import type { IAuthAdapter } from './IAuthAdapter'
import type { BusStopRole } from './types'

let adapterInstance: IAuthAdapter | null = null

function getAuthAdapter(): IAuthAdapter {
  if (!adapterInstance) {
    adapterInstance = new KeycloakAdapter()
  }
  return adapterInstance
}

export function useAuth() {
  const auth = getAuthAdapter()
  const store = useAuthStore()
  const initStarted = useRef(false)

  useEffect(() => {
    if (initStarted.current) return
    initStarted.current = true

    store.setLoading(true)

    auth.onTokenExpired(() => {
      auth.getToken().catch(() => {
        store.clear()
      })
    })

    auth.onAuthRefreshSuccess(() => {
      const user = auth.getUserProfile()
      store.setAuthenticated(user)
    })

    auth.onAuthRefreshError(() => {
      store.clear()
    })

    auth
      .init()
      .then((authenticated) => {
        if (authenticated) {
          const user = auth.getUserProfile()
          store.setAuthenticated(user)
        } else {
          store.setLoading(false)
        }
      })
      .catch(() => {
        store.setError('Failed to initialize authentication')
        store.setLoading(false)
      })
  }, [])

  const login = useCallback(async () => {
    try {
      store.setLoading(true)
      await auth.login()
    } catch {
      store.setError('Login failed')
    }
  }, [])

  const logout = useCallback(async () => {
    try {
      await auth.logout()
      store.clear()
    } catch {
      store.setError('Logout failed')
    }
  }, [])

  const register = useCallback(async () => {
    try {
      store.setLoading(true)
      await auth.register()
    } catch {
      store.setError('Registration failed')
    }
  }, [])

  const getToken = useCallback(async () => {
    return auth.getToken()
  }, [])

  const hasRole = useCallback(
    (role: BusStopRole | string) => {
      return auth.hasRole(role)
    },
    []
  )

  return {
    isAuthenticated: store.isAuthenticated,
    isLoading: store.isLoading,
    error: store.error,
    user: store.user,
    login,
    logout,
    register,
    getToken,
    hasRole,
  }
}

export function getAuthToken(): Promise<string | undefined> {
  return getAuthAdapter().getToken()
}
