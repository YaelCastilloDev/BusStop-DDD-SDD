import { type Map as MaplibreMap } from 'maplibre-gl'
import type { InteractionMode, LatLng, MapEntityType } from '@/features/map/types'

export class MapInteractionManager {
  private markerClickHandler: ((type: MapEntityType, id: string) => void) | null = null
  private mapClickHandler: ((location: LatLng) => void) | null = null

  private handleMapClick = (e: { lngLat: { lat: number; lng: number } }): void => {
    this.mapClickHandler?.({
      lat: e.lngLat.lat,
      lng: e.lngLat.lng,
    })
  }

  getMarkerClickFallback(): ((type: MapEntityType, id: string) => void) | null {
    return this.markerClickHandler
  }

  onMarkerClick(handler: (entityType: MapEntityType, entityId: string) => void): void {
    this.markerClickHandler = handler
  }

  offMarkerClick(): void {
    this.markerClickHandler = null
  }

  onMapClick(map: MaplibreMap, handler: (location: LatLng) => void): void {
    this.mapClickHandler = handler
    map.on('click', this.handleMapClick)
  }

  offMapClick(map: MaplibreMap): void {
    this.mapClickHandler = null
    map.off('click', this.handleMapClick)
  }

  setInteractionMode(map: MaplibreMap, mode: InteractionMode): void {
    const canvas = map.getCanvas()
    if (mode === 'add-stop') {
      canvas.style.cursor = 'crosshair'
    } else {
      canvas.style.cursor = ''
    }
  }

  destroy(): void {
    this.markerClickHandler = null
    this.mapClickHandler = null
  }
}
