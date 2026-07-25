import { Mail } from 'lucide-react'

interface VerifyEmailNoticeProps {
  email: string
  onBackToSignIn: () => void
}

export function VerifyEmailNotice({
  email,
  onBackToSignIn,
}: VerifyEmailNoticeProps) {
  return (
    <div className='mt-2 flex flex-col items-center gap-4 text-center'>
      <div className='flex size-12 items-center justify-center rounded-full bg-muted'>
        <Mail className='size-6 text-primary' />
      </div>

      <h2 className='text-h3 text-foreground'>Verify your email</h2>

      <p className='text-body-sm text-muted-foreground'>
        We sent a verification link to{' '}
        <span className='font-medium text-foreground'>{email}</span>. Click the
        link in the email to activate your account, then sign in.
      </p>

      <button
        type='button'
        onClick={onBackToSignIn}
        className='text-sm font-medium text-primary hover:underline'
      >
        Back to sign in
      </button>
    </div>
  )
}
