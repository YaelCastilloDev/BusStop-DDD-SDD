import { useTranslation } from 'react-i18next'
import { Badge } from '@/components/ui/badge'
import { RouteIcon } from 'lucide-react'
import type { Route } from '../types'
import { getOrderedStops } from '../data/entity-lookups'

export function RouteDetails({ route }: { route: Route }) {
  const { t } = useTranslation('map')
  const orderedStops = getOrderedStops(route.stopIds)

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
