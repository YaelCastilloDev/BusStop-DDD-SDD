import { useEffect, useCallback } from 'react'
import { getAuthAdapter } from './adapter-instance'
import { useAuthStore } from './auth-store'
import { initializeAuth } from './initialize-auth'
import type { BusStopRole } from './types'

export function useAuth() {
  const auth = getAuthAdapter()
  const store = useAuthStore()

  useEffect(() => {
    void initializeAuth()
  }, [])

  const login = useCallback(async () => {
    try {
      store.setError(null)
      store.setLoading(true)
      await auth.login()
    } catch {
      store.setError('Login failed')
    }
  }, [auth, store])

  const logout = useCallback(async () => {
    try {
      store.setError(null)
      store.setLoading(true)
      await auth.logout()
      store.clear()
    } catch {
      store.setError('Logout failed')
    }
  }, [auth, store])

  const register = useCallback(async () => {
    try {
      store.setError(null)
      store.setLoading(true)
      await auth.register()
    } catch {
      store.setError('Registration failed')
    }
  }, [auth, store])

  const getToken = useCallback(async () => {
    return auth.getToken()
  }, [auth])

  const hasRole = useCallback(
    (role: BusStopRole | string) => {
      return auth.hasRole(role)
    },
    [auth]
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
