import { useState } from 'react'
import {
  createRootRoute,
  createRoute,
  createRouter,
  Outlet,
  RouterProvider,
} from '@tanstack/react-router'
import '@/styles/index.css'
import { I18nextProvider } from 'react-i18next'
import { beforeEach, describe, expect, it } from 'vitest'
import { render } from 'vitest-browser-react'
import { useMapUIStore } from '@/stores/map-ui-store'
import i18n from '@/lib/i18n'
import { SidebarProvider, SidebarTrigger } from '@/components/ui/sidebar'
import { DashboardSidebar } from './dashboard-sidebar'

function DashboardShellFixture() {
  const [open, setOpen] = useState(true)

  return (
    <I18nextProvider i18n={i18n}>
      <SidebarProvider open={open} onOpenChange={setOpen}>
        <div className='relative h-svh w-full overflow-hidden'>
          <div className='relative z-60'>
            <SidebarTrigger aria-label='Toggle sidebar' />
          </div>
          <output data-testid='sidebar-state' className='sr-only'>
            {open ? 'expanded' : 'collapsed'}
          </output>
          <div
            data-testid='map-viewport'
            className='absolute inset-0 z-0'
            style={{ height: '600px', width: '960px' }}
          />
          <DashboardSidebar />
        </div>
      </SidebarProvider>
    </I18nextProvider>
  )
}

const rootRoute = createRootRoute({
  component: Outlet,
})

const fixtureRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/',
  component: DashboardShellFixture,
})

const router = createRouter({
  routeTree: rootRoute.addChildren([fixtureRoute]),
})

describe('DashboardSidebar', () => {
  beforeEach(async () => {
    await i18n.changeLanguage('en')
    useMapUIStore.setState({ interactionMode: 'browse' })
  })

  it('keeps the map viewport unchanged while collapsing to an icon rail', async () => {
    const screen = await render(<RouterProvider router={router} />)
    const mapViewport = document.querySelector<HTMLElement>(
      '[data-testid="map-viewport"]'
    )

    expect(mapViewport).not.toBeNull()
    const initialBounds = mapViewport?.getBoundingClientRect()

    await screen.getByRole('button', { name: 'Toggle sidebar' }).click()

    await expect
      .element(screen.getByRole('button', { name: 'Add a missing place' }))
      .toBeVisible()
    await expect
      .element(screen.getByTestId('sidebar-state'))
      .toHaveTextContent('collapsed')
    expect(mapViewport?.getBoundingClientRect()).toEqual(initialBounds)
  })
})
