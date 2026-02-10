import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../lib/api'
import type { ShareLink } from '../types'

// List all user's share links
export function useShareLinks() {
  return useQuery({
    queryKey: ['shareLinks'],
    queryFn: async () => {
      const { data } = await api.get<ShareLink[]>('/sharelinks')
      return data
    },
  })
}

// Create a share link
export function useCreateShareLink() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (params: {
      type: number
      photoId?: string
      albumId?: string
      password?: string
      expiresAt?: string
      allowDownload?: boolean
    }) => {
      const { data } = await api.post<ShareLink>('/sharelinks', params)
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['shareLinks'] })
    },
  })
}

// Revoke a share link
export function useRevokeShareLink() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/sharelinks/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['shareLinks'] })
    },
  })
}
