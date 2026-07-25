import axios, { AxiosError, type AxiosResponse } from 'axios'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ApiError, getMe, parseApiError, signup } from './auth-api'
import type { BusStopUser } from './types'

vi.mock('axios', async (importOriginal) => {
  const actual = await importOriginal<typeof import('axios')>()
  return {
    ...actual,
    default: {
      ...actual.default,
      get: vi.fn(),
      post: vi.fn(),
    },
  }
})

const mockedGet = vi.mocked(axios.get)
const mockedPost = vi.mocked(axios.post)

function axiosError(status: number, data: unknown): AxiosError {
  return new AxiosError('Request failed', undefined, undefined, undefined, {
    status,
    data,
  } as AxiosResponse)
}

const sampleUser: BusStopUser = {
  id: 1,
  username: null,
  email: 'user@example.com',
  externalId: 'kc-sub-1',
  createdAt: '2026-01-01T00:00:00Z',
  countryId: null,
}

describe('getMe', () => {
  beforeEach(() => {
    vi.resetAllMocks()
  })

  it('returns the profile when the API responds 200', async () => {
    mockedGet.mockResolvedValue({ data: sampleUser })

    await expect(getMe()).resolves.toEqual(sampleUser)
  })

  it('returns null when the profile does not exist yet (404)', async () => {
    mockedGet.mockRejectedValue(axiosError(404, {}))

    await expect(getMe()).resolves.toBeNull()
  })

  it('throws an ApiError for non-404 failures', async () => {
    mockedGet.mockRejectedValue(
      axiosError(500, {
        message: 'boom',
        errors: { Error: ['Server exploded'] },
      })
    )

    const failure = await getMe().catch((error: unknown) => error)

    expect(failure).toBeInstanceOf(ApiError)
    expect((failure as ApiError).message).toBe('Server exploded')
    expect((failure as ApiError).status).toBe(500)
  })
})

describe('signup', () => {
  beforeEach(() => {
    vi.resetAllMocks()
  })

  it('resolves when the API accepts the signup', async () => {
    mockedPost.mockResolvedValue({ data: null })

    await expect(
      signup({ email: 'user@example.com', password: 'password123' })
    ).resolves.toBeUndefined()
  })

  it('surfaces the duplicate-email message from the API', async () => {
    mockedPost.mockRejectedValue(
      axiosError(400, {
        message: 'One or more errors occurred!',
        errors: { Error: ['A user with this email already exists.'] },
      })
    )

    const failure = await signup({
      email: 'user@example.com',
      password: 'password123',
    }).catch((error: unknown) => error)

    expect(failure).toBeInstanceOf(ApiError)
    expect((failure as ApiError).message).toBe(
      'A user with this email already exists.'
    )
  })
})

describe('parseApiError', () => {
  it('maps field errors to lowercased field names', () => {
    const error = axiosError(400, {
      message: 'One or more errors occurred!',
      errors: {
        Username: ['Username is already taken.'],
        CountryId: ['Country not found.'],
      },
    })

    const parsed = parseApiError(error, 'fallback')

    expect(parsed.fieldErrors).toEqual({
      username: 'Username is already taken.',
      countryId: 'Country not found.',
    })
  })

  it('treats the Error key as a general message', () => {
    const error = axiosError(400, {
      errors: { Error: ['Something went wrong.'] },
    })

    const parsed = parseApiError(error, 'fallback')

    expect(parsed.message).toBe('Something went wrong.')
    expect(parsed.fieldErrors).toEqual({})
  })

  it('falls back for non-axios errors', () => {
    const parsed = parseApiError(new Error('network down'), 'fallback')

    expect(parsed.message).toBe('network down')
    expect(parsed.status).toBeUndefined()
  })
})
