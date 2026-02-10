import { useEffect } from 'react'

interface SlideshowProgressProps {
  currentIndex: number
  totalPhotos: number
  isPlaying: boolean
  timing: number
}

export function SlideshowProgress({ currentIndex, totalPhotos, isPlaying, timing }: SlideshowProgressProps) {
  useEffect(() => {
    if (!isPlaying) {
      return
    }

    const interval = setInterval(() => {
      // Progress is calculated dynamically for dots display
    }, 100)

    return () => clearInterval(interval)
  }, [isPlaying, currentIndex, timing])

  return (
    <div className="absolute top-0 left-0 right-0 flex items-center justify-center gap-1 p-4">
      {Array.from({ length: totalPhotos }).map((_, index) => (
        <div
          key={index}
          className={`h-1 rounded-full transition-all duration-300 ${
            index === currentIndex
              ? 'bg-primary-500 w-8'
              : index < currentIndex
              ? 'bg-white w-2'
              : 'bg-white/30 w-2'
          }`}
        />
      ))}
    </div>
  )
}
