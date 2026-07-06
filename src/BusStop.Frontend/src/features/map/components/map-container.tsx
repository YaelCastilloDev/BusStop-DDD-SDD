import 'maplibre-gl/dist/maplibre-gl.css'
import { useEffect } from 'react'
import { useMapService } from '@/lib/adapters/maps'
import { useMapUIStore } from '@/stores/map-ui-store'
import type { MapEntityType } from '../types'
import { MOCK_STOPS, MOCK_ROUTES } from '../data/mock-data'

const DEFAULT_MAP_OPTIONS = {
  center: { lat: 40.7128, lng: -74.006 },
  zoom: 13,
  style: 'https://basemaps.cartocdn.com/gl/positron-gl-style/style.json',
  attributionControl: false,
}

export function MapContainer() {
  const { adapter, containerRef } = useMapService(DEFAULT_MAP_OPTIONS)
  const selectEntity = useMapUIStore((s) => s.selectEntity)
  const interactionMode = useMapUIStore((s) => s.interactionMode)

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

  useEffect(() => {
    adapter.setInteractionMode(interactionMode)

    if (interactionMode === 'add-stop') {
      const handleMapClick = (location: { lat: number; lng: number }) => {
        console.log('Map clicked at', location)
      }
      adapter.onMapClick(handleMapClick)
    } else {
      adapter.offMapClick()
    }
  }, [adapter, interactionMode])

  return (
    <div ref={containerRef} className='flex-1' />
  )
}
