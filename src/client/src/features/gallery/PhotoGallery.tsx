import { useState, useMemo } from 'react'
import { PhotoGrid, SelectionBar } from '../../components/photos'
import { Lightbox } from '../../components/lightbox'
import { BulkActionsBar } from '../../components/bulk'
import { Slideshow } from '../../components/slideshow'
import { usePhotos, getAllPhotosFromPages } from '../../hooks/usePhotos'
import { useFavorites, getAllFavoritesFromPages } from '../../hooks/useFavorites'
import { usePhotoSearch } from '../../hooks/usePhotoSearch'
import { useAlbums } from '../../hooks/useAlbums'
import { ShareModal } from '../sharing/ShareModal'
import { useSelectionStore } from '../../stores/selectionStore'
import { useBulkOperations } from '../../hooks/useBulkOperations'
import type { Photo } from '../../types'

type FilterType = 'all' | 'favorites' | 'recent' | 'search'

export default function PhotoGallery() {
 const [filterType, setFilterType] = useState<FilterType>('all')
 const [lightboxOpen, setLightboxOpen] = useState(false)
 const [lightboxIndex, setLightboxIndex] = useState(0)
 const [slideshowOpen, setSlideshowOpen] = useState(false)
 const [slideshowIndex, setSlideshowIndex] = useState(0)
 const [sharePhoto, setSharePhoto] = useState<Photo | null>(null)
 const [searchQuery, setSearchQuery] = useState('')
 const [showAlbumSelect, setShowAlbumSelect] = useState(false)

 const { isSelectionMode, clearSelection } = useSelectionStore()
 const { bulkAddToAlbum } = useBulkOperations()

 // All photos query
 const {
  data: allPhotosData,
  isLoading: isLoadingAllPhotos,
  isFetchingNextPage: isFetchingNextAllPhotos,
  hasNextPage: hasNextAllPhotos,
  fetchNextPage: fetchNextAllPhotos,
 } = usePhotos()

 // Favorites query
 const {
  data: favoritesData,
  isLoading: isLoadingFavorites,
  isFetchingNextPage: isFetchingNextFavorites,
  hasNextPage: hasNextFavorites,
  fetchNextPage: fetchNextFavorites,
 } = useFavorites()

 // Search query (server-side)
 const { data: searchData, isLoading: isSearching } = usePhotoSearch(searchQuery)

 // Albums query (for bulk add to album)
 const { data: albums } = useAlbums()

 // Flatten all pages into a single array based on filter type
 const allPhotos = useMemo(
  () => getAllPhotosFromPages(allPhotosData?.pages),
  [allPhotosData?.pages]
 )

 const favoritePhotos = useMemo(
  () => getAllFavoritesFromPages(favoritesData?.pages),
  [favoritesData?.pages]
 )

 const searchResults = useMemo(
  () => searchData?.items ?? [],
  [searchData?.items]
 )

 // Determine which data to use based on filter type
 const { photos, isLoading, isFetchingNextPage, hasNextPage, fetchNextPage, totalCount } = useMemo(() => {
  switch (filterType) {
   case 'favorites':
    return {
     photos: favoritePhotos,
     isLoading: isLoadingFavorites,
     isFetchingNextPage: isFetchingNextFavorites,
     hasNextPage: hasNextFavorites,
     fetchNextPage: fetchNextFavorites,
     totalCount: favoritesData?.pages[0]?.totalCount ?? 0,
    }
   case 'recent':
    // Recent uses same data as all photos (already sorted by uploadedAt DESC)
    return {
     photos: allPhotos,
     isLoading: isLoadingAllPhotos,
     isFetchingNextPage: isFetchingNextAllPhotos,
     hasNextPage: hasNextAllPhotos,
     fetchNextPage: fetchNextAllPhotos,
     totalCount: allPhotosData?.pages[0]?.totalCount ?? 0,
    }
   case 'search':
    return {
     photos: searchResults,
     isLoading: isSearching,
     isFetchingNextPage: false,
     hasNextPage: false,
     fetchNextPage: () => {},
     totalCount: searchData?.totalCount ?? 0,
    }
   default:
    return {
     photos: allPhotos,
     isLoading: isLoadingAllPhotos,
     isFetchingNextPage: isFetchingNextAllPhotos,
     hasNextPage: hasNextAllPhotos,
     fetchNextPage: fetchNextAllPhotos,
     totalCount: allPhotosData?.pages[0]?.totalCount ?? 0,
    }
  }
 }, [
  filterType,
  allPhotos,
  favoritePhotos,
  searchResults,
  isLoadingAllPhotos,
  isLoadingFavorites,
  isSearching,
  isFetchingNextAllPhotos,
  isFetchingNextFavorites,
  hasNextAllPhotos,
  hasNextFavorites,
  fetchNextAllPhotos,
  fetchNextFavorites,
  allPhotosData?.pages,
  favoritesData?.pages,
  searchData,
 ])

 const handlePhotoClick = (_photo: Photo, index: number) => {
  setLightboxIndex(index)
  setLightboxOpen(true)
 }

 const handleFilterChange = (newFilter: FilterType) => {
  setFilterType(newFilter)
  setSearchQuery('') // Clear search when changing filters
 }

 const handleSearchChange = (value: string) => {
  setSearchQuery(value)
  if (value.trim().length >= 2) {
   setFilterType('search')
  } else if (value.trim().length === 0) {
   setFilterType('all')
  }
 }

 const handleLoadMore = () => {
  if (!isFetchingNextPage && hasNextPage) {
   fetchNextPage()
  }
 }

 const handleStartSlideshow = () => {
  setSlideshowIndex(lightboxIndex)
  setSlideshowOpen(true)
  setLightboxOpen(false)
 }

 const handleAddToAlbum = async (albumId: string) => {
  const selectedIds = Array.from(useSelectionStore.getState().selectedIds)
  await bulkAddToAlbum({ photoIds: selectedIds, albumId })
  setShowAlbumSelect(false)
  clearSelection()
 }

 // Slideshow button in toolbar
 const SlideshowButton = () => (
  <button
   onClick={() => {
    setSlideshowIndex(0)
    setSlideshowOpen(true)
   }}
   disabled={photos.length === 0}
   className="flex items-center gap-2 px-4 py-2 rounded-lg bg-white border border-gray-200 shadow-sm hover:shadow-md hover:border-primary-300 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
  >
   <svg className="w-5 h-5 text-primary-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
   </svg>
   <span className="text-sm font-medium text-gray-700">Slideshow</span>
  </button>
 )

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

      {/* Toolbar with Search and Slideshow */}
      <div className="flex items-center gap-3">
       <SlideshowButton />
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
          onChange={(e) => handleSearchChange(e.target.value)}
          placeholder="Search photos by name, description, tags..."
          className="w-full pl-10 pr-10 py-2.5 bg-white border border-gray-300 rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20 transition-all duration-200 shadow-sm"
         />
         {searchQuery && (
          <button
           onClick={() => handleSearchChange('')}
           className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition-colors"
          >
           <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
           </svg>
          </button>
         )}
         {isSearching && (
          <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
           <div className="w-4 h-4 border-2 border-gray-400 border-t-primary-600 rounded-full animate-spin" />
          </div>
         )}
        </div>
       </div>
      </div>
     </div>

     {/* Filter Pills */}
     <div className="flex flex-wrap gap-2 mt-6">
      <button
       onClick={() => handleFilterChange('all')}
       className={`hero-filter-pill-minimal ${filterType === 'all' ? 'hero-filter-pill-minimal-active' : ''}`}
      >
       <svg className="w-4 h-4 mr-1.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
       </svg>
       All Photos
      </button>
      <button
       onClick={() => handleFilterChange('favorites')}
       className={`hero-filter-pill-minimal ${filterType === 'favorites' ? 'hero-filter-pill-minimal-active' : ''}`}
      >
       <svg className="w-4 h-4 mr-1.5" fill="currentColor" viewBox="0 0 24 24">
        <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
       </svg>
       Favorites
      </button>
      <button
       onClick={() => handleFilterChange('recent')}
       className={`hero-filter-pill-minimal ${filterType === 'recent' ? 'hero-filter-pill-minimal-active' : ''}`}
      >
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
    {!isLoading && filterType === 'search' && photos.length === 0 && searchQuery.trim().length >= 2 && (
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

    {/* Empty Favorites */}
    {!isLoading && filterType === 'favorites' && photos.length === 0 && (
     <div className="flex flex-col items-center justify-center py-24">
      <div className="w-24 h-24 mb-6 rounded-2xl bg-gray-100 flex items-center justify-center">
       <svg className="w-12 h-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
         d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z"
        />
       </svg>
      </div>
      <h3 className="text-2xl font-bold text-gray-900 mb-2">No favorites yet</h3>
      <p className="text-gray-600">Mark photos as favorites to see them here</p>
     </div>
    )}

    {/* Photo Grid */}
    {!isLoading && photos.length > 0 && (
     <PhotoGrid
      photos={photos}
      onPhotoClick={handlePhotoClick}
      onLoadMore={handleLoadMore}
      hasMore={hasNextPage && filterType !== 'search'}
      isLoading={isFetchingNextPage}
     />
    )}
   </div>

   {/* Bulk Actions Bar */}
   {isSelectionMode && <BulkActionsBar onOpenAlbumSelect={() => setShowAlbumSelect(true)} />}

   {/* Selection bar */}
   <SelectionBar />

   {/* Album Select Modal */}
   {showAlbumSelect && (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
     <div className="bg-white rounded-2xl p-6 max-w-md w-full mx-4 shadow-2xl">
      <h3 className="text-xl font-bold text-gray-900 mb-4">Add to Album</h3>
      {albums && albums.length > 0 ? (
       <div className="space-y-2 max-h-96 overflow-y-auto">
        {albums.map((album) => (
         <button
          key={album.id}
          onClick={() => handleAddToAlbum(album.id)}
          className="w-full text-left px-4 py-3 rounded-lg hover:bg-gray-100 transition-colors"
         >
          {album.name}
         </button>
        ))}
       </div>
      ) : (
       <p className="text-gray-500">No albums available. Create an album first.</p>
      )}
      <button
       onClick={() => setShowAlbumSelect(false)}
       className="mt-4 w-full px-4 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50"
      >
       Cancel
      </button>
     </div>
    </div>
   )}

   {/* Lightbox */}
   {lightboxOpen && photos.length > 0 && (
    <Lightbox
     photos={photos}
     initialIndex={lightboxIndex}
     onClose={() => setLightboxOpen(false)}
     onShare={(photo) => {
      setSharePhoto(photo)
     }}
     onStartSlideshow={handleStartSlideshow}
    />
   )}

   {/* Slideshow */}
   {slideshowOpen && photos.length > 0 && (
    <Slideshow
     photos={photos}
     initialIndex={slideshowIndex}
     onClose={() => setSlideshowOpen(false)}
     autoStart={true}
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
