import { Controller, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { useAuth } from '@/lib/adapters/auth'
import { ApiError, completeOnboarding } from '@/lib/api/auth-api'
import type { BusStopUser } from '@/lib/api/types'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { AuthFormError } from '@/components/auth/auth-form-error'
import { FormField } from '@/components/auth/form-field'
import { SubmitButton } from '@/components/auth/submit-button'
import { useCountries } from '@/features/auth/hooks/use-countries'
import {
  onboardingSchema,
  type OnboardingFormValues,
} from '@/features/auth/schemas/auth-schemas'

interface OnboardingFormProps {
  enabled: boolean
}

export function OnboardingForm({ enabled }: OnboardingFormProps) {
  const { user } = useAuth()
  const queryClient = useQueryClient()
  const countries = useCountries(enabled)

  const {
    register,
    control,
    handleSubmit,
    setError,
    clearErrors,
    formState: { errors },
  } = useForm<OnboardingFormValues>({
    resolver: zodResolver(onboardingSchema),
    defaultValues: { username: '', countryId: 0 },
  })

  const mutation = useMutation({
    mutationFn: completeOnboarding,
    onSuccess: (updated: BusStopUser) => {
      queryClient.setQueryData(['me', user?.id, user?.email], updated)
      toast.success('Your profile is ready. Welcome to BusStop!')
    },
    onError: (error) => {
      if (error instanceof ApiError) {
        if (error.fieldErrors.username) {
          setError('username', { message: error.fieldErrors.username })
        }
        if (error.fieldErrors.countryId) {
          setError('countryId', { message: error.fieldErrors.countryId })
        }
        // Backend returns domain errors (e.g. duplicate username) as general
        // errors — surface username-related ones on the field itself.
        if (!error.fieldErrors.username && /username/i.test(error.message)) {
          setError('username', { message: error.message })
          return
        }
        setError('root', { message: error.message })
        return
      }
      setError('root', {
        message: 'Failed to complete onboarding. Please try again.',
      })
    },
  })

  const onSubmit = (values: OnboardingFormValues) => {
    clearErrors('root')
    mutation.mutate({
      username: values.username.trim(),
      countryId: values.countryId,
    })
  }

  const countriesLoading = countries.isPending
  const countriesFailed = countries.isError

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className='flex flex-col'
      noValidate
    >
      <AuthFormError error={errors.root?.message ?? null} />

      <FormField
        id='onboarding-username'
        label='Username'
        autoComplete='username'
        autoFocus
        disabled={mutation.isPending}
        error={errors.username?.message}
        {...register('username')}
      />

      <div className='mb-4'>
        <div className='mb-2 block'>
          <Label htmlFor='onboarding-country' className='font-semibold'>
            Country
          </Label>
        </div>
        <Controller
          control={control}
          name='countryId'
          render={({ field }) => (
            <Select
              value={field.value > 0 ? String(field.value) : ''}
              onValueChange={(value) => field.onChange(Number(value))}
              disabled={
                mutation.isPending || countriesLoading || countriesFailed
              }
            >
              <SelectTrigger
                id='onboarding-country'
                aria-invalid={!!errors.countryId}
                className='h-10 w-full rounded-lg text-sm shadow-none'
              >
                <SelectValue
                  placeholder={
                    countriesLoading
                      ? 'Loading countries...'
                      : countriesFailed
                        ? 'Could not load countries'
                        : 'Select your country'
                  }
                />
              </SelectTrigger>
              <SelectContent>
                {(countries.data ?? []).map((country) => (
                  <SelectItem key={country.id} value={String(country.id)}>
                    {country.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        />
        {errors.countryId ? (
          <p className='mt-1 text-sm text-destructive' role='alert'>
            {errors.countryId.message}
          </p>
        ) : null}
        {countriesFailed ? (
          <p className='mt-1 text-sm text-destructive' role='alert'>
            Failed to load countries.{' '}
            <button
              type='button'
              className='font-medium text-primary hover:underline'
              onClick={() => countries.refetch()}
            >
              Retry
            </button>
          </p>
        ) : null}
      </div>

      <SubmitButton
        loading={mutation.isPending}
        loadingText='Saving...'
        text='Continue'
        disabled={false}
      />
    </form>
  )
}
