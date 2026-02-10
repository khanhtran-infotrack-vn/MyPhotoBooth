import { useState } from 'react'
import type { Photo } from '../../types'
import { useDeletePhoto, getPhotoUrl } from '../../hooks/usePhotos'
import { useToggleFavorite } from '../../hooks/useFavorites'
import api from '../../lib/api'

interface LightboxActionsProps {
 photo: Photo
 onClose: () => void
 onToggleInfo: () => void
 showInfo: boolean
 onShare?: () => void
}

function ActionButton({
 onClick,
 disabled,
 isLoading,
 icon,
 title,
 variant = 'default',
}: {
 onClick?: () => void
 disabled?: boolean
 isLoading?: boolean
 icon: React.ReactNode
 title: string
 variant?: 'default' | 'danger' | 'active'
}) {
 const baseClasses = "w-10 h-10 sm:w-11 sm:h-11 rounded-xl flex items-center justify-center transition-all duration-200 backdrop-blur-sm border"
 const variantClasses = {
  default: "bg-black/40 hover:bg-black/60 border-white/10 hover:border-white/20 text-white/70 hover:text-white hover:scale-110",
  danger: "bg-black/40 hover:bg-red-600/30 border-white/10 hover:border-red-500/50 text-white/70 hover:text-red-400 hover:scale-110",
  active: "bg-white/20 border-white/30 text-white shadow-lg shadow-white/10",
 }

 return (
  <button
   onClick={onClick}
   disabled={disabled || isLoading}
   className={`${baseClasses} ${variantClasses[variant]} disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 group`}
   title={title}
  >
   {isLoading ? (
    <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
   ) : (
    <span className="transition-transform group-hover:scale-110">{icon}</span>
   )}
  </button>
 )
}

export function LightboxActions({
 photo,
 onClose,
 onToggleInfo,
 showInfo,
 onShare,
}: LightboxActionsProps) {
 const deletePhoto = useDeletePhoto()
 const toggleFavorite = useToggleFavorite()
 const [isDeleting, setIsDeleting] = useState(false)
 const [isDownloading, setIsDownloading] = useState(false)
 const [isTogglingFavorite, setIsTogglingFavorite] = useState(false)

 const handleDelete = async () => {
  if (!confirm('Delete this photo? This cannot be undone.')) return

  setIsDeleting(true)
  try {
   await deletePhoto.mutateAsync(photo.id)
   onClose()
  } catch (error) {
   console.error('Failed to delete photo:', error)
  } finally {
   setIsDeleting(false)
  }
 }

 const handleDownload = async () => {
  setIsDownloading(true)
  try {
   const response = await api.get(getPhotoUrl(photo.id).replace('/api', ''), {
    responseType: 'blob',
   })

   const url = URL.createObjectURL(response.data)
   const a = document.createElement('a')
   a.href = url
   a.download = photo.originalFileName
   document.body.appendChild(a)
   a.click()
   document.body.removeChild(a)
   URL.revokeObjectURL(url)
  } catch (error) {
   console.error('Failed to download photo:', error)
  } finally {
   setIsDownloading(false)
  }
 }

 const handleToggleFavorite = async () => {
  setIsTogglingFavorite(true)
  try {
   await toggleFavorite.mutateAsync(photo.id)
  } catch (error) {
   console.error('Failed to toggle favorite:', error)
  } finally {
   setIsTogglingFavorite(false)
  }
 }

 return (
  <div className="flex items-center gap-2">
   {/* Favorite */}
   <ActionButton
    onClick={handleToggleFavorite}
    isLoading={isTogglingFavorite}
    title={photo.isFavorite ? 'Remove from favorites' : 'Add to favorites'}
    variant={photo.isFavorite ? 'active' : 'default'}
    icon={
     <svg className="w-5 h-5" fill={photo.isFavorite ? 'currentColor' : 'none'} viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
       d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z"
      />
     </svg>
    }
   />

   {/* Download */}
   <ActionButton
    onClick={handleDownload}
    isLoading={isDownloading}
    title="Download"
    icon={
     <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
       d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
      />
     </svg>
    }
   />

   {/* Share */}
   {onShare && (
    <ActionButton
     onClick={onShare}
     title="Share"
     icon={
      <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
        d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z"
       />
      </svg>
     }
    />
   )}

   {/* Info toggle */}
   <ActionButton
    onClick={onToggleInfo}
    title="Info (i)"
    variant={showInfo ? 'active' : 'default'}
    icon={
     <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
       d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
      />
     </svg>
    }
   />

   {/* Delete */}
   <ActionButton
    onClick={handleDelete}
    isLoading={isDeleting}
    title="Delete"
    variant="danger"
    icon={
     <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
       d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
      />
     </svg>
    }
   />

   {/* Close */}
   <ActionButton
    onClick={onClose}
    title="Close (Esc)"
    icon={
     <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M6 18L18 6M6 6l12 12" />
     </svg>
    }
   />
  </div>
 )
}
