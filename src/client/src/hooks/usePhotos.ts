import { useQuery, useMutation, useQueryClient, useInfiniteQuery } from '@tanstack/react-query'
import api from '../lib/api'
import type { Photo, PhotoDetails, PhotoListResponse, PhotoGroup } from '../types'

// Fetch paginated photos
export function usePhotos(pageSize = 50) {
  return useInfiniteQuery({
    queryKey: ['photos'],
    queryFn: async ({ pageParam = 1 }) => {
      const { data } = await api.get<PhotoListResponse>('/photos', {
        params: { page: pageParam, pageSize },
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

// Fetch single photo details
export function usePhotoDetails(id: string | null) {
  return useQuery({
    queryKey: ['photo', id],
    queryFn: async () => {
      const { data } = await api.get<PhotoDetails>(`/photos/${id}`)
      return data
    },
    enabled: !!id,
  })
}

// Delete photo mutation
export function useDeletePhoto() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/photos/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['photos'] })
    },
  })
}

// Delete multiple photos
export function useDeletePhotos() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (ids: string[]) => {
      await Promise.all(ids.map((id) => api.delete(`/photos/${id}`)))
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['photos'] })
    },
  })
}

// Update photo description
export function useUpdatePhoto() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ id, description }: { id: string; description: string }) => {
      await api.put(`/photos/${id}`, { description })
    },
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['photo', id] })
      queryClient.invalidateQueries({ queryKey: ['photos'] })
    },
  })
}

// Helper to get thumbnail URL
export function getThumbnailUrl(photoId: string): string {
  const baseUrl = import.meta.env.VITE_API_BASE_URL || '/api'
  return `${baseUrl}/photos/${photoId}/thumbnail`
}

// Helper to get full image URL
export function getPhotoUrl(photoId: string): string {
  const baseUrl = import.meta.env.VITE_API_BASE_URL || '/api'
  return `${baseUrl}/photos/${photoId}/file`
}

// Helper to group photos by date
export function groupPhotosByDate(photos: Photo[]): PhotoGroup[] {
  const groups = new Map<string, Photo[]>()

  photos.forEach((photo) => {
    const date = photo.capturedAt || photo.uploadedAt
    const dateKey = new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    })

    if (!groups.has(dateKey)) {
      groups.set(dateKey, [])
    }
    groups.get(dateKey)!.push(photo)
  })

  return Array.from(groups.entries()).map(([date, photos]) => ({
    date,
    photos,
  }))
}

// Get all photos from infinite query pages
export function getAllPhotosFromPages(
  pages: { items: Photo[] }[] | undefined
): Photo[] {
  if (!pages) return []
  return pages.flatMap((page) => page.items)
}
