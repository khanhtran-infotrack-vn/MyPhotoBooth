import { useState, useEffect, useCallback } from 'react'
import type { SharedPhoto } from '../../types'
import { getSharedFileUrl, getSharedThumbnailUrl } from '../../hooks/useSharedContent'

interface SharedLightboxProps {
 token: string
 photos: SharedPhoto[]
 initialIndex: number
 onClose: () => void
}

export function SharedLightbox({ token, photos, initialIndex, onClose }: SharedLightboxProps) {
 const [currentIndex, setCurrentIndex] = useState(initialIndex)
 const [imageLoaded, setImageLoaded] = useState(false)

 const currentPhoto = photos[currentIndex]
 const hasPrev = currentIndex > 0
 const hasNext = currentIndex < photos.length - 1

 // Reset loaded state on index change
 useEffect(() => {
  setImageLoaded(false)
 }, [currentIndex])

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

 const handleDownload = () => {
  const url = getSharedFileUrl(token, currentPhoto.id)
  const a = document.createElement('a')
  a.href = url
  a.download = currentPhoto.fileName
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
 }

 if (!currentPhoto) return null

 const fullImageUrl = getSharedFileUrl(token, currentPhoto.id)
 const thumbUrl = getSharedThumbnailUrl(token, currentPhoto.id)

 return (
  <div className="fixed inset-0 z-50 bg-black">
   {/* Main content */}
   <div className="relative w-full h-full flex items-center justify-center">
    <div className="absolute inset-0" onClick={onClose} />

    {/* Image */}
    <div
     className="relative max-w-[90vw] max-h-[90vh] z-10"
     onClick={(e) => e.stopPropagation()}
    >
     {/* Show thumbnail as placeholder while full image loads */}
     {!imageLoaded && (
      <div className="flex items-center justify-center w-64 h-64">
       <img
        src={thumbUrl}
        alt={currentPhoto.fileName}
        className="max-w-[90vw] max-h-[90vh] object-contain rounded-lg opacity-50"
       />
      </div>
     )}
     <img
      src={fullImageUrl}
      alt={currentPhoto.fileName}
      className={`max-w-[90vw] max-h-[90vh] object-contain rounded-lg shadow-2xl ${
       imageLoaded ? '' : 'hidden'
      }`}
      onLoad={() => setImageLoaded(true)}
     />
    </div>

    {/* Nav arrows */}
    {hasPrev && (
     <button
      onClick={() => setCurrentIndex((i) => i - 1)}
      className="absolute left-4 z-20 p-2 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors"
     >
      <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
      </svg>
     </button>
    )}
    {hasNext && (
     <button
      onClick={() => setCurrentIndex((i) => i + 1)}
      className="absolute right-4 z-20 p-2 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors"
     >
      <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
      </svg>
     </button>
    )}
   </div>

   {/* Top bar */}
   <div className="absolute top-0 left-0 right-0 z-20 flex items-center justify-between p-4 bg-gradient-to-b from-black/50 to-transparent">
    <div className="text-white text-sm">
     {currentIndex + 1} / {photos.length}
    </div>

    <div className="flex items-center gap-2">
     {/* Download */}
     {currentPhoto.allowDownload && (
      <button
       onClick={handleDownload}
       className="p-2 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors"
       title="Download"
      >
       <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
         d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
        />
       </svg>
      </button>
     )}

     {/* Close */}
     <button
      onClick={onClose}
      className="p-2 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors"
      title="Close (Esc)"
     >
      <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
      </svg>
     </button>
    </div>
   </div>

   {/* Bottom bar */}
   <div className="absolute bottom-0 left-0 right-0 z-20 p-4 bg-gradient-to-t from-black/50 to-transparent">
    <div className="text-center text-white">
     <p className="font-medium">{currentPhoto.fileName}</p>
     <p className="text-sm text-white/70">
      {new Date(currentPhoto.capturedAt || currentPhoto.uploadedAt).toLocaleDateString(
       undefined,
       { year: 'numeric', month: 'long', day: 'numeric' }
      )}
     </p>
    </div>
   </div>
  </div>
 )
}
