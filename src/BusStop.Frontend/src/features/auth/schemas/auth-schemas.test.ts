import { describe, expect, it } from 'vitest'
import { onboardingSchema } from './auth-schemas'

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
