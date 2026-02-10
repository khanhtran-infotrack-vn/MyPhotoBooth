import { useState, useEffect } from 'react'
import { useAlbums, useAddPhotosToAlbum, useCreateAlbum } from '../../hooks/useAlbums'
import { getThumbnailUrl } from '../../hooks/usePhotos'
import { AuthenticatedImage } from '../../components/photos'
import { useSelectionStore } from '../../stores/selectionStore'

interface AddToAlbumModalProps {
 photoIds: string[]
 onClose: () => void
}

export function AddToAlbumModal({ photoIds, onClose }: AddToAlbumModalProps) {
 const [selectedAlbumId, setSelectedAlbumId] = useState<string | null>(null)
 const [showCreate, setShowCreate] = useState(false)
 const [newAlbumName, setNewAlbumName] = useState('')
 const [error, setError] = useState('')

 const { data: albums, isLoading } = useAlbums()
 const addPhotos = useAddPhotosToAlbum()
 const createAlbum = useCreateAlbum()
 const clearSelection = useSelectionStore((s) => s.clearSelection)

 // Close on Escape
 useEffect(() => {
  const handleKeyDown = (e: KeyboardEvent) => {
   if (e.key === 'Escape') onClose()
  }
  document.addEventListener('keydown', handleKeyDown)
  return () => document.removeEventListener('keydown', handleKeyDown)
 }, [onClose])

 const handleAddToExisting = async () => {
  if (!selectedAlbumId) return
  setError('')

  try {
   await addPhotos.mutateAsync({ albumId: selectedAlbumId, photoIds })
   clearSelection()
   onClose()
  } catch {
   setError('Failed to add photos to album')
  }
 }

 const handleCreateAndAdd = async () => {
  if (!newAlbumName.trim()) {
   setError('Album name is required')
   return
  }
  setError('')

  try {
   const album = await createAlbum.mutateAsync({ name: newAlbumName.trim() })
   await addPhotos.mutateAsync({ albumId: album.id, photoIds })
   clearSelection()
   onClose()
  } catch {
   setError('Failed to create album')
  }
 }

 const isSubmitting = addPhotos.isPending || createAlbum.isPending

 return (
  <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
   {/* Backdrop */}
   <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={onClose} />

   {/* Modal */}
   <div className="relative w-full max-w-md bg-white rounded-2xl shadow-2xl animate-scale-in max-h-[80vh] flex flex-col">
    {/* Header */}
    <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200 flex-shrink-0">
     <h2 className="text-lg font-semibold text-gray-900">
      Add {photoIds.length} {photoIds.length === 1 ? 'photo' : 'photos'} to album
     </h2>
     <button onClick={onClose} className="btn-icon text-gray-400 hover:text-gray-600">
      <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
      </svg>
     </button>
    </div>

    {/* Content */}
    <div className="p-6 overflow-y-auto flex-1">
     {error && (
      <div className="px-4 py-3 mb-4 text-sm font-medium text-red-700 bg-red-50 rounded-lg">
       {error}
      </div>
     )}

     {/* Create new toggle */}
     <button
      onClick={() => setShowCreate(!showCreate)}
      className="w-full flex items-center gap-3 px-4 py-3 rounded-lg border border-dashed border-gray-300 hover:border-primary-500 hover:bg-primary-50 transition-colors mb-4"
     >
      <div className="w-10 h-10 rounded-lg bg-primary-100 flex items-center justify-center">
       <svg className="w-5 h-5 text-primary-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
       </svg>
      </div>
      <span className="font-medium text-gray-900">Create new album</span>
     </button>

     {/* Create form */}
     {showCreate && (
      <div className="mb-4 p-4 bg-gray-50 rounded-lg">
       <input
        type="text"
        value={newAlbumName}
        onChange={(e) => setNewAlbumName(e.target.value)}
        placeholder="Album name"
        className="input mb-3"
        autoFocus
       />
       <button
        onClick={handleCreateAndAdd}
        disabled={isSubmitting || !newAlbumName.trim()}
        className="btn-primary w-full"
       >
        {isSubmitting ? 'Creating...' : 'Create & Add Photos'}
       </button>
      </div>
     )}

     {/* Existing albums */}
     {!showCreate && (
      <>
       {isLoading && (
        <div className="text-center py-8 text-gray-500">Loading albums...</div>
       )}

       {!isLoading && albums?.length === 0 && (
        <div className="text-center py-8 text-gray-500">
         <p>No albums yet</p>
         <p className="text-sm">Create one above</p>
        </div>
       )}

       {!isLoading && albums && albums.length > 0 && (
        <div className="space-y-2">
         {albums.map((album) => (
          <button
           key={album.id}
           onClick={() => setSelectedAlbumId(album.id)}
           className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg border transition-colors ${
            selectedAlbumId === album.id
             ? 'border-primary-500 bg-primary-50'
             : 'border-gray-200 hover:border-gray-300'
           }`}
          >
           <div className="w-10 h-10 rounded-lg bg-gray-100 overflow-hidden flex-shrink-0">
            {album.coverPhotoId ? (
             <AuthenticatedImage
              src={getThumbnailUrl(album.coverPhotoId)}
              alt=""
              className="w-full h-full object-cover"
             />
            ) : (
             <div className="w-full h-full flex items-center justify-center text-gray-400">
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
               <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
               />
              </svg>
             </div>
            )}
           </div>
           <div className="flex-1 text-left">
            <p className="font-medium text-gray-900">{album.name}</p>
            <p className="text-sm text-gray-500">{album.photoCount} photos</p>
           </div>
           {selectedAlbumId === album.id && (
            <svg className="w-5 h-5 text-primary-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
             <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
           )}
          </button>
         ))}
        </div>
       )}
      </>
     )}
    </div>

    {/* Footer */}
    {!showCreate && selectedAlbumId && (
     <div className="px-6 py-4 border-t border-gray-200 flex-shrink-0">
      <button
       onClick={handleAddToExisting}
       disabled={isSubmitting}
       className="btn-primary w-full"
      >
       {isSubmitting ? 'Adding...' : 'Add to Album'}
      </button>
     </div>
    )}
   </div>
  </div>
 )
}
