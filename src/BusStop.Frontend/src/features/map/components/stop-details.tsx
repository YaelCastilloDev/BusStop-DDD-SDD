import { useTranslation } from 'react-i18next'
import { Badge } from '@/components/ui/badge'
import { Bus } from 'lucide-react'
import type { Stop } from '../types'
import { getAssociatedRoutes } from '../data/entity-lookups'

export function StopDetails({ stop }: { stop: Stop }) {
  const { t } = useTranslation('map')
  const associatedRoutes = getAssociatedRoutes(stop.routeIds)

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
