interface AuthFormErrorProps {
  error: string | null
}

export function AuthFormError({ error }: AuthFormErrorProps) {
  if (!error) return null
  return (
    <p className='text-sm text-destructive text-center mb-4' role='alert'>
      {error}
    </p>
  )
}
