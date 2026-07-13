import type { InteractionMode, LatLng, MapEntityType, Route, Stop } from '@/features/map/types'

export interface MapOptions {
  center: LatLng
  zoom: number
  style: string
  attributionControl?: false
}

export interface IMapAdapter {
  initialize(container: HTMLElement, options: MapOptions): void
  destroy(): void
  addStopMarker(stop: Stop, onClick?: (stopId: string) => void): void
  removeStopMarker(stopId: string): void
  drawRoute(route: Route): void
  removeRoute(routeId: string): void
  centerOnLocation(location: LatLng, zoom?: number): void
  fitBounds(bounds: LatLng[], padding?: number): void
  onMarkerClick(handler: (entityType: MapEntityType, entityId: string) => void): void
  offMarkerClick(): void
  setInteractionMode(mode: InteractionMode): void
  onMapClick(handler: (location: LatLng) => void): void
  offMapClick(): void
  addGeolocateControl(): void
  addFullscreenControl(): void
  addScaleControl(): void
}
