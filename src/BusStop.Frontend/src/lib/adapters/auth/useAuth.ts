import { useEffect, useCallback } from 'react'
import { useAuthStore, type AuthStore } from './auth-store'
import { getAuthAdapter, isInitStarted, markInitStarted } from './adapter-instance'
import type { IAuthAdapter } from './IAuthAdapter'
import type { BusStopRole, DirectRegisterRequest } from './types'

function getErrorMessage(err: unknown, fallback: string): string {
  return err instanceof Error ? err.message : fallback
}

function initializeAuth(auth: IAuthAdapter, store: AuthStore) {
  if (isInitStarted()) return
  markInitStarted()

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
}

export function useAuth() {
  const auth = getAuthAdapter()
  const store = useAuthStore()

  useEffect(() => {
    initializeAuth(auth, store)
  }, [])

  const login = useCallback(async () => {
    try {
      store.setLoading(true)
      await auth.login()
    } catch {
      store.setError('Login failed')
    }
  }, [])

  const directLogin = useCallback(async (username: string, password: string) => {
    store.setLoading(true)
    try {
      await auth.directLogin(username, password)
      const user = auth.getUserProfile()
      store.setAuthenticated(user)
    } catch (err) {
      const message = getErrorMessage(err, 'Login failed')
      store.setError(message)
      store.setLoading(false)
      throw err
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

  const directRegister = useCallback(async (userData: DirectRegisterRequest) => {
    store.setLoading(true)
    store.setError(null)
    try {
      await auth.directRegister(userData)
      const user = auth.getUserProfile()
      store.setAuthenticated(user)
    } catch (err) {
      const message = getErrorMessage(err, 'Registration failed')
      store.setError(message)
      store.setLoading(false)
      throw err
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
    directLogin,
    logout,
    register,
    directRegister,
    getToken,
    hasRole,
  }
}
