import 'maplibre-gl/dist/maplibre-gl.css'
import { useEffect } from 'react'
import { useMapService, type MapOptions } from '@/lib/adapters/maps'
import { useMapUIStore } from '@/stores/map-ui-store'
import { MOCK_STOPS, MOCK_ROUTES } from '../data/mock-data'

const MAPTILER_API_KEY = import.meta.env.VITE_MAPTILER_API_KEY as string | undefined

const MAP_STYLE_URL = MAPTILER_API_KEY
  ? `https://api.maptiler.com/maps/pastel/style.json?key=${MAPTILER_API_KEY}`
  : 'https://basemaps.cartocdn.com/gl/positron-gl-style/style.json'

const DEFAULT_MAP_OPTIONS: MapOptions = {
  center: { lat: 40.7128, lng: -74.006 },
  zoom: 13,
  style: MAP_STYLE_URL,
  attributionControl: false,
}

export function MapContainer() {
  const { adapter, containerRef } = useMapService(DEFAULT_MAP_OPTIONS)
  const selectEntity = useMapUIStore((s) => s.selectEntity)
  const interactionMode = useMapUIStore((s) => s.interactionMode)

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
        if (import.meta.env.DEV) {
          // eslint-disable-next-line no-console
          console.log('Map clicked at', location)
        }
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
