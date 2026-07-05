import { useEffect, useRef, useCallback } from 'react'
import type { IMapAdapter, MapOptions } from './types'
import { MapLibreAdapter } from './maplibre-adapter'

let adapterInstance: IMapAdapter | null = null

function getAdapter(): IMapAdapter {
  if (!adapterInstance) {
    adapterInstance = new MapLibreAdapter()
  }
  return adapterInstance
}

export function useMapService(options: MapOptions) {
  const adapterRef = useRef<IMapAdapter>(getAdapter())
  const containerRef = useRef<HTMLDivElement | null>(null)
  const initializedRef = useRef(false)

  const setContainerRef = useCallback((node: HTMLDivElement | null) => {
    containerRef.current = node
  }, [])

  useEffect(() => {
    const container = containerRef.current
    const adapter = adapterRef.current

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
  }, [options])

  return {
    adapter: adapterRef.current,
    containerRef: setContainerRef,
  }
}
