import { useMapUIStore } from '@/stores/map-ui-store'
import { SidebarProvider } from '@/components/ui/sidebar'
import { TopBar } from '@/components/top-bar'
import { OnboardingGate } from '@/features/auth'
import { EntityDetailsPanel } from '@/features/map/components/entity-details-panel'
import { MainSidebar } from '@/features/map/components/main-sidebar'
import { MapContainer } from '@/features/map/components/map-container'
import { MapLayout } from '@/features/map/components/map-layout'

export function MapPage() {
  const sidebarCollapsed = useMapUIStore((s) => s.sidebarCollapsed)
  const setSidebarCollapsed = useMapUIStore((s) => s.setSidebarCollapsed)

  return (
    <SidebarProvider
      defaultOpen={!sidebarCollapsed}
      open={!sidebarCollapsed}
      onOpenChange={(open) => setSidebarCollapsed(!open)}
    >
      <div className='flex h-svh w-full flex-col'>
        <TopBar />
        <div className='relative flex-1 overflow-hidden'>
          <MainSidebar />
          <MapLayout>
            <MapContainer />
            <EntityDetailsPanel />
          </MapLayout>
        </div>
      </div>
      <OnboardingGate />
    </SidebarProvider>
  )
}
