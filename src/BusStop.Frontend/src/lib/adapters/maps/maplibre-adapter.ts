import maplibregl, {
  type Map as MaplibreMap,
  type Marker,
  type GeolocateControl,
  type FullscreenControl,
  type ScaleControl,
} from 'maplibre-gl'
import type { IMapAdapter, MapOptions } from './types'
import type { InteractionMode, LatLng, MapEntityType, Route, Stop } from '@/features/map/types'

export class MapLibreAdapter implements IMapAdapter {
  private map: MaplibreMap | null = null
  private markers: Map<string, Marker> = new Map()
  private routeSources: Set<string> = new Set()
  private markerClickHandler: ((type: MapEntityType, id: string) => void) | null = null
  private mapClickHandler: ((location: LatLng) => void) | null = null
  private geolocateControl: GeolocateControl | null = null
  private fullscreenControl: FullscreenControl | null = null
  private scaleControl: ScaleControl | null = null
  private initialized = false

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
      this.addGeolocateControl()
      this.addFullscreenControl()
      this.addScaleControl()
    })

    this.initialized = true
  }

  destroy(): void {
    this.markers.forEach((marker: Marker) => marker.remove())
    this.markers.clear()
    this.routeSources.clear()
    this.markerClickHandler = null
    this.mapClickHandler = null
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
        this.markerClickHandler?.('stop', stop.id)
      }
    })

    const marker = new maplibregl.Marker({ element: el })
      .setLngLat([stop.location.lng, stop.location.lat])
      .addTo(this.map)

    this.markers.set(stop.id, marker)
  }

  removeStopMarker(stopId: string): void {
    const marker = this.markers.get(stopId)
    if (marker) {
      marker.remove()
      this.markers.delete(stopId)
    }
  }

  drawRoute(route: Route): void {
    if (!this.map) return

    const sourceId = `route-${route.id}`
    const layerId = `route-layer-${route.id}`

    if (this.routeSources.has(sourceId)) {
      this.removeRoute(route.id)
    }

    const coordinates = route.coordinates.map(
      (coord) => [coord.lng, coord.lat] as [number, number]
    )

    const add = () => {
      if (!this.map || this.routeSources.has(sourceId)) return

      this.map.addSource(sourceId, {
        type: 'geojson',
        data: {
          type: 'Feature',
          properties: {},
          geometry: {
            type: 'LineString',
            coordinates,
          },
        },
      })

      this.map.addLayer({
        id: layerId,
        type: 'line',
        source: sourceId,
        layout: {
          'line-join': 'round',
          'line-cap': 'round',
        },
        paint: {
          'line-color': route.color,
          'line-width': 4,
          'line-opacity': 0.8,
        },
      })

      this.routeSources.add(sourceId)
    }

    if (this.map.isStyleLoaded()) {
      add()
    } else {
      this.map.once('style.load', add)
    }
  }

  removeRoute(routeId: string): void {
    if (!this.map) return

    const sourceId = `route-${routeId}`
    const layerId = `route-layer-${routeId}`

    if (this.map.getLayer(layerId)) {
      this.map.removeLayer(layerId)
    }
    if (this.map.getSource(sourceId)) {
      this.map.removeSource(sourceId)
    }

    this.routeSources.delete(sourceId)
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
    this.markerClickHandler = handler
  }

  offMarkerClick(): void {
    this.markerClickHandler = null
  }

  setInteractionMode(mode: InteractionMode): void {
    if (!this.map) return

    const canvas = this.map.getCanvas()

    if (mode === 'add-stop') {
      canvas.style.cursor = 'crosshair'
    } else {
      canvas.style.cursor = ''
    }
  }

  onMapClick(handler: (location: LatLng) => void): void {
    this.mapClickHandler = handler

    if (this.map) {
      this.map.on('click', this.handleMapClick)
    }
  }

  offMapClick(): void {
    this.mapClickHandler = null

    if (this.map) {
      this.map.off('click', this.handleMapClick)
    }
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

  private handleMapClick = (e: { lngLat: { lat: number; lng: number } }): void => {
    this.mapClickHandler?.({
      lat: e.lngLat.lat,
      lng: e.lngLat.lng,
    })
  }
}
