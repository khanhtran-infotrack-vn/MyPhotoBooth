import { useState } from 'react'
import { useSelectionStore } from '../../stores/selectionStore'
import { useDeletePhotos } from '../../hooks/usePhotos'
import { AddToAlbumModal } from '../../features/albums/AddToAlbumModal'

export function SelectionBar() {
  const { selectedIds, clearSelection, exitSelectionMode } = useSelectionStore()
  const deletePhotos = useDeletePhotos()
  const [isDeleting, setIsDeleting] = useState(false)
  const [showAddToAlbum, setShowAddToAlbum] = useState(false)

  const count = selectedIds.size

  if (count === 0) return null

  const handleDelete = async () => {
    if (!confirm(`Delete ${count} photo${count > 1 ? 's' : ''}? This cannot be undone.`)) {
      return
    }

    setIsDeleting(true)
    try {
      await deletePhotos.mutateAsync(Array.from(selectedIds))
      clearSelection()
    } catch (error) {
      console.error('Failed to delete photos:', error)
    } finally {
      setIsDeleting(false)
    }
  }

  const handleCancel = () => {
    exitSelectionMode()
  }

  return (
    <>
      <div className="fixed bottom-6 left-1/2 -translate-x-1/2 z-40 animate-slide-in">
        <div className="flex items-center gap-4 px-6 py-3 bg-gray-900 text-white rounded-full shadow-2xl">
          <span className="text-sm font-medium">
            {count} selected
          </span>

          <div className="w-px h-5 bg-gray-600" />

          <div className="flex items-center gap-2">
            <button
              onClick={() => setShowAddToAlbum(true)}
              className="p-2 rounded-full hover:bg-gray-700 transition-colors"
              title="Add to album"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                  d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                />
              </svg>
            </button>

            <button
              onClick={handleDelete}
              disabled={isDeleting}
              className="p-2 rounded-full hover:bg-red-600 transition-colors disabled:opacity-50"
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
          </div>

          <div className="w-px h-5 bg-gray-600" />

          <button
            onClick={handleCancel}
            className="text-sm font-medium hover:text-gray-300 transition-colors"
          >
            Cancel
          </button>
        </div>
      </div>

      {showAddToAlbum && (
        <AddToAlbumModal
          photoIds={Array.from(selectedIds)}
          onClose={() => setShowAddToAlbum(false)}
        />
      )}
    </>
  )
}
