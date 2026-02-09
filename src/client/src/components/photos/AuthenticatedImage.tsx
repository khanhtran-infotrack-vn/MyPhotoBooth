import { useState, useEffect } from 'react'
import api from '../../lib/api'

interface AuthenticatedImageProps {
  src: string
  alt: string
  className?: string
  style?: React.CSSProperties
  loading?: 'lazy' | 'eager'
}

export function AuthenticatedImage({
  src,
  alt,
  className,
  style,
  loading = 'lazy',
}: AuthenticatedImageProps) {
  const [blobUrl, setBlobUrl] = useState<string | null>(null)
  const [error, setError] = useState(false)

  useEffect(() => {
    let isMounted = true
    let objectUrl: string | null = null

    const fetchImage = async () => {
      try {
        // Extract the path from the full URL (remove base URL if present)
        const path = src.replace(/^https?:\/\/[^/]+/, '').replace(/^\/api/, '')

        const response = await api.get(path, {
          responseType: 'blob',
        })

        if (isMounted) {
          objectUrl = URL.createObjectURL(response.data)
          setBlobUrl(objectUrl)
        }
      } catch (err) {
        if (isMounted) {
          setError(true)
        }
      }
    }

    fetchImage()

    return () => {
      isMounted = false
      if (objectUrl) {
        URL.revokeObjectURL(objectUrl)
      }
    }
  }, [src])

  if (error) {
    return (
      <div
        className={className}
        style={{
          ...style,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: '#f3f4f6',
        }}
      >
        <svg
          className="w-8 h-8 text-gray-400"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={1.5}
            d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
          />
        </svg>
      </div>
    )
  }

  if (!blobUrl) {
    return (
      <div
        className={className}
        style={{
          ...style,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: '#f3f4f6',
        }}
      >
        <div className="w-6 h-6 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
      </div>
    )
  }

  return (
    <img
      src={blobUrl}
      alt={alt}
      className={className}
      style={style}
      loading={loading}
    />
  )
}
