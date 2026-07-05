import 'maplibre-gl/dist/maplibre-gl.css'
import { useEffect } from 'react'
import { useMapService } from '@/lib/adapters/maps'
import { useMapUIStore } from '@/stores/map-ui-store'
import type { MapEntityType, Stop, Route } from '../types'

const DEFAULT_MAP_OPTIONS = {
  center: { lat: 40.7128, lng: -74.006 },
  zoom: 13,
  style: 'https://basemaps.cartocdn.com/gl/positron-gl-style/style.json',
  attributionControl: false,
}

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

export function MapContainer() {
  const { adapter, containerRef } = useMapService(DEFAULT_MAP_OPTIONS)
  const selectEntity = useMapUIStore((s) => s.selectEntity)

  useEffect(() => {
    const handler = (entityType: MapEntityType, entityId: string) => {
      selectEntity(entityType, entityId)
    }
    adapter.onMarkerClick(handler)
    return () => {
      adapter.offMarkerClick()
    }
  }, [adapter, selectEntity])

  useEffect(() => {
    MOCK_STOPS.forEach((stop) => {
      adapter.addStopMarker(stop, (stopId) => {
        selectEntity('stop', stopId)
      })
    })
  }, [adapter, selectEntity])

  useEffect(() => {
    MOCK_ROUTES.forEach((route) => {
      adapter.drawRoute(route)
    })
  }, [adapter])

  return (
    <div className='relative flex-1 overflow-hidden'>
      <div
        ref={containerRef}
        className='absolute inset-0'
        style={{ zIndex: 0 }}
      />
      <div className='pointer-events-none absolute bottom-3 left-3 z-10 rounded-md bg-background/80 px-3 py-1.5 text-xs text-muted-foreground backdrop-blur'>
        MapLibre GL — BusStop
      </div>
    </div>
  )
}
