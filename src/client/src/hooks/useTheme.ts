import { useEffect } from 'react'
import { useUIStore } from '../stores/uiStore'

// Prevent FOUC by setting theme immediately on module load
const initializeTheme = () => {
  const storedTheme = localStorage.getItem('theme-mode')
  const themeMode = storedTheme === 'light' || storedTheme === 'dark' || storedTheme === 'system'
    ? storedTheme
    : 'system'

  const getEffectiveTheme = (): 'light' | 'dark' => {
    if (themeMode === 'system') {
      return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
    }
    return themeMode
  }

  const root = document.documentElement
  const effectiveTheme = getEffectiveTheme()

  // Set theme before first paint to prevent FOUC
  root.setAttribute('data-theme', effectiveTheme)
  root.classList.remove('light', 'dark')
  root.classList.add(effectiveTheme)
}

// Initialize immediately on module load
initializeTheme()

export function useTheme() {
  const { themeMode, getEffectiveTheme } = useUIStore()

  useEffect(() => {
    const root = document.documentElement
    const effectiveTheme = getEffectiveTheme()

    // Prevent FOUC - set before first paint
    root.classList.remove('light', 'dark')
    root.classList.add(effectiveTheme)

    // Also set data-theme for CSS selectors that need it
    root.setAttribute('data-theme', effectiveTheme)

    // Remove opposite class (both shouldn't be present)
    const opposite = effectiveTheme === 'dark' ? 'light' : 'dark'
    root.classList.remove(opposite)

    // Listen for system preference changes
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
    const handleChange = () => {
      if (themeMode === 'system') {
        const newTheme = mediaQuery.matches ? 'dark' : 'light'
        root.setAttribute('data-theme', newTheme)
        root.classList.remove('light', 'dark')
        root.classList.add(newTheme)
      }
    }

    mediaQuery.addEventListener('change', handleChange)
    return () => mediaQuery.removeEventListener('change', handleChange)
  }, [themeMode, getEffectiveTheme])

  return { themeMode, effectiveTheme: getEffectiveTheme() }
}
