import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useAlbums, useDeleteAlbum, type Album } from '../../hooks/useAlbums'
import { getThumbnailUrl } from '../../hooks/usePhotos'
import { AuthenticatedImage } from '../../components/photos'
import { CreateAlbumModal } from './CreateAlbumModal'

export default function AlbumList() {
  const [showCreateModal, setShowCreateModal] = useState(false)
  const { data: albums, isLoading } = useAlbums()
  const deleteAlbum = useDeleteAlbum()

  const handleDelete = async (album: Album) => {
    if (!confirm(`Delete album "${album.name}"? Photos will not be deleted.`)) return
    await deleteAlbum.mutateAsync(album.id)
  }

  return (
    <div className="p-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Albums</h1>
          <p className="text-sm text-gray-500 mt-1">
            {albums?.length || 0} {albums?.length === 1 ? 'album' : 'albums'}
          </p>
        </div>
        <button onClick={() => setShowCreateModal(true)} className="btn-primary">
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          New Album
        </button>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex items-center justify-center py-20">
          <div className="flex items-center gap-3 text-gray-500">
            <div className="w-6 h-6 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
            <span>Loading albums...</span>
          </div>
        </div>
      )}

      {/* Empty state */}
      {!isLoading && albums?.length === 0 && (
        <div className="flex flex-col items-center justify-center py-20 text-gray-500">
          <svg className="w-20 h-20 mb-4 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
              d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"
            />
          </svg>
          <h3 className="text-xl font-semibold text-gray-700">No albums yet</h3>
          <p className="mt-2">Create your first album to organize your photos</p>
          <button onClick={() => setShowCreateModal(true)} className="btn-primary mt-4">
            Create Album
          </button>
        </div>
      )}

      {/* Album grid */}
      {!isLoading && albums && albums.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {albums.map((album) => (
            <AlbumCard key={album.id} album={album} onDelete={() => handleDelete(album)} />
          ))}
        </div>
      )}

      {/* Create modal */}
      {showCreateModal && <CreateAlbumModal onClose={() => setShowCreateModal(false)} />}
    </div>
  )
}

function AlbumCard({ album, onDelete }: { album: Album; onDelete: () => void }) {
  const [menuOpen, setMenuOpen] = useState(false)

  return (
    <div className="group relative bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
      <Link to={`/albums/${album.id}`}>
        {/* Cover image */}
        <div className="aspect-[4/3] bg-gray-100 relative">
          {album.coverPhotoId ? (
            <AuthenticatedImage
              src={getThumbnailUrl(album.coverPhotoId)}
              alt={album.name}
              className="w-full h-full object-cover"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center text-gray-300">
              <svg className="w-16 h-16" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
                  d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
            </div>
          )}
        </div>

        {/* Info */}
        <div className="p-4">
          <h3 className="font-semibold text-gray-900 truncate">{album.name}</h3>
          <p className="text-sm text-gray-500 mt-1">
            {album.photoCount} {album.photoCount === 1 ? 'photo' : 'photos'}
          </p>
        </div>
      </Link>

      {/* Menu button */}
      <div className="absolute top-2 right-2">
        <button
          onClick={(e) => {
            e.preventDefault()
            setMenuOpen(!menuOpen)
          }}
          className="p-2 bg-black/40 hover:bg-black/60 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
        >
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
              d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"
            />
          </svg>
        </button>

        {menuOpen && (
          <>
            <div className="fixed inset-0 z-10" onClick={() => setMenuOpen(false)} />
            <div className="absolute right-0 mt-1 w-40 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-20 animate-scale-in">
              <button
                onClick={(e) => {
                  e.preventDefault()
                  setMenuOpen(false)
                  onDelete()
                }}
                className="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-red-50"
              >
                Delete album
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  )
}
