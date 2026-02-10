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
  const [searchQuery, setSearchQuery] = useState('')

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

  // Filter photos by search query
  const filteredPhotos = useMemo(() => {
    if (!searchQuery.trim()) return photos
    const query = searchQuery.toLowerCase()
    return photos.filter(photo =>
      photo.originalFileName.toLowerCase().includes(query)
    )
  }, [photos, searchQuery])

  const handlePhotoClick = (photo: Photo, index: number) => {
    // Find the actual index in the filtered photos array
    const flatIndex = filteredPhotos.findIndex((p) => p.id === photo.id)
    setLightboxIndex(flatIndex >= 0 ? flatIndex : index)
    setLightboxOpen(true)
  }

  const handleLoadMore = () => {
    if (!isFetchingNextPage && hasNextPage) {
      fetchNextPage()
    }
  }

  const totalCount = data?.pages[0]?.totalCount ?? 0

  return (
    <div className="min-h-screen">
      {/* Hero Header with Gradient Background */}
      <div className="relative overflow-hidden bg-gradient-to-br from-primary-600 via-primary-700 to-purple-700 dark:from-gray-900 dark:via-slate-800 dark:to-gray-900">
        {/* Animated background elements */}
        <div className="absolute inset-0 overflow-hidden pointer-events-none">
          <div className="absolute -top-1/2 -right-1/2 w-full h-full bg-gradient-to-br from-white/10 to-transparent rounded-full blur-3xl animate-pulse" />
          <div className="absolute -bottom-1/2 -left-1/2 w-full h-full bg-gradient-to-tr from-primary-400/20 to-transparent rounded-full blur-3xl animate-pulse" style={{ animationDelay: '1s' }} />
        </div>

        {/* Header Content */}
        <div className="relative max-w-[1800px] mx-auto px-6 py-12 sm:py-16">
          <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-6">
            {/* Title Section */}
            <div className="animate-fade-in-up">
              <h1 className="text-4xl sm:text-5xl font-bold text-white mb-2">
                Photos
              </h1>
              {totalCount > 0 && (
                <p className="text-white/80 text-lg">
                  {totalCount.toLocaleString()} {totalCount === 1 ? 'photo' : 'photos'}
                </p>
              )}
            </div>

            {/* Search Bar */}
            <div className="w-full sm:w-80 animate-fade-in-up" style={{ animationDelay: '0.1s' }}>
              <div className="relative group">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                  <svg className="w-5 h-5 text-gray-400 group-focus-within:text-primary-500 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search photos..."
                  className="w-full pl-12 pr-4 py-3 bg-white/10 backdrop-blur-md border border-white/20 rounded-xl text-white placeholder-white/60 focus:outline-none focus:ring-2 focus:ring-white/30 focus:bg-white/20 transition-all"
                />
                {searchQuery && (
                  <button
                    onClick={() => setSearchQuery('')}
                    className="absolute inset-y-0 right-0 pr-4 flex items-center text-white/60 hover:text-white transition-colors"
                  >
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                )}
              </div>
            </div>
          </div>

          {/* Filter Pills */}
          <div className="flex flex-wrap gap-2 mt-6 animate-fade-in-up" style={{ animationDelay: '0.2s' }}>
            <button className="px-4 py-2 rounded-full bg-white/20 backdrop-blur-md text-white text-sm font-medium hover:bg-white/30 transition-colors">
              All Photos
            </button>
            <button className="px-4 py-2 rounded-full bg-white/10 backdrop-blur-md text-white/80 text-sm font-medium hover:bg-white/20 transition-colors">
              Favorites
            </button>
            <button className="px-4 py-2 rounded-full bg-white/10 backdrop-blur-md text-white/80 text-sm font-medium hover:bg-white/20 transition-colors">
              Recently Added
            </button>
          </div>
        </div>

        {/* Bottom fade */}
        <div className="absolute bottom-0 left-0 right-0 h-12 bg-gradient-to-t from-gray-50 dark:from-dark-bg-primary to-transparent" />
      </div>

      {/* Main Content */}
      <div className="max-w-[1800px] mx-auto px-6 py-8">
        {/* Loading State */}
        {isLoading && (
          <div className="flex items-center justify-center py-32">
            <div className="flex flex-col items-center gap-6">
              <div className="relative">
                <div className="w-16 h-16 border-4 border-white/10 border-t-primary-600 dark:border-t-primary-500 rounded-full animate-spin" />
                <div className="absolute inset-0 w-16 h-16 border-4 border-transparent border-l-primary-400/30 rounded-full animate-spin" style={{ animationDirection: 'reverse' }} />
              </div>
              <div className="text-center">
                <span className="text-lg font-semibold text-gray-900 dark:text-dark-text-primary">Loading your photos...</span>
                <p className="text-sm text-gray-500 dark:text-dark-text-secondary mt-1">Preparing your memories</p>
              </div>
            </div>
          </div>
        )}

        {/* Empty State (No Photos) */}
        {!isLoading && photos.length === 0 && (
          <div className="flex flex-col items-center justify-center py-24">
            <div className="relative mb-8">
              <div className="w-32 h-32 rounded-3xl bg-gradient-to-br from-primary-100 to-purple-100 dark:from-dark-bg-tertiary dark:to-dark-border-default flex items-center justify-center shadow-xl float">
                <svg className="w-16 h-16 text-primary-400 dark:text-primary-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                    d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                  />
                </svg>
              </div>
              <div className="absolute -bottom-2 -right-2 w-12 h-12 rounded-2xl bg-gradient-to-br from-primary-500 to-purple-600 flex items-center justify-center shadow-lg pulse-glow">
                <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M12 4v16m8-8H4" />
                </svg>
              </div>
            </div>
            <h3 className="text-3xl font-bold text-gray-900 dark:text-dark-text-primary mb-3">No photos yet</h3>
            <p className="text-gray-600 dark:text-dark-text-secondary text-lg mb-8">Upload your first photos to get started</p>
            <button className="btn-primary-lg shadow-xl">
              Upload Photos
            </button>
          </div>
        )}

        {/* No Search Results */}
        {!isLoading && photos.length > 0 && filteredPhotos.length === 0 && (
          <div className="flex flex-col items-center justify-center py-24">
            <div className="w-24 h-24 mb-6 rounded-2xl bg-gray-100 dark:bg-dark-bg-tertiary flex items-center justify-center">
              <svg className="w-12 h-12 text-gray-400 dark:text-dark-text-secondary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
            <h3 className="text-2xl font-bold text-gray-900 dark:text-dark-text-primary mb-2">No photos found</h3>
            <p className="text-gray-600 dark:text-dark-text-secondary">Try a different search term</p>
          </div>
        )}

        {/* Photo Grid */}
        {!isLoading && filteredPhotos.length > 0 && (
          <PhotoGrid
            photos={filteredPhotos}
            onPhotoClick={handlePhotoClick}
            onLoadMore={handleLoadMore}
            hasMore={hasNextPage && !searchQuery}
            isLoading={isFetchingNextPage}
          />
        )}
      </div>

      {/* Selection bar */}
      <SelectionBar />

      {/* Lightbox */}
      {lightboxOpen && filteredPhotos.length > 0 && (
        <Lightbox
          photos={filteredPhotos}
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
