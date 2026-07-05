import { useTranslation } from 'react-i18next'
import { cn } from '@/lib/utils'
import { useIsMobile } from '@/hooks/use-mobile'
import { useMapUIStore } from '@/stores/map-ui-store'
import { Drawer } from 'vaul'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { ScrollArea } from '@/components/ui/scroll-area'
import { X, Bus, RouteIcon } from 'lucide-react'
import type { Stop, Route } from '../types'

const MOCK_STOPS: Stop[] = [
  {
    id: 'stop-1',
    name: 'Central Station',
    description: 'Main transit hub in downtown area.',
    location: { lat: 40.7128, lng: -74.006 },
    routeIds: ['route-1', 'route-2'],
  },
  {
    id: 'stop-2',
    name: 'Park Avenue',
    description: 'Busy stop near the park and shopping district.',
    location: { lat: 40.715, lng: -74.008 },
    routeIds: ['route-1'],
  },
  {
    id: 'stop-3',
    name: 'Riverside Blvd',
    description: 'Scenic stop along the river with access to ferry terminal.',
    location: { lat: 40.709, lng: -74.01 },
    routeIds: ['route-2'],
  },
  {
    id: 'stop-4',
    name: 'University Campus',
    description: 'Stop serving the main university campus.',
    location: { lat: 40.717, lng: -74.003 },
    routeIds: ['route-1', 'route-2'],
  },
]

const MOCK_ROUTES: Route[] = [
  {
    id: 'route-1',
    name: 'Line A - Downtown Express',
    description: 'Express service connecting Central Station to University Campus.',
    stopIds: ['stop-1', 'stop-2', 'stop-4'],
    color: '#3b82f6',
    coordinates: [
      { lat: 40.7128, lng: -74.006 },
      { lat: 40.715, lng: -74.008 },
      { lat: 40.717, lng: -74.003 },
    ],
  },
  {
    id: 'route-2',
    name: 'Line B - Riverside Local',
    description: 'Local service along the riverside connecting Central Station to Riverside Blvd.',
    stopIds: ['stop-1', 'stop-3', 'stop-4'],
    color: '#ef4444',
    coordinates: [
      { lat: 40.7128, lng: -74.006 },
      { lat: 40.709, lng: -74.01 },
      { lat: 40.717, lng: -74.003 },
    ],
  },
]

const stopMap = new Map(MOCK_STOPS.map((s) => [s.id, s]))
const routeMap = new Map(MOCK_ROUTES.map((r) => [r.id, r]))

