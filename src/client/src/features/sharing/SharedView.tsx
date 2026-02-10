import { useState, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { useSharedMeta, useAccessSharedContent } from '../../hooks/useSharedContent'
import { SharedPhotoGrid } from './SharedPhotoGrid'
import { SharedLightbox } from './SharedLightbox'
import type { SharedContent, SharedPhoto } from '../../types'

interface ApiError {
  response?: {
    status: number
  }
}

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
    } catch (err) {
      const error = err as ApiError
      if (error.response?.status === 401) {
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
      <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50/20 to-purple-50/10 dark:from-dark-bg-primary dark:via-[#0d1220] dark:to-black flex items-center justify-center">
        <div className="flex flex-col items-center gap-4 text-gray-600 dark:text-dark-text-secondary">
          <div className="w-12 h-12 border-3 border-gray-300 border-t-primary-600 dark:border-t-primary-500 rounded-full animate-spin" />
          <span className="text-lg font-medium">Loading...</span>
        </div>
      </div>
    )
  }

  // Not found
  if (metaError || !meta) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50/20 to-purple-50/10 dark:from-dark-bg-primary dark:via-[#0d1220] dark:to-black flex items-center justify-center">
        <div className="text-center">
          <div className="w-24 h-24 mx-auto mb-6 rounded-3xl bg-gradient-to-br from-gray-100 to-gray-200 dark:from-dark-bg-tertiary dark:to-dark-border-default flex items-center justify-center shadow-lg">
            <svg className="w-12 h-12 text-gray-400 dark:text-dark-text-secondary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
                d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1"
              />
            </svg>
          </div>
          <h2 className="text-2xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 dark:from-white dark:to-gray-300 bg-clip-text text-transparent mb-2">Link not found</h2>
          <p className="text-gray-600 dark:text-dark-text-secondary text-lg">This share link doesn't exist or has been removed.</p>
        </div>
      </div>
    )
  }

  // Expired or revoked
  if (!meta.isActive) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50/20 to-purple-50/10 dark:from-dark-bg-primary dark:via-[#0d1220] dark:to-black flex items-center justify-center">
        <div className="text-center">
          <div className="w-24 h-24 mx-auto mb-6 rounded-3xl bg-gradient-to-br from-gray-100 to-gray-200 dark:from-dark-bg-tertiary dark:to-dark-border-default flex items-center justify-center shadow-lg">
            <svg className="w-12 h-12 text-gray-400 dark:text-dark-text-secondary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
                d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
          </div>
          <h2 className="text-2xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 dark:from-white dark:to-gray-300 bg-clip-text text-transparent mb-2">
            {meta.isExpired ? 'Link expired' : 'Link revoked'}
          </h2>
          <p className="text-gray-600 dark:text-dark-text-secondary text-lg">This share link is no longer active.</p>
        </div>
      </div>
    )
  }

  // Password prompt
  if (meta.hasPassword && !content) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50/20 to-purple-50/10 dark:from-dark-bg-primary dark:via-[#0d1220] dark:to-black flex items-center justify-center relative overflow-hidden">
        {/* Animated background elements */}
        <div className="absolute inset-0 overflow-hidden pointer-events-none">
          <div className="absolute -top-1/2 -right-1/2 w-full h-full bg-gradient-to-br from-primary-100/20 to-transparent rounded-full blur-3xl animate-pulse" />
          <div className="absolute -bottom-1/2 -left-1/2 w-full h-full bg-gradient-to-tr from-primary-200/10 to-transparent rounded-full blur-3xl animate-pulse" style={{ animationDelay: '1s' }} />
        </div>

        <div className="relative glass-strong rounded-3xl p-10 w-full max-w-sm mx-4 shadow-2xl dark:shadow-primary-600/20 animate-fade-in-up">
          <div className="text-center mb-8">
            <div className="w-16 h-16 mx-auto mb-4 rounded-2xl bg-gradient-to-br from-primary-500 to-purple-600 dark:from-primary-600 dark:to-purple-700 flex items-center justify-center shadow-xl dark:shadow-primary-600/30 float">
              <svg className="w-8 h-8 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                  d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
                />
              </svg>
            </div>
            <h2 className="text-2xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 dark:from-white dark:to-gray-300 bg-clip-text text-transparent mb-2">Password Protected</h2>
            <p className="text-sm text-gray-600 dark:text-dark-text-secondary">Enter the password to view this content.</p>
          </div>

          <form onSubmit={handlePasswordSubmit} className="space-y-4">
            <div className="space-y-2">
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Enter password"
                className="input"
                autoFocus
              />
            </div>
            {passwordError && (
              <div className="flex items-center gap-2 px-3 py-2 text-sm font-medium text-red-700 dark:text-red-400 bg-red-50/90 dark:bg-red-900/20 backdrop-blur-sm rounded-lg border border-red-200 dark:border-red-800/50">
                <svg className="w-4 h-4 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                    d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
                {passwordError}
              </div>
            )}
            <button
              type="submit"
              disabled={!password || accessContent.isPending}
              className="w-full btn-primary-lg py-4 shadow-xl dark:shadow-primary-600/30"
            >
              {accessContent.isPending ? (
                <span className="flex items-center justify-center gap-2">
                  <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  Verifying...
                </span>
              ) : (
                'View Content'
              )}
            </button>
          </form>
        </div>
      </div>
    )
  }

  // Loading content
  if (accessContent.isPending && !content) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50/20 to-purple-50/10 dark:from-dark-bg-primary dark:via-[#0d1220] dark:to-black flex items-center justify-center">
        <div className="flex flex-col items-center gap-4 text-gray-600 dark:text-dark-text-secondary">
          <div className="w-12 h-12 border-3 border-gray-300 border-t-primary-600 dark:border-t-primary-500 rounded-full animate-spin" />
          <span className="text-lg font-medium">Loading content...</span>
        </div>
      </div>
    )
  }

  // Display content
  if (!content) return null

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50/20 to-purple-50/10 dark:from-dark-bg-primary dark:via-[#0d1220] dark:to-black">
      {/* Header */}
      <header className="glass-strong border-b border-gray-200/50 dark:border-white/10 px-6 py-4 sticky top-0 z-30">
        <div className="max-w-6xl mx-auto flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary-600 to-purple-600 dark:from-primary-600 dark:to-purple-700 flex items-center justify-center shadow-lg dark:shadow-primary-600/20">
            <svg className="w-5 h-5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z"
              />
            </svg>
          </div>
          <span className="font-semibold bg-gradient-to-r from-gray-900 to-gray-700 dark:from-white dark:to-gray-300 bg-clip-text text-transparent text-lg">MyPhotoBooth</span>
          <span className="text-gray-400">|</span>
          <span className="text-gray-600 dark:text-dark-text-secondary">
            {content.type === 'Photo' ? 'Shared Photo' : content.album?.name || 'Shared Album'}
          </span>
        </div>
      </header>

      {/* Content */}
      <main className="max-w-[1800px] mx-auto p-6">
        {content.type === 'Album' && content.album && (
          <div className="mb-8">
            <h1 className="text-3xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 dark:from-white dark:to-gray-300 bg-clip-text text-transparent mb-2">{content.album.name}</h1>
            {content.album.description && (
              <p className="text-gray-600 dark:text-dark-text-secondary text-lg">{content.album.description}</p>
            )}
            <p className="text-sm text-gray-500 dark:text-dark-text-secondary mt-3">
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
