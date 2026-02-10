import { useState, useEffect, useCallback } from 'react'
import type { Photo } from '../../types'
import { getPhotoUrl, usePhotoDetails } from '../../hooks/usePhotos'
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

  const currentPhoto = photos[currentIndex]
  const { data: photoDetails } = usePhotoDetails(currentPhoto?.id ?? null)

  const hasPrev = currentIndex > 0
  const hasNext = currentIndex < photos.length - 1

  // Load image
  useEffect(() => {
    if (!currentPhoto) return

    setLoading(true)
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

  return (
    <div className="fixed inset-0 z-50 bg-black dark:bg-black">
      {/* Main content area */}
      <div className="relative w-full h-full flex items-center justify-center">
        {/* Background click to close */}
        <div className="absolute inset-0" onClick={onClose} />

        {/* Image */}
        <div
          className="relative max-w-[90vw] max-h-[90vh] z-10"
          onClick={(e) => e.stopPropagation()}
        >
          {loading ? (
            <div className="flex items-center justify-center w-64 h-64">
              <div className="w-12 h-12 border-4 border-white/10 border-t-white rounded-full animate-spin shadow-lg shadow-white/20" />
            </div>
          ) : imageUrl ? (
            <img
              src={imageUrl}
              alt={currentPhoto.originalFileName}
              className="max-w-[90vw] max-h-[90vh] object-contain rounded-xl shadow-2xl dark:shadow-black/50"
              style={{ filter: 'drop-shadow(0 25px 50px rgba(0,0,0,0.5))' }}
            />
          ) : (
            <div className="flex items-center justify-center w-64 h-64 text-white/80">
              Failed to load image
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

      {/* Top bar with actions */}
      <div className="absolute top-0 left-0 right-0 z-20 flex items-center justify-between p-6 bg-gradient-to-b from-black/80 via-black/40 to-transparent">
        <div className="glass-strong rounded-full px-4 py-2">
          <span className="text-white text-sm font-medium">
            {currentIndex + 1} / {photos.length}
          </span>
        </div>

        <LightboxActions
          photo={currentPhoto}
          onClose={onClose}
          onToggleInfo={() => setShowInfo((s) => !s)}
          showInfo={showInfo}
          onShare={onShare ? () => onShare(currentPhoto) : undefined}
        />
      </div>

      {/* Bottom bar with filename and date */}
      <div className="absolute bottom-0 left-0 right-0 z-20 p-6 bg-gradient-to-t from-black/80 via-black/40 to-transparent">
        <div className="glass-strong rounded-2xl px-6 py-4 max-w-2xl mx-auto">
          <div className="text-center text-white">
            <p className="font-semibold text-lg">{currentPhoto.originalFileName}</p>
            <p className="text-sm text-white/70 mt-1">
              {new Date(currentPhoto.capturedAt || currentPhoto.uploadedAt).toLocaleDateString(
                undefined,
                { year: 'numeric', month: 'long', day: 'numeric' }
              )}
            </p>
          </div>
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
