import { useState } from 'react'
import { useUIStore } from '../../stores/uiStore'

export function ThemeToggle() {
  const { themeMode, setThemeMode } = useUIStore()
  const [isOpen, setIsOpen] = useState(false)

  const themes = [
    { value: 'light' as const, icon: '☀️', label: 'Light' },
    { value: 'dark' as const, icon: '🌙', label: 'Dark' },
    { value: 'system' as const, icon: '💻', label: 'System' },
  ]

  const currentTheme = themes.find(t => t.value === themeMode)

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="btn-icon hover:bg-gray-100 dark:hover:bg-dark-bg-tertiary"
        title={`Theme: ${currentTheme?.label}`}
        aria-label={`Current theme: ${currentTheme?.label}. Click to change.`}
      >
        <span className="text-xl transition-transform duration-300 hover:scale-110 inline-block" role="img" aria-label={currentTheme?.label}>
          {currentTheme?.icon}
        </span>
      </button>

      {isOpen && (
        <>
          <div
            className="fixed inset-0 z-10"
            onClick={() => setIsOpen(false)}
          />
          <div className="absolute right-0 mt-2 w-44 glass-strong rounded-2xl shadow-2xl dark:shadow-primary-600/20 border border-gray-200/50 dark:border-white/10 py-2 z-20 animate-scale-in overflow-hidden">
            <div className="px-3 py-2 border-b border-gray-100 dark:border-white/5">
              <p className="text-xs font-semibold text-gray-500 dark:text-dark-text-tertiary uppercase tracking-wider">Theme</p>
            </div>
            {themes.map((theme) => (
              <button
                key={theme.value}
                onClick={() => {
                  setThemeMode(theme.value)
                  setIsOpen(false)
                }}
                className={`w-full px-4 py-3 text-left text-sm flex items-center gap-3 transition-all duration-200 relative
                  ${themeMode === theme.value
                    ? 'bg-gradient-to-r from-primary-50 to-purple-50 dark:from-primary-600/20 dark:to-purple-600/10 text-primary-700 dark:text-primary-300 font-semibold'
                    : 'text-gray-700 dark:text-dark-text-primary hover:bg-gray-50 dark:hover:bg-dark-bg-tertiary'
                  }`}
              >
                <span className="text-lg transition-transform duration-200" role="img" aria-label={theme.label}>
                  {theme.icon}
                </span>
                <span>{theme.label}</span>
                {themeMode === theme.value && (
                  <svg className="w-4 h-4 ml-auto text-primary-600 dark:text-primary-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                  </svg>
                )}
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  )
}
