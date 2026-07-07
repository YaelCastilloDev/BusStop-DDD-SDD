import type { ReactNode } from 'react'
import { cn } from '@/lib/utils'
import { useMapUIStore } from '@/stores/map-ui-store'

interface MapLayoutProps {
  children: ReactNode
}

export function MapLayout({ children }: MapLayoutProps) {
  const detailsPanelOpen = useMapUIStore((s) => s.detailsPanelOpen)

  return (
    <div
      className={cn(
        'absolute inset-0 flex overflow-hidden',
        detailsPanelOpen && 'md:pr-[380px]'
      )}
    >
      {children}
    </div>
  )
}
