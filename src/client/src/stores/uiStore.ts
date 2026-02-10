import { create } from 'zustand'
import { persist } from 'zustand/middleware'

type ThemeMode = 'light' | 'dark' | 'system'

interface UIState {
  sidebarCollapsed: boolean
  sidebarOpen: boolean // For mobile drawer
  themeMode: ThemeMode
  toggleSidebarCollapsed: () => void
  setSidebarOpen: (open: boolean) => void
  setThemeMode: (mode: ThemeMode) => void
  getEffectiveTheme: () => 'light' | 'dark'
}

export const useUIStore = create<UIState>()(
  persist(
    (set, get) => ({
      sidebarCollapsed: false,
      sidebarOpen: false,
      themeMode: 'system',
      toggleSidebarCollapsed: () =>
        set((state) => ({ sidebarCollapsed: !state.sidebarCollapsed })),
      setSidebarOpen: (open) => set({ sidebarOpen: open }),
      setThemeMode: (mode) => set({ themeMode: mode }),
      getEffectiveTheme: () => {
        const mode = get().themeMode
        if (mode === 'system') {
          return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
        }
        return mode
      }
    }),
    {
      name: 'ui-storage',
      partialize: (state) => ({
        sidebarCollapsed: state.sidebarCollapsed,
        themeMode: state.themeMode
      }),
    }
  )
)
