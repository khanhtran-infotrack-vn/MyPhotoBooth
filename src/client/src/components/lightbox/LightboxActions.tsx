import { useState } from 'react'
import type { Photo } from '../../types'
import { useDeletePhoto, getPhotoUrl } from '../../hooks/usePhotos'
import api from '../../lib/api'

interface LightboxActionsProps {
  photo: Photo
  onClose: () => void
  onToggleInfo: () => void
  showInfo: boolean
  onShare?: () => void
}

export function LightboxActions({
  photo,
  onClose,
  onToggleInfo,
  showInfo,
  onShare,
}: LightboxActionsProps) {
  const deletePhoto = useDeletePhoto()
  const [isDeleting, setIsDeleting] = useState(false)
  const [isDownloading, setIsDownloading] = useState(false)

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

  return (
    <div className="flex items-center gap-2">
      {/* Download */}
      <button
        onClick={handleDownload}
        disabled={isDownloading}
        className="p-2 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors disabled:opacity-50"
        title="Download"
      >
        {isDownloading ? (
          <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
        ) : (
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
              d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
            />
          </svg>
        )}
      </button>

      {/* Share */}
      {onShare && (
        <button
          onClick={onShare}
          className="p-2 text-white/70 hover:text-white hover:bg-white/10 rounded-full transition-colors"
          title="Share"
        >
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
              d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z"
            />
          </svg>
        </button>
      )}

      {/* Info toggle */}
      <button
        onClick={onToggleInfo}
        className={`p-2 rounded-full transition-colors ${
          showInfo
            ? 'text-white bg-white/20'
            : 'text-white/70 hover:text-white hover:bg-white/10'
        }`}
        title="Info (i)"
      >
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
            d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
      </button>

      {/* Delete */}
      <button
        onClick={handleDelete}
        disabled={isDeleting}
        className="p-2 text-white/70 hover:text-red-400 hover:bg-white/10 rounded-full transition-colors disabled:opacity-50"
        title="Delete"
      >
        {isDeleting ? (
          <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
        ) : (
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
              d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
            />
          </svg>
        )}
      </button>

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
  )
}
