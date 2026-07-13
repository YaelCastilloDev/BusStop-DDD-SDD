import type { ChangeEvent } from 'react'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

const INPUT_CLASS = 'h-10 rounded-lg py-2 shadow-none text-sm !border focus-visible:outline-0 focus-visible:ring-0 focus-visible:border-primary'

interface FormFieldProps {
  id: string
  label: string
  type?: string
  autoComplete?: string
  autoFocus?: boolean
  value: string
  onChange: (e: ChangeEvent<HTMLInputElement>) => void
  disabled?: boolean
  name?: string
  className?: string
}

export function FormField({
  id,
  label,
  type = 'text',
  autoComplete,
  autoFocus,
  value,
  onChange,
  disabled,
  name,
  className = 'mb-4',
}: FormFieldProps) {
  return (
    <div className={className}>
      <div className='mb-2 block'>
        <Label htmlFor={id} className='font-semibold'>{label}</Label>
      </div>
      <Input
        id={id}
        name={name ?? id}
        type={type}
        autoFocus={autoFocus}
        autoComplete={autoComplete}
        value={value}
        onChange={onChange}
        disabled={disabled}
        className={INPUT_CLASS}
      />
    </div>
  )
}
