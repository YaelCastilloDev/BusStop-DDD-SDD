import { Button } from '@/components/ui/button'

interface SubmitButtonProps {
  loading: boolean
  loadingText: string
  text: string
  disabled: boolean
}

export function SubmitButton({ loading, loadingText, text, disabled }: SubmitButtonProps) {
  return (
    <div className='my-5'>
      <Button
        type='submit'
        className='w-full h-10 px-5 py-2 text-sm font-medium shadow-none transition-colors hover:bg-primary-emphasis focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 ring-offset-background'
        disabled={loading || disabled}
      >
        {loading ? loadingText : text}
      </Button>
    </div>
  )
}
