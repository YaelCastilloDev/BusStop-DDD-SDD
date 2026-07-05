import { create } from 'zustand'
import { getCookie, setCookie } from '@/lib/cookies'
import type { MapEntityType, SelectedEntity } from '@/features/map/types'

const SIDEBAR_COLLAPSED_KEY = 'bs_sidebar_collapsed'

interface MapUIState {
  selectedEntity: SelectedEntity | null
  detailsPanelOpen: boolean
  sidebarCollapsed: boolean

  selectEntity: (type: MapEntityType, id: string) => void
  clearSelection: () => void
  togglePanel: () => void
  openPanel: () => void
  closePanel: () => void
  toggleSidebar: () => void
  setSidebarCollapsed: (collapsed: boolean) => void
}

function getInitialSidebarState(): boolean {
  try {
    return getCookie(SIDEBAR_COLLAPSED_KEY) === 'true'
  } catch {
    return false
  }
}

export const useMapUIStore = create<MapUIState>()((set) => ({
  selectedEntity: null,
  detailsPanelOpen: false,
  sidebarCollapsed: getInitialSidebarState(),

  selectEntity: (type, id) =>
    set({
      selectedEntity: { type, id },
      detailsPanelOpen: true,
    }),

  clearSelection: () =>
    set({
      selectedEntity: null,
      detailsPanelOpen: false,
    }),

  togglePanel: () =>
    set((state) => ({
      detailsPanelOpen: !state.detailsPanelOpen,
    })),

  openPanel: () => set({ detailsPanelOpen: true }),
  closePanel: () => set({ detailsPanelOpen: false }),

  toggleSidebar: () =>
    set((state) => {
      const next = !state.sidebarCollapsed
      setCookie(SIDEBAR_COLLAPSED_KEY, String(next))
      return { sidebarCollapsed: next }
    }),

  setSidebarCollapsed: (collapsed) => {
    setCookie(SIDEBAR_COLLAPSED_KEY, String(collapsed))
    set({ sidebarCollapsed: collapsed })
  },
}))