function StopDetails({ stop }: { stop: Stop }) {
  const { t } = useTranslation('map')
  const associatedRoutes = stop.routeIds
    .map((id) => routeMap.get(id))
    .filter((r): r is Route => r !== undefined)

  return (
    <div className='space-y-4'>
      <div className='flex items-start justify-between gap-2'>
        <div className='flex items-center gap-2'>
          <Bus className='size-5 text-primary' />
          <div>
            <h3 className='font-semibold'>{stop.name}</h3>
            <Badge variant='secondary' className='mt-0.5'>
              {t('typeStop')}
            </Badge>
          </div>
        </div>
      </div>

      <p className='text-sm text-muted-foreground'>
        {stop.description || t('noDescription')}
      </p>

      <div className='text-xs text-muted-foreground'>
        <span className='font-medium'>Lat:</span> {stop.location.lat.toFixed(4)},{' '}
        <span className='font-medium'>Lng:</span> {stop.location.lng.toFixed(4)}
      </div>

      {associatedRoutes.length > 0 && (
        <div>
          <h4 className='mb-2 text-sm font-medium'>
            {t('associatedRoutes')} ({associatedRoutes.length})
          </h4>
          <div className='space-y-1.5'>
            {associatedRoutes.map((route) => (
              <div
                key={route.id}
                className='flex items-center gap-2 rounded-md border px-3 py-2 text-sm'
              >
                <div
                  className='size-2.5 shrink-0 rounded-full'
                  style={{ backgroundColor: route.color }}
                />
                <span className='truncate'>{route.name}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

function RouteDetails({ route }: { route: Route }) {
  const { t } = useTranslation('map')
  const orderedStops = route.stopIds
    .map((id) => stopMap.get(id))
    .filter((s): s is Stop => s !== undefined)

  return (
    <div className='space-y-4'>
      <div className='flex items-start justify-between gap-2'>
        <div className='flex items-center gap-2'>
          <RouteIcon className='size-5' style={{ color: route.color }} />
          <div>
            <h3 className='font-semibold'>{route.name}</h3>
            <Badge variant='secondary' className='mt-0.5'>
              {t('typeRoute')}
            </Badge>
          </div>
        </div>
      </div>

      <p className='text-sm text-muted-foreground'>
        {route.description || t('noDescription')}
      </p>

      {orderedStops.length > 0 && (
        <div>
          <h4 className='mb-2 text-sm font-medium'>
            {t('orderedStops')} ({orderedStops.length})
          </h4>
          <div className='space-y-1.5'>
            {orderedStops.map((stop, idx) => (
              <div
                key={stop.id}
                className='flex items-center gap-2 rounded-md border px-3 py-2 text-sm'
              >
                <span className='flex size-5 shrink-0 items-center justify-center rounded-full bg-muted text-xs font-medium tabular-nums'>
                  {idx + 1}
                </span>
                <span className='truncate'>{stop.name}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

function PanelContent() {
  const { t } = useTranslation('map')
  const selectedEntity = useMapUIStore((s) => s.selectedEntity)
  const clearSelection = useMapUIStore((s) => s.clearSelection)

  if (!selectedEntity) return null

  const entity =
    selectedEntity.type === 'stop'
      ? stopMap.get(selectedEntity.id)
      : routeMap.get(selectedEntity.id)

  if (!entity) return null

  return (
    <div className='flex h-full flex-col'>
      <div className='flex items-center justify-between border-b px-4 py-3'>
        <h2 className='text-sm font-semibold'>
          {selectedEntity.type === 'stop'
            ? t('stopDetails')
            : t('routeDetails')}
        </h2>
        <Button
          variant='ghost'
          size='icon'
          className='size-8'
          onClick={clearSelection}
          aria-label={t('common:close')}
        >
          <X className='size-4' />
        </Button>
      </div>
      <ScrollArea className='flex-1 px-4 py-4'>
        {selectedEntity.type === 'stop' ? (
          <StopDetails stop={entity as Stop} />
        ) : (
          <RouteDetails route={entity as Route} />
        )}
      </ScrollArea>
    </div>
  )
}

export function EntityDetailsPanel() {
  const isMobile = useIsMobile()
  const selectedEntity = useMapUIStore((s) => s.selectedEntity)
  const clearSelection = useMapUIStore((s) => s.clearSelection)

  if (!selectedEntity) return null

  if (isMobile) {
    return (
      <Drawer.Root
        open={!!selectedEntity}
        onOpenChange={(open) => {
          if (!open) clearSelection()
        }}
      >
        <Drawer.Portal>
          <Drawer.Overlay className='fixed inset-0 z-40 bg-black/40' />
          <Drawer.Content className='fixed inset-x-0 bottom-0 z-40 mt-24 flex max-h-[75vh] flex-col rounded-t-xl border bg-background'>
            <div className='mx-auto mt-3 h-1.5 w-10 shrink-0 rounded-full bg-muted' />
            <PanelContent />
          </Drawer.Content>
        </Drawer.Portal>
      </Drawer.Root>
    )
  }

  return (
    <aside
      className={cn(
        'absolute inset-y-0 right-0 z-40 w-[380px] border-l bg-background shadow-lg',
        'animate-in slide-in-from-right duration-300'
      )}
    >
      <PanelContent />
    </aside>
  )
}
