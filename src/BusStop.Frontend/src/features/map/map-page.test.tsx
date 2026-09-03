import {
  createRootRoute,
  createRoute,
  createRouter,
  Outlet,
  RouterProvider,
} from '@tanstack/react-router'
import '@/styles/index.css'
import { I18nextProvider } from 'react-i18next'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { render } from 'vitest-browser-react'
import { useMapUIStore } from '@/stores/map-ui-store'
import i18n from '@/lib/i18n'
import { MapPage } from './map-page'

const mapService = vi.hoisted(() => {
  const adapter = {
    initialize: vi.fn(),
    destroy: vi.fn(),
    addStopMarker: vi.fn(),
    removeStopMarker: vi.fn(),
    drawRoute: vi.fn(),
    removeRoute: vi.fn(),
    centerOnLocation: vi.fn(),
    fitBounds: vi.fn(),
    onMarkerClick: vi.fn(),
    offMarkerClick: vi.fn(),
    setInteractionMode: vi.fn(),
    onMapClick: vi.fn(),
    offMapClick: vi.fn(),
    addGeolocateControl: vi.fn(),
    addFullscreenControl: vi.fn(),
    addScaleControl: vi.fn(),
  }
  const containerRef = vi.fn()

  return {
    adapter,
    containerRef,
    useMapService: vi.fn(() => ({ adapter, containerRef })),
  }
})

vi.mock('@/features/auth', () => ({
  OnboardingGate: () => null,
}))

vi.mock('@/features/map/components/dashboard-header', async () => {
  const { SidebarTrigger } = await import('@/components/ui/sidebar')

  return {
    DashboardHeader: () => (
      <header>
        <SidebarTrigger aria-label='Toggle sidebar' />
      </header>
    ),
  }
})

vi.mock('@/lib/adapters/maps', () => ({
  useMapService: mapService.useMapService,
}))

function MapPageFixture() {
  return (
    <I18nextProvider i18n={i18n}>
      <MapPage />
    </I18nextProvider>
  )
}

const rootRoute = createRootRoute({
  component: Outlet,
})

const fixtureRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/',
  component: MapPageFixture,
})

const router = createRouter({
  routeTree: rootRoute.addChildren([fixtureRoute]),
})

const originalMatchMedia = window.matchMedia

function mockMatchMedia(isMobile: boolean): void {
  window.matchMedia = vi.fn(
    (_query: string): MediaQueryList => ({
      matches: isMobile,
      media: '',
      onchange: null,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      addListener: vi.fn(),
      removeListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })
  )
}

function getDesktopSidebarState(): string | undefined {
  return document.querySelector<HTMLElement>(
    '[data-slot="sidebar"][data-state]'
  )?.dataset.state
}

describe('MapPage', () => {
  afterEach(() => {
    window.matchMedia = originalMatchMedia
  })

  beforeEach(async () => {
    await i18n.changeLanguage('en')
    mockMatchMedia(false)
    useMapUIStore.setState({
      selectedEntity: null,
      detailsPanelOpen: false,
      sidebarCollapsed: false,
      interactionMode: 'browse',
    })
    mapService.useMapService.mockClear()
    mapService.adapter.setInteractionMode.mockClear()
    mapService.adapter.onMapClick.mockClear()
    mapService.adapter.offMapClick.mockClear()
  })

  it('keeps the map component stable while the desktop sidebar collapses and expands', async () => {
    const screen = await render(<RouterProvider router={router} />)

    await expect.poll(() => mapService.useMapService.mock.calls.length).toBe(1)
    expect(getDesktopSidebarState()).toBe('expanded')

    await screen.getByRole('button', { name: 'Toggle sidebar' }).click()

    await expect.poll(getDesktopSidebarState).toBe('collapsed')
    expect(mapService.useMapService).toHaveBeenCalledTimes(1)

    await screen.getByRole('button', { name: 'Toggle sidebar' }).click()

    await expect.poll(getDesktopSidebarState).toBe('expanded')
    expect(mapService.useMapService).toHaveBeenCalledTimes(1)

    useMapUIStore.getState().setInteractionMode('add-stop')

    await expect.poll(() => mapService.useMapService.mock.calls.length).toBe(2)
    expect(mapService.adapter.setInteractionMode).toHaveBeenLastCalledWith(
      'add-stop'
    )
    expect(mapService.adapter.onMapClick).toHaveBeenCalledTimes(1)
  })

  it('keeps the map component stable while the mobile sidebar opens and closes', async () => {
    mockMatchMedia(true)
    const screen = await render(<RouterProvider router={router} />)

    await expect.poll(() => mapService.useMapService.mock.calls.length).toBe(1)

    await screen.getByRole('button', { name: 'Toggle sidebar' }).click()

    await expect
      .element(screen.getByRole('dialog', { name: 'Sidebar' }))
      .toBeVisible()
    expect(mapService.useMapService).toHaveBeenCalledTimes(1)

    document.dispatchEvent(
      new KeyboardEvent('keydown', { bubbles: true, key: 'Escape' })
    )

    await expect
      .poll(
        () =>
          document.querySelector<HTMLElement>('[data-slot="sheet-content"]')
            ?.dataset.state ?? 'closed'
      )
      .toBe('closed')
    expect(mapService.useMapService).toHaveBeenCalledTimes(1)
  })
})
