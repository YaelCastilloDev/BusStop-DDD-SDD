import { create } from 'zustand'
import type { AuthState, UserProfile } from './types'

interface AuthStore extends AuthState {
  user: UserProfile | null
  setAuthenticated: (user: UserProfile | null) => void
  setLoading: (isLoading: boolean) => void
  setError: (error: string | null) => void
  clear: () => void
}

export const useAuthStore = create<AuthStore>()((set) => ({
  isAuthenticated: false,
  isLoading: true,
  error: null,
  user: null,

  setAuthenticated: (user) =>
    set({
      isAuthenticated: user !== null,
      user,
      isLoading: false,
      error: null,
    }),

  setLoading: (isLoading) => set({ isLoading }),

  setError: (error) =>
    set({
      error,
      isLoading: false,
    }),

  clear: () =>
    set({
      isAuthenticated: false,
      user: null,
      isLoading: false,
      error: null,
    }),
}))
