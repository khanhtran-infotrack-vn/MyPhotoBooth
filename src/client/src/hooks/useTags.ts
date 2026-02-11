import { useQuery, useMutation, useQueryClient, useInfiniteQuery } from '@tanstack/react-query'
import api from '../lib/api'
import type { Photo } from '../types'

export interface Tag {
  id: string
  name: string
  createdAt: string
  photoCount?: number
}

export interface TagPhotosResponse {
  items: Photo[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
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

// List all tags with photo count
export function useTagsWithCount() {
  return useQuery({
    queryKey: ['tags', 'with-count'],
    queryFn: async () => {
      const { data } = await api.get<Tag[]>('/tags/with-count')
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

// Get photos by tag (paginated)
export function useTagPhotos(tagId: string, pageSize = 50) {
  return useInfiniteQuery({
    queryKey: ['tag', tagId, 'photos'],
    queryFn: async ({ pageParam = 1 }) => {
      const { data } = await api.get<TagPhotosResponse>(`/tags/${tagId}/photos`, {
        params: { page: pageParam, pageSize }
      })
      return data
    },
    getNextPageParam: (lastPage) => {
      if (lastPage.page < lastPage.totalPages) {
        return lastPage.page + 1
      }
      return undefined
    },
    initialPageParam: 1,
  })
}

// Remove single tag from photo
export function useRemoveTagFromPhoto() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ photoId, tagId }: { photoId: string; tagId: string }) => {
      await api.delete(`/photos/${photoId}/tags/${tagId}`)
    },
    onSuccess: (_, { photoId }) => {
      queryClient.invalidateQueries({ queryKey: ['photo', photoId] })
      queryClient.invalidateQueries({ queryKey: ['photos'] })
      queryClient.invalidateQueries({ queryKey: ['tags', 'with-count'] })
    },
  })
}

// Add tags to photo
export function useAddTagsToPhoto() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ photoId, tagIds }: { photoId: string; tagIds: string[] }) => {
      await api.post(`/photos/${photoId}/tags`, { tagIds })
    },
    onSuccess: (_, { photoId }) => {
      queryClient.invalidateQueries({ queryKey: ['photo', photoId] })
      queryClient.invalidateQueries({ queryKey: ['photos'] })
      queryClient.invalidateQueries({ queryKey: ['tags', 'with-count'] })
    },
  })
}

// Helper function to flatten paginated tag photos
export function getAllTagPhotosFromPages(pages?: TagPhotosResponse[]): Photo[] {
  if (!pages) return []
  return pages.flatMap(page => page.items)
}

// Update photo description
export function useUpdatePhotoDescription() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ photoId, description }: { photoId: string; description: string | null }) => {
      const { data } = await api.put(`/photos/${photoId}`, { description: description === '' ? null : description })
      return data
    },
    onSuccess: (_, { photoId }) => {
      queryClient.invalidateQueries({ queryKey: ['photo', photoId] })
      queryClient.invalidateQueries({ queryKey: ['photos'] })
    },
  })
}
