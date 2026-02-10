import { useState } from 'react'
import { useShareLinks, useRevokeShareLink } from '../../hooks/useShareLinks'

export default function ShareManagement() {
  const { data: links, isLoading } = useShareLinks()
  const revokeShareLink = useRevokeShareLink()
  const [copiedId, setCopiedId] = useState<string | null>(null)

  const handleCopy = async (url: string, id: string) => {
    await navigator.clipboard.writeText(url)
    setCopiedId(id)
    setTimeout(() => setCopiedId(null), 2000)
  }

  const handleRevoke = async (id: string) => {
    if (!confirm('Revoke this share link?')) return
    await revokeShareLink.mutateAsync(id)
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="flex items-center gap-3 text-gray-500">
          <div className="w-6 h-6 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
          <span>Loading share links...</span>
        </div>
      </div>
    )
  }

  const activeLinks = (links || []).filter((l) => l.isActive)
  const expiredLinks = (links || []).filter((l) => !l.isActive)

  return (
    <div className="p-6">
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Shared Links</h1>

      {(!links || links.length === 0) && (
        <div className="flex flex-col items-center justify-center py-20 text-gray-500">
          <svg className="w-20 h-20 mb-4 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
              d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1"
            />
          </svg>
          <h3 className="text-xl font-semibold text-gray-700">No shared links yet</h3>
          <p className="mt-2">Share photos or albums from the gallery to create links</p>
        </div>
      )}

      {/* Active links */}
      {activeLinks.length > 0 && (
        <div className="mb-8">
          <h2 className="text-lg font-medium text-gray-800 mb-3">
            Active ({activeLinks.length})
          </h2>
          <div className="space-y-3">
            {activeLinks.map((link) => (
              <ShareLinkCard
                key={link.id}
                link={link}
                copiedId={copiedId}
                onCopy={handleCopy}
                onRevoke={handleRevoke}
              />
            ))}
          </div>
        </div>
      )}

      {/* Expired/Revoked links */}
      {expiredLinks.length > 0 && (
        <div>
          <h2 className="text-lg font-medium text-gray-500 mb-3">
            Expired / Revoked ({expiredLinks.length})
          </h2>
          <div className="space-y-3 opacity-60">
            {expiredLinks.map((link) => (
              <ShareLinkCard
                key={link.id}
                link={link}
                copiedId={copiedId}
                onCopy={handleCopy}
                onRevoke={handleRevoke}
                disabled
              />
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

function ShareLinkCard({
  link,
  copiedId,
  onCopy,
  onRevoke,
  disabled,
}: {
  link: { id: string; type: string; targetName: string | null; hasPassword: boolean; expiresAt: string | null; allowDownload: boolean; shareUrl: string; isActive: boolean; createdAt: string }
  copiedId: string | null
  onCopy: (url: string, id: string) => void
  onRevoke: (id: string) => void
  disabled?: boolean
}) {
  return (
    <div className="flex items-center gap-4 p-4 bg-white border rounded-lg">
      {/* Icon */}
      <div className={`w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0 ${
        link.type === 'Photo' ? 'bg-blue-100 text-blue-600' : 'bg-purple-100 text-purple-600'
      }`}>
        {link.type === 'Photo' ? (
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
              d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
            />
          </svg>
        ) : (
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
              d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"
            />
          </svg>
        )}
      </div>

      {/* Info */}
      <div className="flex-1 min-w-0">
        <p className="font-medium text-gray-900 truncate">
          {link.targetName || 'Unknown'}
        </p>
        <div className="flex items-center gap-3 text-xs text-gray-500 mt-0.5">
          <span>{link.type}</span>
          {link.hasPassword && <span>Password protected</span>}
          {link.expiresAt && (
            <span>Expires {new Date(link.expiresAt).toLocaleDateString()}</span>
          )}
          {!link.allowDownload && <span>No download</span>}
          <span>Created {new Date(link.createdAt).toLocaleDateString()}</span>
        </div>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-2 flex-shrink-0">
        <button
          onClick={() => onCopy(link.shareUrl, link.id)}
          className="px-3 py-1.5 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
        >
          {copiedId === link.id ? 'Copied!' : 'Copy Link'}
        </button>
        {!disabled && link.isActive && (
          <button
            onClick={() => onRevoke(link.id)}
            className="px-3 py-1.5 text-sm text-red-600 hover:bg-red-50 rounded-lg transition-colors"
          >
            Revoke
          </button>
        )}
      </div>
    </div>
  )
}
