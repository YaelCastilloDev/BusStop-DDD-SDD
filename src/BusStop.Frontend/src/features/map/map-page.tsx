import { SidebarProvider } from '@/components/ui/sidebar'
import { useMapUIStore } from '@/stores/map-ui-store'
import { TopBar } from '@/features/map/components/top-bar'
import { MainSidebar } from '@/features/map/components/main-sidebar'
import { MapLayout } from '@/features/map/components/map-layout'
import { MapContainer } from '@/features/map/components/map-container'
import { EntityDetailsPanel } from '@/features/map/components/entity-details-panel'

export function MapPage() {
  const sidebarCollapsed = useMapUIStore((s) => s.sidebarCollapsed)
  const setSidebarCollapsed = useMapUIStore((s) => s.setSidebarCollapsed)

  return (
    <SidebarProvider
      defaultOpen={!sidebarCollapsed}
      open={!sidebarCollapsed}
      onOpenChange={(open) => setSidebarCollapsed(!open)}
    >
      <div className='flex h-svh flex-col w-full'>
        <TopBar />
        <div className='flex flex-1 overflow-hidden'>
          <MainSidebar />
          <MapLayout>
            <MapContainer />
            <EntityDetailsPanel />
          </MapLayout>
        </div>
      </div>
    </SidebarProvider>
  )
}
