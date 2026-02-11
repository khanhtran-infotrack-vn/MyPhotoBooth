import { useState, useMemo, useEffect } from 'react'
import { Link, useParams } from 'react-router-dom'
import { PhotoGrid } from '../../components/photos/PhotoGrid'
import { SelectionToggleButton } from '../../components/photos'
import { Lightbox } from '../../components/lightbox'
import { BulkActionsBar } from '../../components/bulk'
import { useTagPhotos, getAllTagPhotosFromPages, useTagsWithCount } from '../../hooks/useTags'
import { SelectionContextProvider } from '../../contexts/SelectionContext'
import { useSelectionStore } from '../../stores/selectionStore'
import type { Photo } from '../../types'

interface TagPhotosContentProps {
 tagId: string
}

function TagPhotosContent({ tagId }: TagPhotosContentProps) {
 const [lightboxOpen, setLightboxOpen] = useState(false)
 const [lightboxIndex, setLightboxIndex] = useState(0)

 const { setContext } = useSelectionStore()

 const {
   data: tagPhotosData,
   isLoading: isLoadingPhotos,
   isFetchingNextPage,
   hasNextPage,
   fetchNextPage,
 } = useTagPhotos(tagId ?? '', 50)

 const { data: tags = [] } = useTagsWithCount()

 // Update selection context for tag view
 useEffect(() => {
  setContext({ view: 'tags', entityId: tagId })
 }, [tagId, setContext])

 // Find the current tag from the tags list
 const currentTag = useMemo(
   () => tags.find(t => t.id === tagId),
   [tags, tagId]
 )

 // Flatten all pages into a single array
 const photos = useMemo(
   () => getAllTagPhotosFromPages(tagPhotosData?.pages),
   [tagPhotosData?.pages]
 )

 const totalCount = tagPhotosData?.pages?.[0]?.totalCount ?? 0

 const handlePhotoClick = (_photo: Photo, index: number) => {
   setLightboxIndex(index)
   setLightboxOpen(true)
 }

 // Scroll to top on mount
 useEffect(() => {
   window.scrollTo({ top: 0, behavior: 'smooth' })
 }, [tagId])

 if (!tagId) {
   return (
     <div className="p-6">
       <div className="flex flex-col items-center justify-center py-20 text-gray-500">
         <h3 className="text-xl font-semibold text-gray-700">Invalid tag ID</h3>
         <Link to="/tags" className="btn-secondary mt-4">
           Back to Tags
         </Link>
       </div>
     </div>
   )
 }

 if (isLoadingPhotos && !photos.length) {
   return (
     <div className="p-6">
       <div className="flex items-center justify-center py-32">
         <div className="flex flex-col items-center gap-4 text-gray-600">
           <div className="w-12 h-12 border-3 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
           <span className="text-lg font-medium">Loading photos...</span>
         </div>
       </div>
     </div>
   )
 }

 return (
   <div className="p-6">
     {/* Header */}
     <div className="mb-6">
       <div className="flex items-center justify-between">
         <div className="flex items-center gap-2 text-sm text-gray-500 mb-2">
           <Link to="/tags" className="hover:text-primary-600">Tags</Link>
           <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
             <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
           </svg>
           <span className="text-gray-700">{currentTag?.name || 'Tag Photos'}</span>
         </div>
         <SelectionToggleButton photoCount={photos.length} />
       </div>
       <h1 className="text-3xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent">
         {currentTag?.name || 'Tag Photos'}
       </h1>
       <p className="text-sm text-gray-600 mt-2">
         {totalCount} {totalCount === 1 ? 'photo' : 'photos'}
       </p>
     </div>

     {/* Empty state */}
     {!isLoadingPhotos && photos.length === 0 && (
       <div className="flex flex-col items-center justify-center py-20 text-gray-500">
         <svg className="w-20 h-20 mb-4 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
           <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
             d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A2 2 0 013 12V7a4 4 0 014-4z"
           />
         </svg>
         <h3 className="text-xl font-semibold text-gray-700">No photos with this tag</h3>
         <p className="mt-2 text-gray-500">
           Photos tagged with "{currentTag?.name || 'this tag'}" will appear here
         </p>
         <Link to="/photos" className="btn-secondary mt-4">
           Browse Photos
         </Link>
       </div>
     )}

     {/* Photo grid */}
     {photos.length > 0 && (
       <PhotoGrid
         photos={photos}
         onPhotoClick={handlePhotoClick}
         onLoadMore={hasNextPage ? () => fetchNextPage() : undefined}
         hasMore={hasNextPage ?? false}
         isLoading={isFetchingNextPage}
       />
     )}

     {/* Bulk Actions Bar */}
     {useSelectionStore.getState().isSelectionMode && <BulkActionsBar />}

     {/* Lightbox */}
     {lightboxOpen && (
       <Lightbox
         photos={photos}
         initialIndex={lightboxIndex}
         onClose={() => setLightboxOpen(false)}
       />
     )}
   </div>
 )
}

// Wrapper component that provides SelectionContext
export default function TagPhotos() {
 const { id: tagId } = useParams<{ id: string }>()

 if (!tagId) {
   return (
     <div className="p-6">
       <div className="flex flex-col items-center justify-center py-20 text-gray-500">
         <h3 className="text-xl font-semibold text-gray-700">Invalid tag ID</h3>
         <Link to="/tags" className="btn-secondary mt-4">
           Back to Tags
         </Link>
       </div>
     </div>
   )
 }

 return (
  <SelectionContextProvider context={{ view: 'tags', entityId: tagId }}>
   <TagPhotosContent tagId={tagId} />
  </SelectionContextProvider>
 )
}
