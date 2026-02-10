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
   {/* Minimal Accent Hero Header */}
   <div className="relative bg-white border-b border-gray-200">
    {/* Header Content */}
    <div className="max-w-[1800px] mx-auto px-6 py-10 sm:py-12 lg:py-14">
     <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-5">
      {/* Title Section */}
      <div className="flex-1">
       <h1 className="text-3xl sm:text-4xl lg:text-5xl font-bold bg-gradient-to-r from-primary-600 to-purple-600 bg-clip-text text-transparent mb-2 tracking-tight">
        Photos
       </h1>
       {/* Gradient accent line below title */}
       <div className="h-0.5 w-16 bg-gradient-to-r from-primary-600 to-purple-600 rounded-full mb-3" />
       {totalCount > 0 && (
        <p className="text-gray-500 text-sm sm:text-base">
         {totalCount.toLocaleString()} {totalCount === 1 ? 'memory' : 'memories'} captured
        </p>
       )}
      </div>

      {/* Search Bar */}
      <div className="w-full sm:w-72 lg:w-80">
       <div className="relative">
        <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none">
         <svg className="w-4.5 h-4.5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
         </svg>
        </div>
        <input
         type="text"
         value={searchQuery}
         onChange={(e) => setSearchQuery(e.target.value)}
         placeholder="Search photos..."
         className="w-full pl-10 pr-10 py-2.5 bg-white border border-gray-300 rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20 transition-all duration-200 shadow-sm"
        />
        {searchQuery && (
         <button
          onClick={() => setSearchQuery('')}
          className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition-colors"
         >
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
           <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
         </button>
        )}
       </div>
      </div>
     </div>

     {/* Filter Pills */}
     <div className="flex flex-wrap gap-2 mt-6">
      <button className="hero-filter-pill-minimal hero-filter-pill-minimal-active">
       <svg className="w-4 h-4 mr-1.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
       </svg>
       All Photos
      </button>
      <button className="hero-filter-pill-minimal">
       <svg className="w-4 h-4 mr-1.5" fill="currentColor" viewBox="0 0 24 24">
        <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
       </svg>
       Favorites
      </button>
      <button className="hero-filter-pill-minimal">
       <svg className="w-4 h-4 mr-1.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
       </svg>
       Recently Added
      </button>
     </div>
    </div>

    {/* Bottom fade transition */}
    <div className="absolute bottom-0 left-0 right-0 h-12 bg-gradient-to-t from-gray-50 to-transparent pointer-events-none" />
   </div>

   {/* Main Content */}
   <div className="bg-gray-50">
    <div className="max-w-[1800px] mx-auto px-6 py-8">
    {/* Loading State */}
    {isLoading && (
     <div className="flex items-center justify-center py-32">
      <div className="flex flex-col items-center gap-6">
       <div className="relative">
        <div className="w-16 h-16 border-4 border-white/10 border-t-primary-600 rounded-full animate-spin" />
        <div className="absolute inset-0 w-16 h-16 border-4 border-transparent border-l-primary-400/30 rounded-full animate-spin" style={{ animationDirection: 'reverse' }} />
       </div>
       <div className="text-center">
        <span className="text-lg font-semibold text-gray-900">Loading your photos...</span>
        <p className="text-sm text-gray-500 mt-1">Preparing your memories</p>
       </div>
      </div>
     </div>
    )}

    {/* Empty State (No Photos) */}
    {!isLoading && photos.length === 0 && (
     <div className="flex flex-col items-center justify-center py-24">
      <div className="relative mb-8">
       <div className="w-32 h-32 rounded-3xl bg-gradient-to-br from-primary-100 to-purple-100 flex items-center justify-center shadow-xl float">
        <svg className="w-16 h-16 text-primary-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
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
      <h3 className="text-3xl font-bold text-gray-900 mb-3">No photos yet</h3>
      <p className="text-gray-600 text-lg mb-8">Upload your first photos to get started</p>
      <button className="btn-primary-lg shadow-xl">
       Upload Photos
      </button>
     </div>
    )}

    {/* No Search Results */}
    {!isLoading && photos.length > 0 && filteredPhotos.length === 0 && (
     <div className="flex flex-col items-center justify-center py-24">
      <div className="w-24 h-24 mb-6 rounded-2xl bg-gray-100 flex items-center justify-center">
       <svg className="w-12 h-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
       </svg>
      </div>
      <h3 className="text-2xl font-bold text-gray-900 mb-2">No photos found</h3>
      <p className="text-gray-600">Try a different search term</p>
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
  </div>
 )
}
