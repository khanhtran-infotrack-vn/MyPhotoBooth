import { useEffect } from 'react'

interface KeyboardShortcutConfig {
  onSpace?: () => void
  onArrowLeft?: () => void
  onArrowRight?: () => void
  onEscape?: () => void
  onKeyF?: () => void
  onKeyC?: () => void // For controls
  onKeyS?: () => void // For settings
  onKeyL?: () => void // For loop toggle
  onKeyK?: () => void // For Ken Burns toggle
  enabled?: boolean
}

export function useKeyboardShortcuts(config: KeyboardShortcutConfig) {
  const {
    onSpace,
    onArrowLeft,
    onArrowRight,
    onEscape,
    onKeyF,
    onKeyC,
    onKeyS,
    onKeyL,
    onKeyK,
    enabled = true
  } = config

  useEffect(() => {
    if (!enabled) return

    const handleKeyDown = (e: KeyboardEvent) => {
      // Ignore if user is typing in an input
      if (
        e.target instanceof HTMLInputElement ||
        e.target instanceof HTMLTextAreaElement ||
        e.target instanceof HTMLSelectElement ||
        (e.target as HTMLElement).isContentEditable
      ) {
        return
      }

      switch (e.key) {
        case ' ':
          e.preventDefault()
          onSpace?.()
          break
        case 'ArrowLeft':
          e.preventDefault()
          onArrowLeft?.()
          break
        case 'ArrowRight':
          e.preventDefault()
          onArrowRight?.()
          break
        case 'Escape':
          e.preventDefault()
          onEscape?.()
          break
        case 'f':
        case 'F':
          e.preventDefault()
          onKeyF?.()
          break
        case 'c':
        case 'C':
          e.preventDefault()
          onKeyC?.()
          break
        case 's':
        case 'S':
          e.preventDefault()
          onKeyS?.()
          break
        case 'l':
        case 'L':
          e.preventDefault()
          onKeyL?.()
          break
        case 'k':
        case 'K':
          e.preventDefault()
          onKeyK?.()
          break
      }
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [enabled, onSpace, onArrowLeft, onArrowRight, onEscape, onKeyF, onKeyC, onKeyS, onKeyL, onKeyK])
}
