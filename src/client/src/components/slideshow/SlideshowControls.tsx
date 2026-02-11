interface SlideshowControlsProps {
  isPlaying: boolean
  isFullscreen: boolean
  onPlayPause: () => void
  onPrevious: () => void
  onNext: () => void
  onFullscreen: () => void
  onSettings: () => void
  onClose: () => void
  onInteraction?: () => void
  onMouseEnter?: () => void
  onMouseLeave?: () => void
}

export function SlideshowControls({
  isPlaying,
  isFullscreen,
  onPlayPause,
  onPrevious,
  onNext,
  onFullscreen,
  onSettings,
  onClose,
  onInteraction,
  onMouseEnter,
  onMouseLeave
}: SlideshowControlsProps) {
  const handleInteraction = (handler: () => void) => (e: React.MouseEvent) => {
    e.stopPropagation()
    onInteraction?.()
    handler()
  }

  return (
    <div
      className="absolute bottom-8 left-1/2 transform -translate-x-1/2 flex items-center gap-3 bg-black/50 backdrop-blur-md rounded-full px-6 py-3"
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
    >
      <button
        onClick={handleInteraction(onPrevious)}
        className="p-2 text-white hover:text-primary-400 transition-colors"
        title="Previous (Left Arrow)"
      >
        <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
        </svg>
      </button>

      <button
        onClick={handleInteraction(onPlayPause)}
        className="p-3 bg-white text-black rounded-full hover:bg-gray-200 transition-colors"
        title={isPlaying ? 'Pause (Space)' : 'Play (Space)'}
      >
        {isPlaying ? (
          <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
            <path d="M6 4h4v16H6V4zm8 0h4v16h-4V4z" />
          </svg>
        ) : (
          <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
            <path d="M8 5v14l11-7z" />
          </svg>
        )}
      </button>

      <button
        onClick={handleInteraction(onNext)}
        className="p-2 text-white hover:text-primary-400 transition-colors"
        title="Next (Right Arrow)"
      >
        <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
        </svg>
      </button>

      <div className="w-px h-8 bg-white/30 mx-2" />

      <button
        onClick={handleInteraction(onFullscreen)}
        className="p-2 text-white hover:text-primary-400 transition-colors"
        title={`Fullscreen (${isFullscreen ? 'Exit' : 'Enter'}) (F)`}
      >
        {isFullscreen ? (
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        ) : (
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" />
          </svg>
        )}
      </button>

      <button
        onClick={handleInteraction(onSettings)}
        className="p-2 text-white hover:text-primary-400 transition-colors"
        title="Settings (S)"
      >
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
        </svg>
      </button>

      <button
        onClick={handleInteraction(onClose)}
        className="p-2 text-white hover:text-red-400 transition-colors ml-2"
        title="Close (Escape)"
      >
        <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>
  )
}
