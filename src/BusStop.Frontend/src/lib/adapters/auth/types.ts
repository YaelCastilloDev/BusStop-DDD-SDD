export interface AuthState {
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}

export interface UserProfile {
  id: string
  username: string
  email: string
  firstName: string
  lastName: string
}

export type BusStopRole =
  | 'RegisteredUser'
  | 'Curator'
  | 'SubAdmin'
  | 'Admin'

export interface DirectRegisterRequest {
  firstName: string
  lastName: string
  email: string
  username: string
  password: string
}
