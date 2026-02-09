import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../lib/api'

export interface Tag {
  id: string
  name: string
  createdAt: string
}

// List all tags
export function useTags() {
  return useQuery({
    queryKey: ['tags'],
    queryFn: async () => {
      const { data } = await api.get<Tag[]>('/tags')
      return data
    },
  })
}

// Search tags
export function useSearchTags(query: string) {
  return useQuery({
    queryKey: ['tags', 'search', query],
    queryFn: async () => {
      const { data } = await api.get<Tag[]>('/tags/search', { params: { query } })
      return data
    },
    enabled: query.length > 0,
  })
}

// Create tag
export function useCreateTag() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (name: string) => {
      const { data } = await api.post<Tag>('/tags', { name })
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tags'] })
    },
  })
}

// Delete tag
export function useDeleteTag() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/tags/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tags'] })
    },
  })
}
