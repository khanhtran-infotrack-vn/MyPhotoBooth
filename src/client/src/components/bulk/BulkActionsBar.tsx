import { useSelectionStore } from '../../stores/selectionStore'
import { useBulkOperations } from '../../hooks/useBulkOperations'
import { useBulkDownload } from '../../hooks/useBulkDownload'
import { ConfirmBulkActionModal } from './ConfirmBulkActionModal'
import { useState } from 'react'

interface BulkActionsBarProps {
  onOpenAlbumSelect?: () => void
  availableAlbums?: Array<{ id: string; name: string }>
}

export function BulkActionsBar({ onOpenAlbumSelect }: BulkActionsBarProps) {
  const { selectedIds, clearSelection } = useSelectionStore()
  const {
    bulkDelete,
    bulkToggleFavorite,
    isLoading
  } = useBulkOperations()

  const { downloadPhotos, isLoading: isDownloading } = useBulkDownload()

  const [confirmModal, setConfirmModal] = useState<{
    show: boolean
    action: 'delete' | 'favorite' | 'unfavorite' | null
    title: string
    message: string
  }>({ show: false, action: null, title: '', message: '' })

  const selectedCount = selectedIds.size

  if (selectedCount === 0) return null

  const handleDelete = () => {
    setConfirmModal({
      show: true,
      action: 'delete',
      title: `Delete ${selectedCount} photo${selectedCount > 1 ? 's' : ''}`,
      message: `Are you sure you want to delete ${selectedCount} photo${selectedCount > 1 ? 's' : ''}? This action cannot be undone.`
    })
  }

  const handleFavorite = (favorite: boolean) => {
    setConfirmModal({
      show: true,
      action: favorite ? 'favorite' : 'unfavorite',
      title: `${favorite ? 'Add' : 'Remove'} ${selectedCount} photo${selectedCount > 1 ? 's' : ''} to ${favorite ? 'Favorites' : 'Favorites'}`,
      message: `Are you sure you want to ${favorite ? 'add' : 'remove'} ${selectedCount} photo${selectedCount > 1 ? 's' : ''} ${favorite ? 'to' : 'from'} favorites?`
    })
  }

  const handleConfirmAction = async () => {
    const photoIds = Array.from(selectedIds)

    try {
      switch (confirmModal.action) {
        case 'delete':
          await bulkDelete(photoIds)
          break
        case 'favorite':
          await bulkToggleFavorite(photoIds, true)
          break
        case 'unfavorite':
          await bulkToggleFavorite(photoIds, false)
          break
      }
      clearSelection()
      setConfirmModal({ show: false, action: null, title: '', message: '' })
    } catch (error) {
      console.error('Bulk action failed:', error)
    }
  }

  const handleDownload = async () => {
    const photoIds = Array.from(selectedIds)
    await downloadPhotos(photoIds)
    clearSelection()
  }

  const handleAddToAlbum = () => {
    if (onOpenAlbumSelect) {
      onOpenAlbumSelect()
    }
  }

  return (
    <>
      <div className="fixed bottom-6 left-1/2 transform -translate-x-1/2 z-40">
        <div className="bg-white rounded-2xl shadow-2xl border border-gray-200 px-6 py-4 flex items-center gap-4">
          <span className="text-sm font-semibold text-gray-700">
            {selectedCount} photo{selectedCount > 1 ? 's' : ''} selected
          </span>

          <div className="h-6 w-px bg-gray-200" />

          <div className="flex items-center gap-2">
            <button
              onClick={handleDownload}
              disabled={isDownloading || selectedCount > 50}
              className={`p-2 rounded-lg transition-all ${
                isDownloading || selectedCount > 50
                  ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                  : 'bg-primary-50 text-primary-600 hover:bg-primary-100'
              }`}
              title={selectedCount > 50 ? 'Maximum 50 photos for download' : 'Download as ZIP'}
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
              </svg>
            </button>

            <button
              onClick={() => handleFavorite(true)}
              disabled={isLoading}
              className="p-2 rounded-lg bg-yellow-50 text-yellow-600 hover:bg-yellow-100 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              title="Add to favorites"
            >
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
              </svg>
            </button>

            <button
              onClick={() => handleFavorite(false)}
              disabled={isLoading}
              className="p-2 rounded-lg bg-gray-50 text-gray-600 hover:bg-gray-100 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              title="Remove from favorites"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z" />
              </svg>
            </button>

            {onOpenAlbumSelect && (
              <button
                onClick={handleAddToAlbum}
                disabled={isLoading}
                className="p-2 rounded-lg bg-purple-50 text-purple-600 hover:bg-purple-100 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                title="Add to album"
              >
                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
              </button>
            )}

            <button
              onClick={handleDelete}
              disabled={isLoading}
              className="p-2 rounded-lg bg-red-50 text-red-600 hover:bg-red-100 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              title="Delete"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </button>
          </div>

          <button
            onClick={clearSelection}
            className="ml-2 text-sm font-medium text-gray-500 hover:text-gray-700 transition-colors"
          >
            Cancel
          </button>
        </div>
      </div>

      <ConfirmBulkActionModal
        isOpen={confirmModal.show}
        title={confirmModal.title}
        message={confirmModal.message}
        onConfirm={handleConfirmAction}
        onCancel={() => setConfirmModal({ show: false, action: null, title: '', message: '' })}
        isLoading={isLoading}
      />
    </>
  )
}
