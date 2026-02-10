import { useEffect, useState, useRef } from 'react'
import { useSlideshow } from '../../hooks/useSlideshow'
import { useKeyboardShortcuts } from '../../hooks/useKeyboardShortcuts'
import { useSlideshowStore } from '../../stores/slideshowStore'
import { SlideshowControls } from './SlideshowControls'
import { SlideshowProgress } from './SlideshowProgress'
import { SlideshowSettings } from './SlideshowSettings'
import { KenBurnsEffect } from './KenBurnsEffect'
import { AuthenticatedImage } from '../photos/AuthenticatedImage'
import type { Photo } from '../../types'

interface SlideshowProps {
  photos: Photo[]
  initialIndex?: number
  onClose: () => void
  autoStart?: boolean
}

export function Slideshow({ photos, initialIndex = 0, onClose, autoStart = true }: SlideshowProps) {
  const {
    isPlaying,
    currentIndex,
    currentPhoto,
    totalPhotos,
    config,
    nextSlide,
    prevSlide,
    togglePlayPause,
    setIsPlaying,
    setCurrentIndex
  } = useSlideshow({
    photos
  })

  const { isFullscreen, showControls, setIsFullscreen, setShowControls, setConfig } = useSlideshowStore()
  const [showSettings, setShowSettings] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)

  // Set initial index
  useEffect(() => {
    if (initialIndex >= 0 && initialIndex < photos.length) {
      setCurrentIndex(initialIndex)
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialIndex, photos.length])

  // Auto-start
  useEffect(() => {
    if (autoStart) {
      setIsPlaying(true)
    }
  }, [autoStart, setIsPlaying])

  // Fullscreen handling
  useEffect(() => {
    if (isFullscreen && containerRef.current) {
      containerRef.current.requestFullscreen().catch(console.error)
    } else if (!isFullscreen && document.fullscreenElement) {
      document.exitFullscreen().catch(console.error)
    }
  }, [isFullscreen])

  // Auto-hide controls
  useEffect(() => {
    if (!isPlaying || showSettings) {
      setShowControls(true)
      return
    }

    const timer = setTimeout(() => {
      setShowControls(false)
    }, 3000)

    return () => clearTimeout(timer)
  }, [isPlaying, showSettings, setShowControls])

  // Keyboard shortcuts
  useKeyboardShortcuts({
    enabled: true,
    onSpace: togglePlayPause,
    onArrowLeft: prevSlide,
    onArrowRight: nextSlide,
    onEscape: onClose,
    onKeyF: () => setIsFullscreen(!isFullscreen),
    onKeyC: () => setShowControls(!showControls),
    onKeyS: () => setShowSettings(!showSettings),
    onKeyL: () => setConfig({ loop: !config.loop }),
    onKeyK: () => setConfig({ kenBurns: !config.kenBurns })
  })

  if (!currentPhoto) return null

  const imageUrl = `/api/photos/${currentPhoto.id}/file`

  return (
    <div
      ref={containerRef}
      className="fixed inset-0 bg-black z-50 flex items-center justify-center"
      onClick={() => setShowControls(!showControls)}
    >
      {/* Main photo display */}
      <div className="relative w-full h-full flex items-center justify-center">
        {config.kenBurns ? (
          <KenBurnsEffect
            src={imageUrl}
            alt={currentPhoto.originalFileName}
            direction={config.kenBurnsDirection}
            className="max-w-full max-h-full object-contain"
          />
        ) : (
          <AuthenticatedImage
            src={imageUrl}
            alt={currentPhoto.originalFileName}
            className="max-w-full max-h-full object-contain transition-opacity duration-500"
          />
        )}

        {/* Photo info overlay */}
        {showControls && (
          <div className="absolute bottom-24 left-0 right-0 text-center">
            <p className="text-white text-lg font-medium drop-shadow-lg">
              {currentPhoto.originalFileName}
            </p>
            <p className="text-white/70 text-sm drop-shadow-lg">
              {currentIndex + 1} / {totalPhotos}
            </p>
          </div>
        )}
      </div>

      {/* Progress bar */}
      <SlideshowProgress currentIndex={currentIndex} totalPhotos={totalPhotos} isPlaying={isPlaying} timing={config.timing} />

      {/* Controls */}
      {showControls && (
        <>
          <SlideshowControls
            isPlaying={isPlaying}
            isFullscreen={isFullscreen}
            onPlayPause={togglePlayPause}
            onPrevious={prevSlide}
            onNext={nextSlide}
            onFullscreen={() => setIsFullscreen(!isFullscreen)}
            onSettings={() => setShowSettings(!showSettings)}
            onClose={onClose}
          />

          {showSettings && (
            <SlideshowSettings
              config={config}
              onConfigChange={setConfig}
              onClose={() => setShowSettings(false)}
            />
          )}
        </>
      )}

      {/* Pause on hover */}
      <div
        className="absolute inset-0"
        onMouseEnter={() => {
          if (isPlaying) setIsPlaying(false)
        }}
        onMouseLeave={() => {
          if (!showSettings && !document.fullscreenElement) setIsPlaying(true)
        }}
      />
    </div>
  )
}
