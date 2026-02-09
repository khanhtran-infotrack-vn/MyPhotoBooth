import { useMemo, useCallback, useEffect, useRef } from 'react'
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
  const { isSelectionMode, selectedIds, toggleSelection } = useSelectionStore()

  // Group photos by date
  const photoGroups = useMemo(() => groupPhotosByDate(photos), [photos])

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

  if (photos.length === 0 && !isLoading) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-gray-500">
        <svg className="w-20 h-20 mb-4 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
            d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
          />
        </svg>
        <h3 className="text-xl font-semibold text-gray-700">No photos yet</h3>
        <p className="mt-2">Upload your first photos to get started</p>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {photoGroups.map((group) => (
        <div key={group.date}>
          <DateGroupHeader
            date={group.date}
            photoCount={group.photos.length}
            photoIds={group.photos.map((p) => p.id)}
          />
          <div className="mt-2">
            <RowsPhotoAlbum
              photos={group.photos.map((photo, idx) => ({
                src: getThumbnailUrl(photo.id),
                width: photo.width || 300,
                height: photo.height || 300,
                key: photo.id,
                originalPhoto: photo,
                originalIndex: idx,
              }))}
              targetRowHeight={150}
              rowConstraints={{ minPhotos: 1, maxPhotos: 5, singleRowMaxHeight: 200 }}
              spacing={6}
              onClick={({ photo }) => {
                const orig = (photo as any).originalPhoto as Photo
                const idx = (photo as any).originalIndex as number
                if (isSelectionMode) {
                  toggleSelection(orig.id)
                } else if (orig) {
                  handlePhotoClick(orig, idx)
                }
              }}
              render={{
                image: (props, { photo }) => {
                  const originalPhoto = (photo as any).originalPhoto as Photo
                  return (
                    <AuthenticatedImage
                      src={photo.src}
                      alt={originalPhoto?.originalFileName || 'Photo'}
                      className="w-full h-full object-cover transition-transform duration-200 group-hover:scale-105"
                      style={props.style}
                      loading="lazy"
                    />
                  )
                },
                wrapper: ({ children, style }, { photo }) => {
                  const originalPhoto = (photo as any).originalPhoto as Photo
                  const isSelected = selectedIds.has(originalPhoto?.id || '')
                  return (
                    <div
                      className={`relative group cursor-pointer overflow-hidden rounded-lg ${
                        isSelected ? 'ring-4 ring-primary-500' : ''
                      }`}
                      style={style}
                    >
                      {children}
                      {/* Hover overlay */}
                      <div
                        className={`absolute inset-0 bg-black/0 group-hover:bg-black/10 transition-colors pointer-events-none ${
                          isSelected ? 'bg-primary-500/20' : ''
                        }`}
                      />
                      {/* Selection checkbox */}
                      <div
                        className={`absolute top-2 left-2 transition-opacity pointer-events-auto ${
                          isSelectionMode || isSelected ? 'opacity-100' : 'opacity-0 group-hover:opacity-100'
                        }`}
                      >
                        <button
                          onClick={(e) => {
                            e.stopPropagation()
                            if (originalPhoto) toggleSelection(originalPhoto.id)
                          }}
                          className={`w-6 h-6 rounded-full border-2 flex items-center justify-center transition-colors ${
                            isSelected
                              ? 'bg-primary-600 border-primary-600 text-white'
                              : 'bg-white/90 border-gray-400 hover:border-primary-600'
                          }`}
                        >
                          {isSelected && (
                            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                            </svg>
                          )}
                        </button>
                      </div>
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
        <div ref={loadMoreRef} className="flex justify-center py-8">
          {isLoading ? (
            <div className="flex items-center gap-2 text-gray-500">
              <div className="w-5 h-5 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
              <span>Loading more photos...</span>
            </div>
          ) : (
            <button onClick={onLoadMore} className="btn-secondary">
              Load more
            </button>
          )}
        </div>
      )}
    </div>
  )
}
