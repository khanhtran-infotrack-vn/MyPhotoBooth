export interface SlideshowConfig {
  timing: number // in seconds: 3, 5, 10, 15
  shuffle: boolean
  loop: boolean
  kenBurns: boolean
  kenBurnsDirection: 'zoom-in' | 'zoom-out' | 'pan-left' | 'pan-right' | 'random'
}

export interface SlideshowState {
  isPlaying: boolean
  currentIndex: number
  isFullscreen: boolean
  showControls: boolean
}

export const DEFAULT_SLIDESHOW_CONFIG: SlideshowConfig = {
  timing: 5,
  shuffle: false,
  loop: true,
  kenBurns: false,
  kenBurnsDirection: 'random'
}
