import { useState, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { useSharedMeta, useAccessSharedContent } from '../../hooks/useSharedContent'
import { SharedPhotoGrid } from './SharedPhotoGrid'
import { SharedLightbox } from './SharedLightbox'
import type { SharedContent, SharedPhoto } from '../../types'

export function SharedView() {
  const { token } = useParams<{ token: string }>()
  const { data: meta, isLoading: metaLoading, error: metaError } = useSharedMeta(token)
  const accessContent = useAccessSharedContent()

  const [password, setPassword] = useState('')
  const [content, setContent] = useState<SharedContent | null>(null)
  const [passwordError, setPasswordError] = useState('')
  const [lightboxOpen, setLightboxOpen] = useState(false)
  const [lightboxIndex, setLightboxIndex] = useState(0)

  // Auto-access if no password required
  useEffect(() => {
    if (meta && meta.isActive && !meta.hasPassword && !content) {
      handleAccess()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [meta])

  const handleAccess = async (pw?: string) => {
    if (!token) return
    setPasswordError('')
    try {
      const result = await accessContent.mutateAsync({
        token,
        password: pw || password || undefined,
      })
      setContent(result)
    } catch (err: any) {
      if (err.response?.status === 401) {
        setPasswordError('Incorrect password')
      }
    }
  }

  const handlePasswordSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    handleAccess()
  }

  // Get all photos for lightbox
  const photos: SharedPhoto[] = content?.type === 'Photo' && content.photo
    ? [content.photo]
    : content?.album?.photos || []

  const handlePhotoClick = (_photo: SharedPhoto, index: number) => {
    setLightboxIndex(index)
    setLightboxOpen(true)
  }

  // Loading state
  if (metaLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="flex items-center gap-3 text-gray-500">
          <div className="w-6 h-6 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
          <span>Loading...</span>
        </div>
      </div>
    )
  }

  // Not found
  if (metaError || !meta) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <svg className="w-16 h-16 mx-auto text-gray-300 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
              d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1"
            />
          </svg>
          <h2 className="text-xl font-semibold text-gray-700">Link not found</h2>
          <p className="text-gray-500 mt-2">This share link doesn't exist or has been removed.</p>
        </div>
      </div>
    )
  }

  // Expired or revoked
  if (!meta.isActive) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <svg className="w-16 h-16 mx-auto text-gray-300 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
              d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <h2 className="text-xl font-semibold text-gray-700">
            {meta.isExpired ? 'Link expired' : 'Link revoked'}
          </h2>
          <p className="text-gray-500 mt-2">This share link is no longer active.</p>
        </div>
      </div>
    )
  }

  // Password prompt
  if (meta.hasPassword && !content) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="bg-white rounded-xl shadow-lg p-8 w-full max-w-sm mx-4">
          <div className="text-center mb-6">
            <svg className="w-12 h-12 mx-auto text-primary-600 mb-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
              />
            </svg>
            <h2 className="text-lg font-semibold text-gray-900">Password Protected</h2>
            <p className="text-sm text-gray-500 mt-1">Enter the password to view this content.</p>
          </div>

          <form onSubmit={handlePasswordSubmit}>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter password"
              className="w-full px-4 py-2 border rounded-lg mb-3 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              autoFocus
            />
            {passwordError && (
              <p className="text-sm text-red-600 mb-3">{passwordError}</p>
            )}
            <button
              type="submit"
              disabled={!password || accessContent.isPending}
              className="w-full btn-primary"
            >
              {accessContent.isPending ? 'Verifying...' : 'View Content'}
            </button>
          </form>
        </div>
      </div>
    )
  }

  // Loading content
  if (accessContent.isPending && !content) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="flex items-center gap-3 text-gray-500">
          <div className="w-6 h-6 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
          <span>Loading content...</span>
        </div>
      </div>
    )
  }

  // Display content
  if (!content) return null

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="max-w-6xl mx-auto flex items-center gap-3">
          <div className="w-8 h-8 rounded-full bg-primary-600 flex items-center justify-center">
            <svg className="w-4 h-4 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z"
              />
            </svg>
          </div>
          <span className="font-semibold text-gray-900">MyPhotoBooth</span>
          <span className="text-gray-400">|</span>
          <span className="text-gray-600">
            {content.type === 'Photo' ? 'Shared Photo' : content.album?.name || 'Shared Album'}
          </span>
        </div>
      </header>

      {/* Content */}
      <main className="max-w-6xl mx-auto p-6">
        {content.type === 'Album' && content.album && (
          <div className="mb-6">
            <h1 className="text-2xl font-semibold text-gray-900">{content.album.name}</h1>
            {content.album.description && (
              <p className="text-gray-500 mt-1">{content.album.description}</p>
            )}
            <p className="text-sm text-gray-400 mt-2">
              {photos.length} {photos.length === 1 ? 'photo' : 'photos'}
            </p>
          </div>
        )}

        <SharedPhotoGrid
          token={token!}
          photos={photos}
          onPhotoClick={handlePhotoClick}
        />
      </main>

      {/* Lightbox */}
      {lightboxOpen && photos.length > 0 && (
        <SharedLightbox
          token={token!}
          photos={photos}
          initialIndex={lightboxIndex}
          onClose={() => setLightboxOpen(false)}
        />
      )}
    </div>
  )
}
