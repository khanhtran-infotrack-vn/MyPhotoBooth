import { useQuery, useMutation } from '@tanstack/react-query'
import { publicApi } from '../lib/api'
import type { ShareMetadata, SharedContent } from '../types'

// Fetch share link metadata (public, no auth)
export function useSharedMeta(token: string | undefined) {
  return useQuery({
    queryKey: ['shared', token],
    queryFn: async () => {
      const { data } = await publicApi.get<ShareMetadata>(`/shared/${token}`)
      return data
    },
    enabled: !!token,
    retry: false,
  })
}

// Access shared content (optionally with password)
export function useAccessSharedContent() {
  return useMutation({
    mutationFn: async ({ token, password }: { token: string; password?: string }) => {
      const { data } = await publicApi.post<SharedContent>(`/shared/${token}/access`, {
        password: password || null,
      })
      return data
    },
  })
}

// Helper to get shared thumbnail URL
export function getSharedThumbnailUrl(token: string, photoId: string): string {
  const baseUrl = import.meta.env.VITE_API_BASE_URL || '/api'
  return `${baseUrl}/shared/${token}/photos/${photoId}/thumbnail`
}

// Helper to get shared file URL
export function getSharedFileUrl(token: string, photoId: string): string {
  const baseUrl = import.meta.env.VITE_API_BASE_URL || '/api'
  return `${baseUrl}/shared/${token}/photos/${photoId}/file`
}
