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
 const { isSelectionMode, selectedIds, toggleSelection } = useSelectionStore()
 const [hoveredPhotoId, setHoveredPhotoId] = useState<string | null>(null)

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
  return null // Empty state is handled in parent
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
         const isHovered = hoveredPhotoId === originalPhoto?.id
         return (
          <AuthenticatedImage
           src={photo.src}
           alt={originalPhoto?.originalFileName || 'Photo'}
           className={`w-full h-full object-cover transition-all duration-300 ${
            isHovered ? 'scale-105' : 'scale-100'
           }`}
           style={props.style}
           loading="lazy"
          />
         )
        },
        wrapper: ({ children, style }, { photo }) => {
         const originalPhoto = (photo as any).originalPhoto as Photo
         const isSelected = selectedIds.has(originalPhoto?.id || '')
         const isHovered = hoveredPhotoId === originalPhoto?.id

         return (
          <div
           className={`relative group cursor-pointer overflow-hidden rounded-xl shadow-sm hover:shadow-xl transition-all duration-300 ${
            isSelected ? 'ring-4 ring-primary-500 shadow-lg shadow-primary-500/20' : ''
           }`}
           style={style}
           onMouseEnter={() => setHoveredPhotoId(originalPhoto?.id || null)}
           onMouseLeave={() => setHoveredPhotoId(null)}
          >
           {children}

           {/* Gradient overlay on hover */}
           <div className={`absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent transition-opacity duration-300 ${
            isHovered ? 'opacity-100' : 'opacity-0'
           }`} />

           {/* Selection tint */}
           {isSelected && (
            <div className="absolute inset-0 bg-primary-500/20 backdrop-blur-[1px]" />
           )}

           {/* Selection checkbox */}
           <div
            className={`absolute top-3 left-3 transition-all duration-200 z-10 ${
             isSelectionMode || isSelected ? 'opacity-100 scale-100' : 'opacity-0 group-hover:opacity-100 scale-90 group-hover:scale-100'
            }`}
           >
            <button
             onClick={(e) => {
              e.stopPropagation()
              if (originalPhoto) toggleSelection(originalPhoto.id)
             }}
             className={`w-7 h-7 rounded-xl border-2 flex items-center justify-center transition-all duration-200 shadow-lg backdrop-blur-md ${
              isSelected
               ? 'bg-primary-600 border-primary-600 text-white'
               : 'bg-white/80 border-gray-300 hover:border-primary-500'
             }`}
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
