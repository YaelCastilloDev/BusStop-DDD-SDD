import { useMapUIStore } from '@/stores/map-ui-store'
import { getStopById, getRouteById } from '../data/entity-lookups'
import type { Stop, Route } from '../types'

export function useSelectedEntity(): Stop | Route | null {
  const selectedEntity = useMapUIStore((s) => s.selectedEntity)
  if (!selectedEntity) return null
  return selectedEntity.type === 'stop'
    ? getStopById(selectedEntity.id) ?? null
    : getRouteById(selectedEntity.id) ?? null
}
