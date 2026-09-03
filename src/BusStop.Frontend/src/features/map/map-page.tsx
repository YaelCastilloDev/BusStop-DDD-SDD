import { memo } from 'react'
import { useMapUIStore } from '@/stores/map-ui-store'
import { SidebarProvider } from '@/components/ui/sidebar'
import { OnboardingGate } from '@/features/auth'
import { DashboardHeader } from '@/features/map/components/dashboard-header'
import { DashboardSidebar } from '@/features/map/components/dashboard-sidebar'
import { EntityDetailsPanel } from '@/features/map/components/entity-details-panel'
import { MapContainer } from '@/features/map/components/map-container'
import { MapLayout } from '@/features/map/components/map-layout'

const MapViewport = memo(function MapViewport() {
  return (
    <MapLayout>
      <MapContainer />
      <EntityDetailsPanel />
    </MapLayout>
  )
})

export function MapPage() {
  const sidebarCollapsed = useMapUIStore((s) => s.sidebarCollapsed)
  const setSidebarCollapsed = useMapUIStore((s) => s.setSidebarCollapsed)

  return (
    <SidebarProvider
      open={!sidebarCollapsed}
      onOpenChange={(open) => setSidebarCollapsed(!open)}
    >
      <div className='flex h-svh w-full flex-col overflow-hidden'>
        <DashboardHeader />
        <div className='relative min-h-0 flex-1 overflow-hidden'>
          <MapViewport />
          <DashboardSidebar />
        </div>
      </div>
      <OnboardingGate />
    </SidebarProvider>
  )
}
