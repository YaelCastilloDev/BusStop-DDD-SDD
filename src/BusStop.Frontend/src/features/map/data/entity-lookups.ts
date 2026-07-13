import type { Stop, Route } from '../types'
import { MOCK_STOPS, MOCK_ROUTES } from './mock-data'

const stopMap = new Map(MOCK_STOPS.map((s) => [s.id, s]))
const routeMap = new Map(MOCK_ROUTES.map((r) => [r.id, r]))

export function getStopById(id: string): Stop | undefined {
  return stopMap.get(id)
}

export function getRouteById(id: string): Route | undefined {
  return routeMap.get(id)
}

export function getAssociatedRoutes(routeIds: string[]): Route[] {
  return routeIds
    .map((id) => routeMap.get(id))
    .filter((r): r is Route => r !== undefined)
}

export function getOrderedStops(stopIds: string[]): Stop[] {
  return stopIds
    .map((id) => stopMap.get(id))
    .filter((s): s is Stop => s !== undefined)
}
