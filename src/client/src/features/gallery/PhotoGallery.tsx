import { useState, useMemo } from 'react'
import { PhotoGrid, SelectionBar } from '../../components/photos'
import { Lightbox } from '../../components/lightbox'
import { usePhotos, getAllPhotosFromPages } from '../../hooks/usePhotos'
import { ShareModal } from '../sharing/ShareModal'
import type { Photo } from '../../types'

export default function PhotoGallery() {
  const [lightboxOpen, setLightboxOpen] = useState(false)
  const [lightboxIndex, setLightboxIndex] = useState(0)
  const [sharePhoto, setSharePhoto] = useState<Photo | null>(null)

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
    <div className="p-6 max-w-[1800px] mx-auto">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 dark:from-white dark:to-gray-300 bg-clip-text text-transparent">Photos</h1>
        {data?.pages[0] && (
          <p className="text-sm text-gray-600 dark:text-dark-text-secondary mt-2 text-lg">
            {data.pages[0].totalCount} {data.pages[0].totalCount === 1 ? 'photo' : 'photos'}
          </p>
        )}
      </div>

      {/* Loading state */}
      {isLoading && (
        <div className="flex items-center justify-center py-32">
          <div className="flex flex-col items-center gap-4 text-gray-600 dark:text-dark-text-secondary">
            <div className="w-12 h-12 border-3 border-gray-300 border-t-primary-600 dark:border-t-primary-500 rounded-full animate-spin" />
            <span className="text-lg font-medium">Loading your photos...</span>
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
          onShare={(photo) => {
            setSharePhoto(photo)
          }}
        />
      )}

      {/* Share Modal */}
      {sharePhoto && (
        <ShareModal
          type="photo"
          targetId={sharePhoto.id}
          targetName={sharePhoto.originalFileName}
          onClose={() => setSharePhoto(null)}
        />
      )}
    </div>
  )
}
