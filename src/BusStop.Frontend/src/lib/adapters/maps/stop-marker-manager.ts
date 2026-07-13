import { type Map as MaplibreMap, Marker } from 'maplibre-gl'
import type { Stop, MapEntityType } from '@/features/map/types'

export class StopMarkerManager {
  private markers: Map<string, Marker> = new Map()
  private clickFallback: ((type: MapEntityType, id: string) => void) | null = null

  setClickFallback(handler: ((type: MapEntityType, id: string) => void) | null): void {
    this.clickFallback = handler
  }

  add(map: MaplibreMap, stop: Stop, onClick?: (stopId: string) => void): void {
    const existingMarker = this.markers.get(stop.id)
    if (existingMarker) {
      existingMarker.remove()
    }

    const el = document.createElement('div')
    el.className = 'map-marker map-marker--stop'

    el.addEventListener('click', () => {
      if (onClick) {
        onClick(stop.id)
      } else {
        this.clickFallback?.('stop', stop.id)
      }
    })

    const marker = new Marker({ element: el })
      .setLngLat([stop.location.lng, stop.location.lat])
      .addTo(map)

    this.markers.set(stop.id, marker)
  }

  remove(stopId: string): void {
    const marker = this.markers.get(stopId)
    if (marker) {
      marker.remove()
      this.markers.delete(stopId)
    }
  }

  destroy(): void {
    this.markers.forEach((marker) => marker.remove())
    this.markers.clear()
    this.clickFallback = null
  }
}
