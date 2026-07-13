import { createLogger } from '@/lib/logger'
import type { DirectRegisterRequest } from './types'

export class KeycloakHttpClient {
  private logger = createLogger('KeycloakHttpClient')

  constructor(
    private baseUrl: string,
    private realm: string,
    private clientId: string
  ) {}

  get tokenUrl(): string {
    return `${this.baseUrl}/realms/${this.realm}/protocol/openid-connect/token`
  }

  get usersUrl(): string {
    return `${this.baseUrl}/admin/realms/${this.realm}/users`
  }

  async requestToken(username: string, password: string): Promise<Response> {
    const body = new URLSearchParams({
      grant_type: 'password',
      client_id: this.clientId,
      username,
      password,
    })

    try {
      return await fetch(this.tokenUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: body.toString(),
      })
    } catch (networkError) {
      const message = networkError instanceof Error ? networkError.message : 'Network error'
      this.logger.error('token request network error', message)
      throw new Error('Unable to reach the authentication server. Please try again.')
    }
  }

  async createUser(adminToken: string, userData: DirectRegisterRequest): Promise<Response> {
    try {
      return await fetch(this.usersUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${adminToken}`,
        },
        body: JSON.stringify({
          firstName: userData.firstName,
          lastName: userData.lastName,
          email: userData.email,
          username: userData.username,
          enabled: true,
          credentials: [{
            type: 'password',
            value: userData.password,
            temporary: false,
          }],
        }),
      })
    } catch (networkError) {
      const message = networkError instanceof Error ? networkError.message : 'Network error'
      this.logger.error('createUser network error', message)
      throw new Error('Unable to reach the authentication server. Please try again.')
    }
  }
}

async function parseErrorResponse(response: Response, fallbackMessage: string): Promise<string> {
  try {
    const errorData = await response.json()
    if (errorData.error_description) return errorData.error_description
    if (errorData.errorMessage) return errorData.errorMessage
    if (errorData.error) return errorData.error
  } catch {
    // response body is not valid JSON
  }
  return fallbackMessage
}

export async function parseTokenError(response: Response): Promise<string> {
  const fallback =
    response.status === 401
      ? 'Invalid username or password.'
      : 'Login failed. Please check your credentials.'
  return parseErrorResponse(response, fallback)
}

export async function parseUserCreationError(response: Response): Promise<string> {
  const fallback =
    response.status === 409
      ? 'A user with this username or email already exists.'
      : 'Registration failed. Please try again.'
  return parseErrorResponse(response, fallback)
}
