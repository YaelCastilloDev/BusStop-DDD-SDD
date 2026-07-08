import type { ReactNode } from 'react'
import { BusTrailCanvas } from './BusTrailCanvas'

interface AuthCardLayoutProps {
  children: ReactNode
  footer?: ReactNode
}

export function AuthCardLayout({
  children,
  footer,
}: AuthCardLayoutProps) {
  return (
    <div className='relative flex min-h-svh items-center justify-center bg-background p-4'>
      <div className='absolute inset-0 bg-gradient-to-br from-muted/50 via-background to-muted/30'>
        <BusTrailCanvas />
      </div>

      <div className='relative z-10 flex w-full flex-col gap-6 rounded-lg border-none bg-card p-6 text-card-foreground shadow-none md:w-[450px]'>
        <div className='mx-auto mb-2'>
          <h1 className='text-2xl font-bold text-foreground dark:text-white'>BusStop</h1>
        </div>

        {children}

        {footer ? (
          <div className='flex gap-2 items-center justify-start text-base font-medium mt-6'>
            {footer}
          </div>
        ) : null}
      </div>
    </div>
  )
}
