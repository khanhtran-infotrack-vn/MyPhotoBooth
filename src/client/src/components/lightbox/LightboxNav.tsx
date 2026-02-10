interface LightboxNavProps {
  hasPrev: boolean
  hasNext: boolean
  onPrev: () => void
  onNext: () => void
}

export function LightboxNav({ hasPrev, hasNext, onPrev, onNext }: LightboxNavProps) {
  return (
    <>
      {/* Previous button */}
      {hasPrev && (
        <button
          onClick={(e) => {
            e.stopPropagation()
            onPrev()
          }}
          className="absolute left-4 sm:left-8 top-1/2 -translate-y-1/2 z-20 w-14 h-14 sm:w-16 sm:h-16 flex items-center justify-center
                   bg-black/50 hover:bg-black/70 backdrop-blur-sm text-white rounded-2xl transition-all duration-200
                   hover:scale-110 shadow-xl border border-white/10 group"
          aria-label="Previous photo"
        >
          <svg className="w-6 h-6 sm:w-7 sm:h-7 transition-transform group-hover:-translate-x-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M15 19l-7-7 7-7" />
          </svg>
        </button>
      )}

      {/* Next button */}
      {hasNext && (
        <button
          onClick={(e) => {
            e.stopPropagation()
            onNext()
          }}
          className="absolute right-4 sm:right-8 top-1/2 -translate-y-1/2 z-20 w-14 h-14 sm:w-16 sm:h-16 flex items-center justify-center
                   bg-black/50 hover:bg-black/70 backdrop-blur-sm text-white rounded-2xl transition-all duration-200
                   hover:scale-110 shadow-xl border border-white/10 group"
          aria-label="Next photo"
        >
          <svg className="w-6 h-6 sm:w-7 sm:h-7 transition-transform group-hover:translate-x-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M9 5l7 7-7 7" />
          </svg>
        </button>
      )}

      {/* Keyboard hint */}
      <div className="absolute bottom-32 left-1/2 -translate-x-1/2 z-10 pointer-events-none opacity-0 hover:opacity-100 transition-opacity">
        <div className="px-3 py-1.5 rounded-lg bg-black/40 backdrop-blur-sm text-white/60 text-xs">
          Use arrow keys to navigate
        </div>
      </div>
    </>
  )
}
