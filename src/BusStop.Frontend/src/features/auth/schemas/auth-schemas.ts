import { z } from 'zod'

// Mirrors OnboardingValidator: username 3-50 chars, countryId > 0.
export const onboardingSchema = z.object({
  username: z
    .string()
    .trim()
    .min(3, 'Username must be at least 3 characters.')
    .max(50, 'Username must be at most 50 characters.'),
  countryId: z
    .number('Please select a country.')
    .int()
    .min(1, 'Please select a country.'),
})

export type OnboardingFormValues = z.infer<typeof onboardingSchema>
