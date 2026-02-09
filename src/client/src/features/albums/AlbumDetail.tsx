import { useState, useMemo } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { useAlbum, useDeleteAlbum } from '../../hooks/useAlbums'
import { PhotoGrid } from '../../components/photos'
import { Lightbox } from '../../components/lightbox'
import type { Photo } from '../../types'

export default function AlbumDetail() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [lightboxOpen, setLightboxOpen] = useState(false)
  const [lightboxIndex, setLightboxIndex] = useState(0)

  const { data: album, isLoading } = useAlbum(id ?? null)
  const deleteAlbum = useDeleteAlbum()

  const photos = useMemo(() => album?.photos || [], [album?.photos])

  const handlePhotoClick = (_photo: Photo, index: number) => {
    setLightboxIndex(index)
    setLightboxOpen(true)
  }

  const handleDeleteAlbum = async () => {
    if (!album) return
    if (!confirm(`Delete album "${album.name}"? Photos will not be deleted.`)) return

    await deleteAlbum.mutateAsync(album.id)
    navigate('/albums')
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="flex items-center gap-3 text-gray-500">
          <div className="w-6 h-6 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
          <span>Loading album...</span>
        </div>
      </div>
    )
  }

  if (!album) {
    return (
      <div className="flex flex-col items-center justify-center h-full text-gray-500">
        <h2 className="text-xl font-semibold">Album not found</h2>
        <Link to="/albums" className="text-primary-600 mt-2 hover:underline">
          Back to albums
        </Link>
      </div>
    )
  }

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center gap-2 text-sm text-gray-500 mb-2">
          <Link to="/albums" className="hover:text-primary-600">Albums</Link>
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
          </svg>
          <span className="text-gray-700">{album.name}</span>
        </div>

        <div className="flex items-start justify-between">
          <div>
            <h1 className="text-2xl font-semibold text-gray-900">{album.name}</h1>
            {album.description && (
              <p className="text-gray-500 mt-1">{album.description}</p>
            )}
            <p className="text-sm text-gray-400 mt-2">
              {photos.length} {photos.length === 1 ? 'photo' : 'photos'}
            </p>
          </div>

          <button
            onClick={handleDeleteAlbum}
            className="btn-ghost text-red-600 hover:bg-red-50"
          >
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
              />
            </svg>
            Delete Album
          </button>
        </div>
      </div>

      {/* Photos */}
      {photos.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 text-gray-500">
          <svg className="w-20 h-20 mb-4 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
              d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
            />
          </svg>
          <h3 className="text-xl font-semibold text-gray-700">No photos in this album</h3>
          <p className="mt-2">Add photos from your gallery</p>
        </div>
      ) : (
        <PhotoGrid
          photos={photos}
          onPhotoClick={handlePhotoClick}
        />
      )}

      {/* Lightbox */}
      {lightboxOpen && photos.length > 0 && (
        <Lightbox
          photos={photos}
          initialIndex={lightboxIndex}
          onClose={() => setLightboxOpen(false)}
        />
      )}
    </div>
  )
}
