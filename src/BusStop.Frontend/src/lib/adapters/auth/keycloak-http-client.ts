import { createLogger } from '@/lib/logger'

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
      const message =
        networkError instanceof Error ? networkError.message : 'Network error'
      this.logger.error('token request network error', message)
      throw new Error(
        'Unable to reach the authentication server. Please try again.'
      )
    }
  }
}

async function parseErrorResponse(
  response: Response,
  fallbackMessage: string
): Promise<string> {
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
