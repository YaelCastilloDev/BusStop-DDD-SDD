import type { InputHTMLAttributes } from 'react'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

const INPUT_CLASS =
  'h-10 rounded-lg py-2 shadow-none text-sm !border focus-visible:outline-0 focus-visible:ring-0 focus-visible:border-primary'

interface FormFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  id: string
  label: string
  error?: string
  wrapperClassName?: string
}

export function FormField({
  id,
  label,
  error,
  wrapperClassName = 'mb-4',
  ...inputProps
}: FormFieldProps) {
  return (
    <div className={wrapperClassName}>
      <div className='mb-2 block'>
        <Label htmlFor={id} className='font-semibold'>
          {label}
        </Label>
      </div>
      <Input
        id={id}
        name={inputProps.name ?? id}
        aria-invalid={!!error}
        className={INPUT_CLASS}
        {...inputProps}
      />
      {error ? (
        <p className='mt-1 text-sm text-destructive' role='alert'>
          {error}
        </p>
      ) : null}
    </div>
  )
}
