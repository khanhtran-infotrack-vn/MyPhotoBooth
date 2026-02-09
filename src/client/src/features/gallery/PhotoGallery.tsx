import { useState, useMemo } from 'react'
import { PhotoGrid, SelectionBar } from '../../components/photos'
import { Lightbox } from '../../components/lightbox'
import { usePhotos, getAllPhotosFromPages } from '../../hooks/usePhotos'
import type { Photo } from '../../types'

export default function PhotoGallery() {
  const [lightboxOpen, setLightboxOpen] = useState(false)
  const [lightboxIndex, setLightboxIndex] = useState(0)

  const {
    data,
    isLoading,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
  } = usePhotos()

  // Flatten all pages into a single array
  const photos = useMemo(
    () => getAllPhotosFromPages(data?.pages),
    [data?.pages]
  )

  const handlePhotoClick = (photo: Photo, index: number) => {
    // Find the actual index in the flat photos array
    const flatIndex = photos.findIndex((p) => p.id === photo.id)
    setLightboxIndex(flatIndex >= 0 ? flatIndex : index)
    setLightboxOpen(true)
  }

  const handleLoadMore = () => {
    if (!isFetchingNextPage && hasNextPage) {
      fetchNextPage()
    }
  }

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">Photos</h1>
        {data?.pages[0] && (
          <p className="text-sm text-gray-500 mt-1">
            {data.pages[0].totalCount} {data.pages[0].totalCount === 1 ? 'photo' : 'photos'}
          </p>
        )}
      </div>

      {/* Loading state */}
      {isLoading && (
        <div className="flex items-center justify-center py-20">
          <div className="flex items-center gap-3 text-gray-500">
            <div className="w-6 h-6 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
            <span>Loading your photos...</span>
          </div>
        </div>
      )}

      {/* Photo grid */}
      {!isLoading && (
        <PhotoGrid
          photos={photos}
          onPhotoClick={handlePhotoClick}
          onLoadMore={handleLoadMore}
          hasMore={hasNextPage}
          isLoading={isFetchingNextPage}
        />
      )}

      {/* Selection bar */}
      <SelectionBar />

      {/* Lightbox */}
      {lightboxOpen && photos.length > 0 && (
        <Lightbox
          photos={photos}
          initialIndex={lightboxIndex}
          onClose={() => setLightboxOpen(false)}
        />
      )}
    </div>
  )
}
