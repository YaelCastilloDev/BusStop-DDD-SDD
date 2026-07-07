import type { ReactNode } from 'react'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { BusTrailCanvas } from './BusTrailCanvas'

interface AuthCardLayoutProps {
  title: string
  description?: string
  children: ReactNode
  footer?: ReactNode
}

export function AuthCardLayout({
  title,
  description,
  children,
  footer,
}: AuthCardLayoutProps) {
  return (
    <div className='relative flex min-h-svh items-center justify-center bg-background p-4'>
      <div className='absolute inset-0 bg-gradient-to-br from-muted/50 via-background to-muted/30'>
        <BusTrailCanvas />
      </div>

      <Card className='relative z-10 w-full max-w-md shadow-lg'>
        <CardHeader className='space-y-1 text-center'>
          <CardTitle className='text-h2'>{title}</CardTitle>
          {description ? (
            <CardDescription className='text-body-sm'>
              {description}
            </CardDescription>
          ) : null}
        </CardHeader>
        <CardContent>{children}</CardContent>
        {footer ? (
          <div className='border-t px-6 pb-6 pt-4'>{footer}</div>
        ) : null}
      </Card>
    </div>
  )
}
