import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { SlideshowConfig } from '../types/slideshow'
import { DEFAULT_SLIDESHOW_CONFIG } from '../types/slideshow'

interface SlideshowStore {
  config: SlideshowConfig
  setConfig: (config: Partial<SlideshowConfig>) => void
  resetConfig: () => void
  isPlaying: boolean
  currentIndex: number
  isFullscreen: boolean
  showControls: boolean
  setIsPlaying: (playing: boolean) => void
  setCurrentIndex: (index: number) => void
  setIsFullscreen: (fullscreen: boolean) => void
  setShowControls: (show: boolean) => void
  nextSlide: () => void
  prevSlide: () => void
}

export const useSlideshowStore = create<SlideshowStore>()(
  persist(
    (set) => ({
      config: DEFAULT_SLIDESHOW_CONFIG,
      isPlaying: false,
      currentIndex: 0,
      isFullscreen: false,
      showControls: true,

      setConfig: (newConfig) =>
        set((state) => ({
          config: { ...state.config, ...newConfig }
        })),

      resetConfig: () =>
        set({
          config: DEFAULT_SLIDESHOW_CONFIG
        }),

      setIsPlaying: (playing) => set({ isPlaying: playing }),

      setCurrentIndex: (index) => set({ currentIndex: index }),

      setIsFullscreen: (fullscreen) => set({ isFullscreen: fullscreen }),

      setShowControls: (show) => set({ showControls: show }),

      nextSlide: () =>
        set((state) => ({
          currentIndex: state.currentIndex + 1
        })),

      prevSlide: () =>
        set((state) => ({
          currentIndex: Math.max(0, state.currentIndex - 1)
        }))
    }),
    {
      name: 'slideshow-config-storage',
      partialize: (state) => ({
        config: state.config
      })
    }
  )
)
