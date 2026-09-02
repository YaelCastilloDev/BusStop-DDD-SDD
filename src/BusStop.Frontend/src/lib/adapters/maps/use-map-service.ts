import { useCallback, useEffect, useRef } from 'react'
import { MapLibreAdapter } from './maplibre-adapter'
import type { IMapAdapter, MapOptions } from './types'

let adapterInstance: IMapAdapter | null = null

function getAdapter(): IMapAdapter {
  if (!adapterInstance) {
    adapterInstance = new MapLibreAdapter()
  }
  return adapterInstance
}

export function useMapService(options: MapOptions) {
  const adapter = getAdapter()
  const containerRef = useRef<HTMLDivElement | null>(null)
  const initializedRef = useRef(false)

  const setContainerRef = useCallback((node: HTMLDivElement | null) => {
    containerRef.current = node
  }, [])

  useEffect(() => {
    const container = containerRef.current
    if (container && !initializedRef.current) {
      adapter.initialize(container, options)
      initializedRef.current = true
    }

    return () => {
      if (initializedRef.current) {
        adapter.destroy()
        adapterInstance = null
        initializedRef.current = false
      }
    }
  }, [adapter, options])

  return {
    adapter,
    containerRef: setContainerRef,
  }
}
