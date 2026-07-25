import axios, { AxiosError } from 'axios'
import type { BusStopUser } from './types'

export interface SignupRequest {
  email: string
  password: string
}

export interface OnboardingRequest {
  username: string
  countryId: number
}

export class ApiError extends Error {
  readonly status: number | undefined
  readonly fieldErrors: Record<string, string>

  constructor(
    message: string,
    status?: number,
    fieldErrors: Record<string, string> = {}
  ) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

// FastEndpoints error shape: { statusCode, message, errors: { "<field>": ["msg"], "Error": ["msg"] } }
// Domain errors surface under the "Error"/"generalErrors" key; validation errors under the property name.
export function parseApiError(error: unknown, fallback: string): ApiError {
  if (!(error instanceof AxiosError)) {
    return new ApiError(error instanceof Error ? error.message : fallback)
  }

  const status = error.response?.status
  const data = error.response?.data as
    | { message?: string; errors?: Record<string, string[]> }
    | undefined

  if (!data?.errors || typeof data.errors !== 'object') {
    return new ApiError(data?.message || fallback, status)
  }

  const fieldErrors: Record<string, string> = {}
  const generalMessages: string[] = []

  for (const [key, messages] of Object.entries(data.errors)) {
    if (!Array.isArray(messages)) continue
    if (
      key.toLowerCase() === 'error' ||
      key.toLowerCase() === 'generalerrors'
    ) {
      generalMessages.push(...messages)
    } else {
      // "Username" -> "username" so it matches form field names
      const field = key.charAt(0).toLowerCase() + key.slice(1)
      fieldErrors[field] = messages[0] ?? fallback
    }
  }

  const message = generalMessages[0] ?? data.message ?? fallback
  return new ApiError(message, status, fieldErrors)
}

export async function signup(request: SignupRequest): Promise<void> {
  try {
    await axios.post('/auth/signup', request)
  } catch (error) {
    throw parseApiError(error, 'Registration failed. Please try again.')
  }
}

export async function registerUser(email: string): Promise<BusStopUser> {
  try {
    const response = await axios.post<BusStopUser>('/auth/register', { email })
    return response.data
  } catch (error) {
    throw parseApiError(
      error,
      'Failed to create your profile. Please try again.'
    )
  }
}

// Returns null when the local profile does not exist yet (404) so callers can
// trigger POST /auth/register. Other errors are thrown.
export async function getMe(): Promise<BusStopUser | null> {
  try {
    const response = await axios.get<BusStopUser>('/auth/me')
    return response.data
  } catch (error) {
    if (error instanceof AxiosError && error.response?.status === 404) {
      return null
    }
    throw parseApiError(error, 'Failed to load your profile. Please try again.')
  }
}

export async function completeOnboarding(
  request: OnboardingRequest
): Promise<BusStopUser> {
  try {
    const response = await axios.post<BusStopUser>('/auth/onboarding', request)
    return response.data
  } catch (error) {
    throw parseApiError(
      error,
      'Failed to complete onboarding. Please try again.'
    )
  }
}
