import type { ReactNode } from 'react'
import { BusTrailCanvas } from './BusTrailCanvas'

interface AuthCardLayoutProps {
  children: ReactNode
  description?: ReactNode
  footer?: ReactNode
  title?: ReactNode
}

export function AuthCardLayout({
  children,
  description,
  footer,
  title,
}: AuthCardLayoutProps) {
  return (
    <div className='relative flex min-h-svh items-center justify-center bg-background p-4'>
      <div className='absolute inset-0 bg-gradient-to-br from-muted/50 via-background to-muted/30'>
        <BusTrailCanvas />
      </div>

      <div className='relative z-10 flex w-full flex-col gap-6 rounded-lg border-none bg-card p-6 text-card-foreground shadow-none md:w-[450px]'>
        <div className='mx-auto mb-2'>
          <h1 className='text-2xl font-bold text-foreground dark:text-white'>
            BusStop
          </h1>
        </div>

        {title || description ? (
          <div className='text-center'>
            {title ? (
              <h2 className='text-xl font-semibold text-foreground'>{title}</h2>
            ) : null}
            {description ? (
              <p className='mt-2 text-sm text-muted-foreground'>
                {description}
              </p>
            ) : null}
          </div>
        ) : null}

        {children}

        {footer ? (
          <div className='mt-6 flex items-center justify-start gap-2 text-base font-medium'>
            {footer}
          </div>
        ) : null}
      </div>
    </div>
  )
}
