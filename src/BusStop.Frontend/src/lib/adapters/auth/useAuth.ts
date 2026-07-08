import { useEffect, useCallback } from 'react'
import { useAuthStore } from './auth-store'
import { KeycloakAdapter } from './KeycloakAdapter'
import type { IAuthAdapter } from './IAuthAdapter'
import type { BusStopRole } from './types'
import { createLogger } from '@/lib/logger'

const logger = createLogger('useAuth')

let adapterInstance: IAuthAdapter | null = null
let initStarted = false

function getAuthAdapter(): IAuthAdapter {
  if (!adapterInstance) {
    adapterInstance = new KeycloakAdapter()
  }
  return adapterInstance
}

export function useAuth() {
  const auth = getAuthAdapter()
  const store = useAuthStore()

  useEffect(() => {
    if (initStarted) return
    initStarted = true

    logger.info('starting auth initialization')
    store.setLoading(true)

    auth.onTokenExpired(() => {
      logger.warn('token expired callback triggered')
      auth.getToken().catch((err) => {
        logger.error('token refresh in onTokenExpired failed', err)
        store.clear()
      })
    })

    auth.onAuthRefreshSuccess(() => {
      logger.info('auth refresh success callback triggered')
      const user = auth.getUserProfile()
      store.setAuthenticated(user)
    })

    auth.onAuthRefreshError(() => {
      logger.error('auth refresh error callback triggered')
      store.clear()
    })

    auth
      .init()
      .then((authenticated) => {
        if (authenticated) {
          const user = auth.getUserProfile()
          logger.info('auth init complete: authenticated', {
            username: user?.username,
            id: user?.id,
          })
          store.setAuthenticated(user)
        } else {
          logger.info('auth init complete: not authenticated')
          store.setLoading(false)
        }
      })
      .catch((err) => {
        logger.error('auth init failed', err)
        store.setError('Failed to initialize authentication')
        store.setLoading(false)
      })
  }, [])

  const login = useCallback(async () => {
    logger.info('login requested')
    try {
      store.setLoading(true)
      await auth.login()
    } catch (err) {
      logger.error('login failed', err)
      store.setError('Login failed')
    }
  }, [])

  const directLogin = useCallback(async (username: string, password: string) => {
    logger.info('direct login requested', { username })
    store.setLoading(true)
    try {
      await auth.directLogin(username, password)
      const user = auth.getUserProfile()
      logger.info('direct login succeeded', { username: user?.username })
      store.setAuthenticated(user)
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Login failed'
      logger.error('direct login failed', message)
      store.setError(message)
      store.setLoading(false)
      throw err
    }
  }, [])

  const logout = useCallback(async () => {
    logger.info('logout requested')
    try {
      await auth.logout()
      store.clear()
    } catch (err) {
      logger.error('logout failed', err)
      store.setError('Logout failed')
    }
  }, [])

  const register = useCallback(async () => {
    logger.info('registration requested')
    try {
      store.setLoading(true)
      await auth.register()
    } catch (err) {
      logger.error('registration failed', err)
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
    directLogin,
    logout,
    register,
    getToken,
    hasRole,
  }
}

export function getAuthToken(): Promise<string | undefined> {
  return getAuthAdapter().getToken()
}
