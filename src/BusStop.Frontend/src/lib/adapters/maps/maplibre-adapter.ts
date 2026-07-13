import maplibregl, {
  type Map as MaplibreMap,
  type GeolocateControl,
  type FullscreenControl,
  type ScaleControl,
} from 'maplibre-gl'
import type { IMapAdapter, MapOptions } from './types'
import type { InteractionMode, LatLng, MapEntityType, Route, Stop } from '@/features/map/types'
import { StopMarkerManager } from './stop-marker-manager'
import { RouteRenderer } from './route-renderer'
import { MapInteractionManager } from './map-interaction-manager'

export class MapLibreAdapter implements IMapAdapter {
  private map: MaplibreMap | null = null
  private initialized = false

  private geolocateControl: GeolocateControl | null = null
  private fullscreenControl: FullscreenControl | null = null
  private scaleControl: ScaleControl | null = null

  private stopMarkers = new StopMarkerManager()
  private routeRenderer = new RouteRenderer()
  private interaction = new MapInteractionManager()

  initialize(container: HTMLElement, options: MapOptions): void {
    if (this.initialized) return

    this.map = new maplibregl.Map({
      container,
      center: [options.center.lng, options.center.lat],
      zoom: options.zoom,
      style: options.style,
      attributionControl: options.attributionControl ?? false,
    })

    this.map.addControl(new maplibregl.NavigationControl(), 'top-left')

    this.map.once('style.load', () => {
      this.addBuiltInControls()
    })

    this.initialized = true
  }

  destroy(): void {
    this.stopMarkers.destroy()
    this.routeRenderer.destroy()
    this.interaction.destroy()

    this.geolocateControl = null
    this.fullscreenControl = null
    this.scaleControl = null

    if (this.map) {
      this.map.remove()
      this.map = null
    }

    this.initialized = false
  }

  addStopMarker(stop: Stop, onClick?: (stopId: string) => void): void {
    if (!this.map) return
    this.stopMarkers.add(this.map, stop, onClick)
  }

  removeStopMarker(stopId: string): void {
    this.stopMarkers.remove(stopId)
  }

  drawRoute(route: Route): void {
    if (!this.map) return
    this.routeRenderer.draw(this.map, route)
  }

  removeRoute(routeId: string): void {
    if (!this.map) return
    this.routeRenderer.remove(this.map, routeId)
  }

  centerOnLocation(location: LatLng, zoom?: number): void {
    if (!this.map) return

    this.map.flyTo({
      center: [location.lng, location.lat],
      zoom: zoom ?? 15,
      duration: 1500,
    })
  }

  fitBounds(bounds: LatLng[], padding = 50): void {
    if (!this.map || bounds.length === 0) return

    const coords = bounds.map((b) => [b.lng, b.lat] as [number, number])
    const mapBounds = coords.reduce(
      (mb, coord) => mb.extend(coord),
      new maplibregl.LngLatBounds(coords[0], coords[0])
    )

    this.map.fitBounds(mapBounds, {
      padding,
      duration: 1500,
    })
  }

  onMarkerClick(handler: (entityType: MapEntityType, entityId: string) => void): void {
    this.interaction.onMarkerClick(handler)
    this.stopMarkers.setClickFallback(handler)
  }

  offMarkerClick(): void {
    this.interaction.offMarkerClick()
    this.stopMarkers.setClickFallback(null)
  }

  setInteractionMode(mode: InteractionMode): void {
    if (!this.map) return
    this.interaction.setInteractionMode(this.map, mode)
  }

  onMapClick(handler: (location: LatLng) => void): void {
    if (!this.map) return
    this.interaction.onMapClick(this.map, handler)
  }

  offMapClick(): void {
    if (!this.map) return
    this.interaction.offMapClick(this.map)
  }

  addGeolocateControl(): void {
    if (!this.map || this.geolocateControl) return

    this.geolocateControl = new maplibregl.GeolocateControl({
      positionOptions: { enableHighAccuracy: true },
      trackUserLocation: true,
    })

    this.map.addControl(this.geolocateControl, 'bottom-right')
  }

  addFullscreenControl(): void {
    if (!this.map || this.fullscreenControl) return

    this.fullscreenControl = new maplibregl.FullscreenControl()
    this.map.addControl(this.fullscreenControl, 'top-right')
  }

  addScaleControl(): void {
    if (!this.map || this.scaleControl) return

    this.scaleControl = new maplibregl.ScaleControl({
      maxWidth: 120,
      unit: 'metric',
    })

    this.map.addControl(this.scaleControl, 'bottom-left')
  }

  private addBuiltInControls(): void {
    this.addGeolocateControl()
    this.addFullscreenControl()
    this.addScaleControl()
  }
}
