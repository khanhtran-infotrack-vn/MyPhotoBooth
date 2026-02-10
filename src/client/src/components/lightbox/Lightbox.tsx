import { useState, useEffect, useCallback } from 'react'
import type { Photo } from '../../types'
import { getPhotoUrl, getThumbnailUrl, usePhotoDetails } from '../../hooks/usePhotos'
import { LightboxNav } from './LightboxNav'
import { LightboxInfo } from './LightboxInfo'
import { LightboxActions } from './LightboxActions'
import api from '../../lib/api'

interface LightboxProps {
  photos: Photo[]
  initialIndex: number
  onClose: () => void
  onShare?: (photo: Photo) => void
}

export function Lightbox({ photos, initialIndex, onClose, onShare }: LightboxProps) {
  const [currentIndex, setCurrentIndex] = useState(initialIndex)
  const [imageUrl, setImageUrl] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [showInfo, setShowInfo] = useState(false)
  const [imageScale, setImageScale] = useState(1)

  const currentPhoto = photos[currentIndex]
  const { data: photoDetails } = usePhotoDetails(currentPhoto?.id ?? null)

  const hasPrev = currentIndex > 0
  const hasNext = currentIndex < photos.length - 1

  // Load image
  useEffect(() => {
    if (!currentPhoto) return

    setLoading(true)
    setImageScale(0.95)

    const loadImage = async () => {
      try {
        const response = await api.get(`/photos/${currentPhoto.id}/file`, {
          responseType: 'blob',
        })
        const url = URL.createObjectURL(response.data)
        setImageUrl((prev) => {
          if (prev) URL.revokeObjectURL(prev)
          return url
        })
        // Animate image in
        setTimeout(() => setImageScale(1), 50)
      } catch (error) {
        console.error('Failed to load photo:', error)
      } finally {
        setLoading(false)
      }
    }

    loadImage()

    return () => {
      if (imageUrl) URL.revokeObjectURL(imageUrl)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentPhoto?.id])

  // Preload adjacent images
  useEffect(() => {
    const preloadIds: string[] = []
    if (hasPrev) preloadIds.push(photos[currentIndex - 1].id)
    if (hasNext) preloadIds.push(photos[currentIndex + 1].id)

    preloadIds.forEach((id) => {
      const img = new Image()
      img.src = getPhotoUrl(id)
    })
  }, [currentIndex, photos, hasPrev, hasNext])

  // Keyboard navigation
  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      switch (e.key) {
        case 'Escape':
          onClose()
          break
        case 'ArrowLeft':
          if (hasPrev) setCurrentIndex((i) => i - 1)
          break
        case 'ArrowRight':
          if (hasNext) setCurrentIndex((i) => i + 1)
          break
        case 'i':
          setShowInfo((s) => !s)
          break
      }
    },
    [onClose, hasPrev, hasNext]
  )

  useEffect(() => {
    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [handleKeyDown])

  // Prevent body scroll
  useEffect(() => {
    document.body.style.overflow = 'hidden'
    return () => {
      document.body.style.overflow = ''
    }
  }, [])

  if (!currentPhoto) return null

  const photoDate = new Date(currentPhoto.capturedAt || currentPhoto.uploadedAt)
  const dateStr = photoDate.toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })

  return (
    <div className="fixed inset-0 z-50 bg-black">
      {/* Ambient background effect */}
      <div className="absolute inset-0 bg-gradient-to-br from-primary-900/20 via-transparent to-purple-900/20 opacity-50" />

      {/* Main content area */}
      <div className="relative w-full h-full flex items-center justify-center">
        {/* Background click to close */}
        <div className="absolute inset-0 z-0" onClick={onClose} />

        {/* Image */}
        <div
          className="relative z-10 transition-transform duration-300 ease-out"
          style={{ transform: `scale(${imageScale})` }}
          onClick={(e) => e.stopPropagation()}
        >
          {loading ? (
            <div className="flex items-center justify-center w-[90vw] h-[60vh]">
              <div className="flex flex-col items-center gap-4">
                <div className="relative">
                  <div className="w-16 h-16 border-4 border-white/10 border-t-white rounded-full animate-spin" />
                  <div className="absolute inset-0 w-16 h-16 border-4 border-transparent border-r-white/30 rounded-full animate-spin" style={{ animationDirection: 'reverse' }} />
                </div>
                <span className="text-white/60 text-sm font-medium">Loading photo...</span>
              </div>
            </div>
          ) : imageUrl ? (
            <img
              src={imageUrl}
              alt={currentPhoto.originalFileName}
              className="max-w-[90vw] max-h-[85vh] object-contain rounded-2xl shadow-2xl"
              style={{
                filter: 'drop-shadow(0 30px 60px rgba(0,0,0,0.6))',
                maxHeight: 'calc(100vh - 180px)',
              }}
            />
          ) : (
            <div className="flex items-center justify-center w-64 h-64 text-white/60">
              <div className="text-center">
                <svg className="w-12 h-12 mx-auto mb-3 opacity-50" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
                Failed to load
              </div>
            </div>
          )}
        </div>

        {/* Navigation arrows */}
        <LightboxNav
          hasPrev={hasPrev}
          hasNext={hasNext}
          onPrev={() => setCurrentIndex((i) => i - 1)}
          onNext={() => setCurrentIndex((i) => i + 1)}
        />
      </div>

      {/* Top bar */}
      <div className="absolute top-0 left-0 right-0 z-20 p-4 sm:p-6">
        <div className="flex items-center justify-between">
          {/* Counter */}
          <div className="px-4 py-2 rounded-full bg-black/40 backdrop-blur-md border border-white/10 shadow-lg">
            <span className="text-white text-sm font-semibold tracking-wide">
              {currentIndex + 1} <span className="text-white/40">/</span> {photos.length}
            </span>
          </div>

          {/* Actions */}
          <LightboxActions
            photo={currentPhoto}
            onClose={onClose}
            onToggleInfo={() => setShowInfo((s) => !s)}
            showInfo={showInfo}
            onShare={onShare ? () => onShare(currentPhoto) : undefined}
          />
        </div>
      </div>

      {/* Bottom bar */}
      <div className="absolute bottom-0 left-0 right-0 z-20 p-4 sm:p-6">
        <div className="max-w-2xl mx-auto">
          <div className="px-6 py-4 rounded-2xl bg-black/60 backdrop-blur-xl border border-white/10 shadow-2xl">
            <div className="text-center">
              <p className="text-white font-semibold text-base sm:text-lg truncate mb-1">
                {currentPhoto.originalFileName}
              </p>
              <div className="flex items-center justify-center gap-2 text-white/60 text-sm">
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span>{dateStr}</span>
                {photoDetails && (
                  <>
                    <span className="text-white/30">•</span>
                    <span>{photoDetails.width} × {photoDetails.height}</span>
                  </>
                )}
              </div>
            </div>
          </div>

          {/* Thumbnail strip */}
          {photos.length > 1 && (
            <div className="flex items-center justify-center gap-1 mt-3 overflow-x-auto scrollbar-hide">
              {photos.map((photo, idx) => (
                <button
                  key={photo.id}
                  onClick={() => setCurrentIndex(idx)}
                  className={`flex-shrink-0 w-12 h-12 rounded-lg overflow-hidden border-2 transition-all ${
                    idx === currentIndex
                      ? 'border-primary-500 scale-110 shadow-lg shadow-primary-500/30'
                      : 'border-white/10 opacity-60 hover:opacity-100 hover:scale-105'
                  }`}
                >
                  <img
                    src={getThumbnailUrl(photo.id)}
                    alt=""
                    className="w-full h-full object-cover"
                    loading="lazy"
                  />
                </button>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Info panel */}
      {showInfo && (
        <LightboxInfo
          photo={currentPhoto}
          details={photoDetails}
          onClose={() => setShowInfo(false)}
        />
      )}
    </div>
  )
}
