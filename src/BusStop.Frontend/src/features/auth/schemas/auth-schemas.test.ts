import { describe, expect, it } from 'vitest'
import { loginSchema, onboardingSchema, registerSchema } from './auth-schemas'

describe('loginSchema', () => {
  it('accepts a valid email and password', () => {
    const result = loginSchema.safeParse({
      email: 'user@example.com',
      password: 'secret',
    })

    expect(result.success).toBe(true)
  })

  it('rejects an invalid email', () => {
    const result = loginSchema.safeParse({
      email: 'not-an-email',
      password: 'secret',
    })

    expect(result.success).toBe(false)
    if (!result.success) {
      expect(result.error.issues[0]?.path).toEqual(['email'])
    }
  })

  it('rejects an empty password', () => {
    const result = loginSchema.safeParse({
      email: 'user@example.com',
      password: '',
    })

    expect(result.success).toBe(false)
    if (!result.success) {
      expect(result.error.issues[0]?.path).toEqual(['password'])
    }
  })
})

describe('registerSchema', () => {
  it('accepts valid email with matching passwords', () => {
    const result = registerSchema.safeParse({
      email: 'user@example.com',
      password: 'password123',
      confirmPassword: 'password123',
    })

    expect(result.success).toBe(true)
  })

  it('rejects passwords shorter than 8 characters', () => {
    const result = registerSchema.safeParse({
      email: 'user@example.com',
      password: 'short',
      confirmPassword: 'short',
    })

    expect(result.success).toBe(false)
    if (!result.success) {
      expect(result.error.issues[0]?.path).toEqual(['password'])
    }
  })

  it('rejects mismatched password confirmation on the confirmPassword field', () => {
    const result = registerSchema.safeParse({
      email: 'user@example.com',
      password: 'password123',
      confirmPassword: 'password456',
    })

    expect(result.success).toBe(false)
    if (!result.success) {
      expect(result.error.issues[0]?.path).toEqual(['confirmPassword'])
      expect(result.error.issues[0]?.message).toBe('Passwords do not match.')
    }
  })
})

describe('onboardingSchema', () => {
  it('accepts a valid username and country', () => {
    const result = onboardingSchema.safeParse({
      username: 'john.doe',
      countryId: 1,
    })

    expect(result.success).toBe(true)
  })

  it('rejects a non-numeric country id', () => {
    const result = onboardingSchema.safeParse({
      username: 'john.doe',
      countryId: '7',
    })

    expect(result.success).toBe(false)
    if (!result.success) {
      expect(result.error.issues[0]?.path).toEqual(['countryId'])
    }
  })

  it.each([
    { username: '', label: 'empty' },
    { username: 'ab', label: 'shorter than 3 characters' },
    { username: 'x'.repeat(51), label: 'longer than 50 characters' },
  ])('rejects a username that is $label', ({ username }) => {
    const result = onboardingSchema.safeParse({ username, countryId: 1 })

    expect(result.success).toBe(false)
    if (!result.success) {
      expect(result.error.issues[0]?.path).toEqual(['username'])
    }
  })

  it.each([{ countryId: 0 }, { countryId: -3 }])(
    'rejects an invalid country id ($countryId)',
    ({ countryId }) => {
      const result = onboardingSchema.safeParse({
        username: 'john.doe',
        countryId,
      })

      expect(result.success).toBe(false)
      if (!result.success) {
        expect(result.error.issues[0]?.path).toEqual(['countryId'])
      }
    }
  )
})
