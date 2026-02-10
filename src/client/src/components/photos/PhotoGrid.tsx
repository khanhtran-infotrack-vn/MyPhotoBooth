import { useMemo, useCallback, useEffect, useRef, useState } from 'react'
import { RowsPhotoAlbum } from 'react-photo-album'
import 'react-photo-album/rows.css'
import type { Photo } from '../../types'
import { getThumbnailUrl, groupPhotosByDate } from '../../hooks/usePhotos'
import { useSelectionStore } from '../../stores/selectionStore'
import { DateGroupHeader } from './DateGroupHeader'
import { AuthenticatedImage } from './AuthenticatedImage'

interface PhotoGridProps {
  photos: Photo[]
  onPhotoClick: (photo: Photo, index: number) => void
  onLoadMore?: () => void
  hasMore?: boolean
  isLoading?: boolean
}

export function PhotoGrid({
  photos,
  onPhotoClick,
  onLoadMore,
  hasMore,
  isLoading,
}: PhotoGridProps) {
  const loadMoreRef = useRef<HTMLDivElement>(null)
  const { isSelectionMode, selectedIds, toggleSelection, selectMultiple, enterSelectionMode } = useSelectionStore()
  const [hoveredPhotoId, setHoveredPhotoId] = useState<string | null>(null)

  // Drag selection state
  const [isDragging, setIsDragging] = useState(false)
  const [dragStartPhotoId, setDragStartPhotoId] = useState<string | null>(null)
  const dragStartPosRef = useRef<{ x: number; y: number } | null>(null)

  // Long press state
  const [longPressPhotoId, setLongPressPhotoId] = useState<string | null>(null)
  const longPressTimerRef = useRef<number | null>(null)

  // Group photos by date
  const photoGroups = useMemo(() => groupPhotosByDate(photos), [photos])

  // Create a map of photo IDs to their indices for range selection
  const photoIdToIndexMap = useMemo(() => {
    const map = new Map<string, number>()
    photos.forEach((photo, index) => {
      map.set(photo.id, index)
    })
    return map
  }, [photos])

  // Intersection observer for infinite scroll
  useEffect(() => {
    if (!onLoadMore || !hasMore || isLoading) return

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting) {
          onLoadMore()
        }
      },
      { threshold: 0.1 }
    )

    if (loadMoreRef.current) {
      observer.observe(loadMoreRef.current)
    }

    return () => observer.disconnect()
  }, [onLoadMore, hasMore, isLoading])

  // Clear long press timer on unmount
  useEffect(() => {
    return () => {
      if (longPressTimerRef.current) {
        clearTimeout(longPressTimerRef.current)
      }
    }
  }, [])

  // Handle photo click
  const handlePhotoClick = useCallback(
    (photo: Photo, index: number) => {
      if (isSelectionMode) {
        toggleSelection(photo.id)
      } else {
        onPhotoClick(photo, index)
      }
    },
    [isSelectionMode, toggleSelection, onPhotoClick]
  )

  // Handle drag selection start
  const handleDragStart = useCallback((photoId: string, clientX: number, clientY: number) => {
    setIsDragging(true)
    setDragStartPhotoId(photoId)
    dragStartPosRef.current = { x: clientX, y: clientY }

    // Enter selection mode if not already
    if (!isSelectionMode) {
      enterSelectionMode()
      // Initially select just this photo
      toggleSelection(photoId)
    }
  }, [isSelectionMode, enterSelectionMode, toggleSelection])

  // Handle drag move
  const handleDragMove = useCallback((photoId: string) => {
    if (!isDragging || !dragStartPhotoId) return

    // Calculate range selection
    const startIndex = photoIdToIndexMap.get(dragStartPhotoId)
    const currentIndex = photoIdToIndexMap.get(photoId)

    if (startIndex !== undefined && currentIndex !== undefined) {
      const minIndex = Math.min(startIndex, currentIndex)
      const maxIndex = Math.max(startIndex, currentIndex)

      // Select all photos in range
      const photoIdsToSelect: string[] = []
      for (let i = minIndex; i <= maxIndex; i++) {
        if (photos[i]) {
          photoIdsToSelect.push(photos[i].id)
        }
      }
      selectMultiple(photoIdsToSelect)
    }
  }, [isDragging, dragStartPhotoId, photoIdToIndexMap, photos, selectMultiple])

  // Handle drag end
  const handleDragEnd = useCallback(() => {
    setIsDragging(false)
    setDragStartPhotoId(null)
    dragStartPosRef.current = null
  }, [])

  // Handle long press start
  const handleLongPressStart = useCallback((photoId: string) => {
    setLongPressPhotoId(photoId)
    longPressTimerRef.current = setTimeout(() => {
      // Long press triggered
      setLongPressPhotoId(null)
      if (!isSelectionMode) {
        enterSelectionMode()
        toggleSelection(photoId)
      } else {
        toggleSelection(photoId)
      }
    }, 500)
  }, [isSelectionMode, enterSelectionMode, toggleSelection])

  // Handle long press end
  const handleLongPressEnd = useCallback(() => {
    setLongPressPhotoId(null)
    if (longPressTimerRef.current) {
      clearTimeout(longPressTimerRef.current)
      longPressTimerRef.current = null
    }
  }, [])

  if (photos.length === 0 && !isLoading) {
    return null
  }

  return (
    <div className="space-y-8">
      {photoGroups.map((group, groupIndex) => (
        <div key={group.date} className="animate-fade-in-up" style={{ animationDelay: `${groupIndex * 0.05}s` }}>
          <DateGroupHeader
            date={group.date}
            photoCount={group.photos.length}
            photoIds={group.photos.map((p) => p.id)}
          />
          <div className="mt-3">
            <RowsPhotoAlbum
              photos={group.photos.map((photo, idx) => ({
                src: getThumbnailUrl(photo.id),
                width: photo.width || 300,
                height: photo.height || 300,
                key: photo.id,
                originalPhoto: photo,
                originalIndex: idx,
              }))}
              targetRowHeight={180}
              rowConstraints={{ minPhotos: 1, maxPhotos: 6, singleRowMaxHeight: 240 }}
              spacing={8}
              onClick={({ photo, index }) => {
                const orig = (photo as any).originalPhoto as Photo
                const idx = (photo as any).originalIndex as number ?? index
                if (orig) {
                  handlePhotoClick(orig, idx)
                }
              }}
              render={{
                photo: ({ onClick }, { photo, width, height }) => {
                  const originalPhoto = (photo as any).originalPhoto as Photo
                  const isSelected = selectedIds.has(originalPhoto?.id || '')
                  const isHovered = hoveredPhotoId === originalPhoto?.id
                  const isLongPressed = longPressPhotoId === originalPhoto?.id
                  const isFavorite = originalPhoto?.isFavorite || false

                  const handleClick = (e: React.MouseEvent) => {
                    if (isSelectionMode) {
                      toggleSelection(originalPhoto.id)
                    } else {
                      onClick?.(e)
                    }
                  }

                  return (
                    <div
                      className={`relative group cursor-pointer overflow-hidden rounded-xl shadow-sm hover:shadow-xl transition-all duration-300 ${
                        isSelected ? 'ring-4 ring-primary-500 shadow-lg shadow-primary-500/20' : ''
                      } ${isLongPressed ? 'scale-95' : ''}`}
                      style={{ width, height }}
                      onMouseEnter={() => {
                        if (originalPhoto?.id) {
                          setHoveredPhotoId(originalPhoto.id)
                        }
                      }}
                      onMouseLeave={() => {
                        setHoveredPhotoId(null)
                        handleLongPressEnd()
                        handleDragEnd()
                      }}
                      onClick={handleClick}
                      // Mouse drag handlers
                      onMouseDown={(e) => {
                        // Don't handle drag if clicking on checkbox
                        if ((e.target as HTMLElement).closest('button[aria-label*="Select"]')) {
                          return
                        }
                        if (e.button === 0) { // Left click only
                          handleDragStart(originalPhoto?.id || '', e.clientX, e.clientY)
                        }
                      }}
                      onMouseMove={() => {
                        if (isDragging) {
                          handleDragMove(originalPhoto?.id || '')
                        }
                      }}
                      onMouseUp={() => {
                        handleDragEnd()
                      }}
                      // Touch handlers for mobile
                      onTouchStart={(e) => {
                        // Don't handle long press if touching checkbox
                        if ((e.target as HTMLElement).closest('button[aria-label*="Select"]')) {
                          return
                        }
                        e.preventDefault()
                        handleLongPressStart(originalPhoto?.id || '')
                      }}
                      onTouchEnd={() => {
                        handleLongPressEnd()
                      }}
                      onTouchMove={() => {
                        // Cancel long press on move
                        handleLongPressEnd()
                      }}
                    >
                      <AuthenticatedImage
                        src={photo.src}
                        alt={originalPhoto?.originalFileName || 'Photo'}
                        loading="lazy"
                        className={`w-full h-full object-cover transition-all duration-300 ${
                          isHovered ? 'scale-105' : 'scale-100'
                        }`}
                        style={{ width, height, objectFit: 'cover' } as React.CSSProperties}
                      />

                      {/* Gradient overlay on hover */}
                      <div className={`absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent transition-opacity duration-300 ${
                        isHovered ? 'opacity-100' : 'opacity-0'
                      }`} />

                      {/* Long press ripple effect */}
                      {isLongPressed && (
                        <div className="absolute inset-0 bg-primary-500/30 animate-pulse" />
                      )}

                      {/* Selection tint */}
                      {isSelected && (
                        <div className="absolute inset-0 bg-primary-500/20 backdrop-blur-[1px]" />
                      )}

                      {/* Favorite indicator */}
                      {isFavorite && (
                        <div className="absolute top-3 right-3 z-10">
                          <div className="w-7 h-7 rounded-lg bg-white/90 backdrop-blur-md shadow-lg flex items-center justify-center">
                            <svg className="w-4 h-4 text-red-500" fill="currentColor" viewBox="0 0 24 24">
                              <path d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z" />
                            </svg>
                          </div>
                        </div>
                      )}

                      {/* Selection checkbox */}
                      <div
                        className={`absolute top-3 left-3 transition-all duration-200 z-10 ${
                          isSelectionMode || isSelected ? 'opacity-100 scale-100' : 'opacity-0 scale-90 group-hover:opacity-100 group-hover:scale-100'
                        }`}
                      >
                        <button
                          onClick={(e) => {
                            e.preventDefault()
                            e.stopPropagation()
                            if (originalPhoto) toggleSelection(originalPhoto.id)
                          }}
                          onMouseDown={(e) => {
                            e.preventDefault()
                            e.stopPropagation()
                          }}
                          className={`w-7 h-7 rounded-xl border-2 flex items-center justify-center transition-all duration-200 shadow-lg backdrop-blur-md ${
                            isSelected
                              ? 'bg-primary-600 border-primary-600 text-white'
                              : 'bg-white/80 border-gray-300 hover:border-primary-500'
                          }`}
                          aria-label={isSelected ? 'Deselect photo' : 'Select photo'}
                        >
                          {isSelected && (
                            <svg className="w-4 h-4 animate-scale-in" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                            </svg>
                          )}
                        </button>
                      </div>

                      {/* Photo info overlay on hover */}
                      <div className={`absolute bottom-0 left-0 right-0 p-3 text-white transition-all duration-300 ${
                        isHovered ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-2'
                      }`}>
                        <p className="text-sm font-medium truncate drop-shadow-lg">
                          {originalPhoto?.originalFileName}
                        </p>
                      </div>

                      {/* Shine effect on hover */}
                      <div className={`absolute inset-0 bg-gradient-to-tr from-white/0 via-white/20 to-white/0 transition-opacity duration-300 pointer-events-none ${
                        isHovered ? 'opacity-100' : 'opacity-0'
                      }`} style={{
                        background: 'linear-gradient(105deg, transparent 40%, rgba(255,255,255,0.2) 45%, rgba(255,255,255,0.2) 50%, transparent 55%)',
                        backgroundSize: '200% 100%',
                        backgroundPosition: isHovered ? '100% 0' : '-100% 0',
                      }} />
                    </div>
                  )
                },
              }}
            />
          </div>
        </div>
      ))}

      {/* Load more trigger */}
      {hasMore && (
        <div ref={loadMoreRef} className="flex justify-center py-12">
          {isLoading ? (
            <div className="flex flex-col items-center gap-4 px-6 py-4 rounded-2xl bg-gray-100 backdrop-blur-sm">
              <div className="relative">
                <div className="w-6 h-6 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
              </div>
              <span className="text-sm font-medium text-gray-600">Loading more photos...</span>
            </div>
          ) : (
            <button
              onClick={onLoadMore}
              className="group px-6 py-3 rounded-full bg-white border border-gray-200 shadow-md hover:shadow-xl transition-all duration-200 hover:-translate-y-0.5"
            >
              <span className="text-sm font-semibold text-gray-700 group-hover:text-primary-600 transition-colors">
                Load more photos
              </span>
            </button>
          )}
        </div>
      )}

      {/* End of photos indicator */}
      {!hasMore && photos.length > 0 && (
        <div className="flex justify-center py-8">
          <div className="flex items-center gap-3 px-6 py-3 rounded-full bg-gray-100">
            <div className="w-2 h-2 rounded-full bg-green-500" />
            <span className="text-sm text-gray-500">You've reached the end</span>
          </div>
        </div>
      )}
    </div>
  )
}
