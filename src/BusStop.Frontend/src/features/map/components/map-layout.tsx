import type { ReactNode } from 'react'

interface MapLayoutProps {
  children: ReactNode
}

export function MapLayout({ children }: MapLayoutProps) {
  return <div className='absolute inset-0 flex overflow-hidden'>{children}</div>
}
