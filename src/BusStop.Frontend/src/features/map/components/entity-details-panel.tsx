import { useTranslation } from 'react-i18next'
import { Drawer } from 'vaul'
import { cn } from '@/lib/utils'
import { useIsMobile } from '@/hooks/use-mobile'
import { useMapUIStore } from '@/stores/map-ui-store'
import { ScrollArea } from '@/components/ui/scroll-area'
import { useSelectedEntity } from '../hooks/use-selected-entity'
import { StopDetails } from './stop-details'
import { RouteDetails } from './route-details'
import { DetailPanelHeader } from './detail-panel-header'
import type { Stop, Route } from '../types'

export function EntityDetailsPanel() {
  const isMobile = useIsMobile()
  const selectedEntity = useMapUIStore((s) => s.selectedEntity)
  const clearSelection = useMapUIStore((s) => s.clearSelection)
  const entity = useSelectedEntity()

  if (!selectedEntity || !entity) return null

  const { t } = useTranslation('map')
  const { t: tc } = useTranslation('common')

  const title =
    selectedEntity.type === 'stop'
      ? t('stopDetails')
      : t('routeDetails')

  const content = (
    <div className='flex h-full flex-col'>
      <DetailPanelHeader
        title={title}
        onClose={clearSelection}
        closeLabel={tc('close')}
      />
      <ScrollArea className='flex-1 px-4 py-4'>
        {selectedEntity.type === 'stop' ? (
          <StopDetails stop={entity as Stop} />
        ) : (
          <RouteDetails route={entity as Route} />
        )}
      </ScrollArea>
    </div>
  )

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
            {content}
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
      {content}
    </aside>
  )
}
