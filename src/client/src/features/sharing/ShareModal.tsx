import { useState, useEffect } from 'react'
import { useCreateShareLink, useShareLinks, useRevokeShareLink } from '../../hooks/useShareLinks'
import type { ShareLink } from '../../types'

interface ShareModalProps {
  type: 'photo' | 'album'
  targetId: string
  targetName: string
  onClose: () => void
}

export function ShareModal({ type, targetId, targetName, onClose }: ShareModalProps) {
  const [password, setPassword] = useState('')
  const [usePassword, setUsePassword] = useState(false)
  const [expiresAt, setExpiresAt] = useState('')
  const [allowDownload, setAllowDownload] = useState(true)
  const [copiedId, setCopiedId] = useState<string | null>(null)
  const [createdLink, setCreatedLink] = useState<ShareLink | null>(null)

  const createShareLink = useCreateShareLink()
  const revokeShareLink = useRevokeShareLink()
  const { data: allLinks } = useShareLinks()

  // Filter existing active links for this target
  const existingLinks = (allLinks || []).filter((link) => {
    if (type === 'photo') return link.photoId === targetId && link.isActive
    return link.albumId === targetId && link.isActive
  })

  const handleCreate = async () => {
    const result = await createShareLink.mutateAsync({
      type: type === 'photo' ? 0 : 1,
      photoId: type === 'photo' ? targetId : undefined,
      albumId: type === 'album' ? targetId : undefined,
      password: usePassword ? password : undefined,
      expiresAt: expiresAt || undefined,
      allowDownload,
    })
    setCreatedLink(result)
  }

  const handleCopy = async (url: string, id: string) => {
    await navigator.clipboard.writeText(url)
    setCopiedId(id)
    setTimeout(() => setCopiedId(null), 2000)
  }

  const handleRevoke = async (id: string) => {
    if (!confirm('Revoke this share link? Anyone with the link will no longer be able to access it.')) return
    await revokeShareLink.mutateAsync(id)
    if (createdLink?.id === id) setCreatedLink(null)
  }

  // Close on escape
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [onClose])

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50" onClick={onClose}>
      <div
        className="bg-white rounded-xl shadow-2xl w-full max-w-md mx-4 max-h-[90vh] overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b">
          <h2 className="text-lg font-semibold text-gray-900">
            Share {type === 'photo' ? 'Photo' : 'Album'}
          </h2>
          <button onClick={onClose} className="p-1 text-gray-400 hover:text-gray-600 rounded">
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <div className="p-4 space-y-4">
          {/* Newly created link */}
          {createdLink && (
            <div className="p-3 bg-green-50 border border-green-200 rounded-lg">
              <p className="text-sm font-medium text-green-800 mb-2">Link created!</p>
              <div className="flex items-center gap-2">
                <input
                  type="text"
                  value={createdLink.shareUrl}
                  readOnly
                  className="flex-1 text-sm bg-white border rounded px-2 py-1"
                />
                <button
                  onClick={() => handleCopy(createdLink.shareUrl, createdLink.id)}
                  className="px-3 py-1 text-sm bg-green-600 text-white rounded hover:bg-green-700"
                >
                  {copiedId === createdLink.id ? 'Copied!' : 'Copy'}
                </button>
              </div>
            </div>
          )}

          {/* Create new link form */}
          {!createdLink && (
            <>
              <p className="text-sm text-gray-500">
                Create a public link to share "{targetName}" with anyone.
              </p>

              {/* Password protection */}
              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={usePassword}
                  onChange={(e) => setUsePassword(e.target.checked)}
                  className="w-4 h-4 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span className="text-sm text-gray-700">Require password</span>
              </label>

              {usePassword && (
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Enter password"
                  className="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                />
              )}

              {/* Expiration */}
              <div>
                <label className="block text-sm text-gray-700 mb-1">Expires at (optional)</label>
                <input
                  type="datetime-local"
                  value={expiresAt}
                  onChange={(e) => setExpiresAt(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                />
              </div>

              {/* Allow download */}
              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={allowDownload}
                  onChange={(e) => setAllowDownload(e.target.checked)}
                  className="w-4 h-4 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span className="text-sm text-gray-700">Allow downloads</span>
              </label>

              <button
                onClick={handleCreate}
                disabled={createShareLink.isPending || (usePassword && !password)}
                className="w-full btn-primary"
              >
                {createShareLink.isPending ? 'Creating...' : 'Create Share Link'}
              </button>
            </>
          )}

          {/* Existing links */}
          {existingLinks.length > 0 && (
            <div>
              <h3 className="text-sm font-medium text-gray-700 mb-2">
                Active share links ({existingLinks.length})
              </h3>
              <div className="space-y-2">
                {existingLinks.map((link) => (
                  <div
                    key={link.id}
                    className="flex items-center gap-2 p-2 bg-gray-50 rounded-lg text-sm"
                  >
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-1 text-gray-500">
                        {link.hasPassword && (
                          <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                              d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
                            />
                          </svg>
                        )}
                        {link.expiresAt && (
                          <span className="text-xs">
                            Expires {new Date(link.expiresAt).toLocaleDateString()}
                          </span>
                        )}
                        {!link.expiresAt && <span className="text-xs">No expiration</span>}
                      </div>
                    </div>
                    <button
                      onClick={() => handleCopy(link.shareUrl, link.id)}
                      className="px-2 py-1 text-xs bg-gray-200 hover:bg-gray-300 rounded"
                    >
                      {copiedId === link.id ? 'Copied!' : 'Copy'}
                    </button>
                    <button
                      onClick={() => handleRevoke(link.id)}
                      className="px-2 py-1 text-xs text-red-600 hover:bg-red-50 rounded"
                    >
                      Revoke
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
