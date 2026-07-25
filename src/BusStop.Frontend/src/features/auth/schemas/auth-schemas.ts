import { z } from 'zod'

export const loginSchema = z.object({
  email: z.email('Enter a valid email address.'),
  password: z.string().min(1, 'Password is required.'),
})

export type LoginFormValues = z.infer<typeof loginSchema>

// Mirrors SignupValidator: password 8-100 chars.
export const registerSchema = z
  .object({
    email: z.email('Enter a valid email address.'),
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters.')
      .max(100, 'Password must be at most 100 characters.'),
    confirmPassword: z.string().min(1, 'Please confirm your password.'),
  })
  .refine((values) => values.password === values.confirmPassword, {
    message: 'Passwords do not match.',
    path: ['confirmPassword'],
  })

export type RegisterFormValues = z.infer<typeof registerSchema>

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
