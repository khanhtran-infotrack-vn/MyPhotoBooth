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
          className="absolute left-4 top-1/2 -translate-y-1/2 z-20 w-12 h-12 flex items-center justify-center
                   bg-black/40 hover:bg-black/60 text-white rounded-full transition-colors"
          aria-label="Previous photo"
        >
          <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
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
          className="absolute right-4 top-1/2 -translate-y-1/2 z-20 w-12 h-12 flex items-center justify-center
                   bg-black/40 hover:bg-black/60 text-white rounded-full transition-colors"
          aria-label="Next photo"
        >
          <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
          </svg>
        </button>
      )}
    </>
  )
}
