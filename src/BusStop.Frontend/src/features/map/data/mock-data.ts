import type { Stop, Route } from '../types'

export const MOCK_STOPS: Stop[] = [
  {
    id: 'stop-1',
    name: 'Central Station',
    description: 'Main transit hub in downtown area.',
    location: { lat: 40.7128, lng: -74.006 },
    routeIds: ['route-1', 'route-2'],
  },
  {
    id: 'stop-2',
    name: 'Park Avenue',
    description: 'Busy stop near the park and shopping district.',
    location: { lat: 40.715, lng: -74.008 },
    routeIds: ['route-1'],
  },
  {
    id: 'stop-3',
    name: 'Riverside Blvd',
    description: 'Scenic stop along the river with access to ferry terminal.',
    location: { lat: 40.709, lng: -74.01 },
    routeIds: ['route-2'],
  },
  {
    id: 'stop-4',
    name: 'University Campus',
    description: 'Stop serving the main university campus.',
    location: { lat: 40.717, lng: -74.003 },
    routeIds: ['route-1', 'route-2'],
  },
]

export const MOCK_ROUTES: Route[] = [
  {
    id: 'route-1',
    name: 'Line A - Downtown Express',
    description: 'Express service connecting Central Station to University Campus.',
    stopIds: ['stop-1', 'stop-2', 'stop-4'],
    color: '#3b82f6',
    coordinates: [
      { lat: 40.7128, lng: -74.006 },
      { lat: 40.715, lng: -74.008 },
      { lat: 40.717, lng: -74.003 },
    ],
  },
  {
    id: 'route-2',
    name: 'Line B - Riverside Local',
    description: 'Local service along the riverside connecting Central Station to Riverside Blvd.',
    stopIds: ['stop-1', 'stop-3', 'stop-4'],
    color: '#ef4444',
    coordinates: [
      { lat: 40.7128, lng: -74.006 },
      { lat: 40.709, lng: -74.01 },
      { lat: 40.717, lng: -74.003 },
    ],
  },
]
