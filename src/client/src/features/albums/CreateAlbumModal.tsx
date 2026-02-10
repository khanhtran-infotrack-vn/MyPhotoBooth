import { useState, useEffect } from 'react'
import { useCreateAlbum } from '../../hooks/useAlbums'

interface CreateAlbumModalProps {
 onClose: () => void
 onCreated?: (albumId: string) => void
}

export function CreateAlbumModal({ onClose, onCreated }: CreateAlbumModalProps) {
 const [name, setName] = useState('')
 const [description, setDescription] = useState('')
 const [error, setError] = useState('')
 const createAlbum = useCreateAlbum()

 // Close on Escape
 useEffect(() => {
  const handleKeyDown = (e: KeyboardEvent) => {
   if (e.key === 'Escape') onClose()
  }
  document.addEventListener('keydown', handleKeyDown)
  return () => document.removeEventListener('keydown', handleKeyDown)
 }, [onClose])

 const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault()
  setError('')

  if (!name.trim()) {
   setError('Album name is required')
   return
  }

  try {
   const album = await createAlbum.mutateAsync({
    name: name.trim(),
    description: description.trim() || undefined,
   })
   onCreated?.(album.id)
   onClose()
  } catch (err) {
   setError('Failed to create album')
  }
 }

 return (
  <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
   {/* Backdrop */}
   <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={onClose} />

   {/* Modal */}
   <div className="relative w-full max-w-md bg-white rounded-2xl shadow-2xl animate-scale-in">
    {/* Header */}
    <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
     <h2 className="text-lg font-semibold text-gray-900">Create Album</h2>
     <button onClick={onClose} className="btn-icon text-gray-400 hover:text-gray-600">
      <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
      </svg>
     </button>
    </div>

    {/* Form */}
    <form onSubmit={handleSubmit} className="p-6 space-y-4">
     {error && (
      <div className="px-4 py-3 text-sm font-medium text-red-700 bg-red-50 rounded-lg">
       {error}
      </div>
     )}

     <div>
      <label htmlFor="name" className="block text-sm font-semibold text-gray-900 mb-2">
       Album name
      </label>
      <input
       id="name"
       type="text"
       value={name}
       onChange={(e) => setName(e.target.value)}
       placeholder="My Album"
       className="input"
       autoFocus
      />
     </div>

     <div>
      <label htmlFor="description" className="block text-sm font-semibold text-gray-900 mb-2">
       Description (optional)
      </label>
      <textarea
       id="description"
       value={description}
       onChange={(e) => setDescription(e.target.value)}
       placeholder="Add a description..."
       rows={3}
       className="input resize-none"
      />
     </div>

     <div className="flex justify-end gap-3 pt-2">
      <button type="button" onClick={onClose} className="btn-secondary">
       Cancel
      </button>
      <button
       type="submit"
       disabled={createAlbum.isPending}
       className="btn-primary"
      >
       {createAlbum.isPending ? (
        <span className="flex items-center gap-2">
         <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
         Creating...
        </span>
       ) : (
        'Create Album'
       )}
      </button>
     </div>
    </form>
   </div>
  </div>
 )
}
