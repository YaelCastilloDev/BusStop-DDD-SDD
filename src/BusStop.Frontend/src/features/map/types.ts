export interface LatLng {
  lat: number
  lng: number
}

export interface Stop {
  id: string
  name: string
  description: string
  location: LatLng
  routeIds: string[]
}

export interface Route {
  id: string
  name: string
  description: string
  stopIds: string[]
  color: string
  coordinates: LatLng[]
}

export type MapEntityType = 'stop' | 'route'

export interface SelectedEntity {
  type: MapEntityType
  id: string
}
