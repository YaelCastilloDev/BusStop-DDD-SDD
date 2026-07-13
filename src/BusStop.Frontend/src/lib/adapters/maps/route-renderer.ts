import { type Map as MaplibreMap } from 'maplibre-gl'
import type { Route } from '@/features/map/types'

export class RouteRenderer {
  private routeSources: Set<string> = new Set()

  draw(map: MaplibreMap, route: Route): void {
    const sourceId = `route-${route.id}`
    const layerId = `route-layer-${route.id}`

    if (this.routeSources.has(sourceId)) {
      this.remove(map, route.id)
    }

    const coordinates = route.coordinates.map(
      (coord) => [coord.lng, coord.lat] as [number, number]
    )

    const add = () => {
      if (this.routeSources.has(sourceId)) return

      map.addSource(sourceId, {
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

      map.addLayer({
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

    if (map.isStyleLoaded()) {
      add()
    } else {
      map.once('style.load', add)
    }
  }

  remove(map: MaplibreMap, routeId: string): void {
    const sourceId = `route-${routeId}`
    const layerId = `route-layer-${routeId}`

    if (map.getLayer(layerId)) {
      map.removeLayer(layerId)
    }
    if (map.getSource(sourceId)) {
      map.removeSource(sourceId)
    }

    this.routeSources.delete(sourceId)
  }

  destroy(): void {
    this.routeSources.clear()
  }
}
