import type { Map, Marker, GeoJSONSource, LngLatBoundsLike } from 'maplibre-gl'
import maplibregl from 'maplibre-gl'
import type { IMapAdapter, MapOptions } from './types'
import type { LatLng, MapEntityType, Route, Stop } from '@/features/map/types'

export class MapLibreAdapter implements IMapAdapter {
  private map: Map | null = null
  private markers: Map<string, Marker> = new Map()
  private routeSources: Set<string> = new Set()
  private markerClickHandler: ((type: MapEntityType, id: string) => void) | null = null
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

    this.initialized = true
  }

  destroy(): void {
    this.markers.forEach((marker) => marker.remove())
    this.markers.clear()
    this.routeSources.clear()
    this.markerClickHandler = null

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
    el.style.cssText = `
      width: 28px;
      height: 28px;
      background-color: hsl(var(--primary));
      border: 3px solid hsl(var(--background));
      border-radius: 50%;
      cursor: pointer;
      box-shadow: 0 2px 6px rgba(0,0,0,0.3);
      transition: transform 0.2s ease;
    `

    el.addEventListener('mouseenter', () => {
      el.style.transform = 'scale(1.2)'
    })
    el.addEventListener('mouseleave', () => {
      el.style.transform = 'scale(1)'
    })

    const marker = new maplibregl.Marker({ element: el })
      .setLngLat([stop.location.lng, stop.location.lat])
      .addTo(this.map)

    if (onClick) {
      el.addEventListener('click', () => onClick(stop.id))
    }

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

    this.map.on('load', () => {
      if (!this.map) return

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
    })

    if (this.map.isStyleLoaded()) {
      this.map.fire('load')
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
    ) as LngLatBoundsLike

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
}
