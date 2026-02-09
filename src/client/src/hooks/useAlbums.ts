import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../lib/api'
import type { Photo } from '../types'

export interface Album {
  id: string
  name: string
  description: string | null
  coverPhotoId: string | null
  createdAt: string
  updatedAt: string
  photoCount: number
}

export interface AlbumDetails extends Omit<Album, 'photoCount'> {
  photos: Photo[]
}

// List all albums
export function useAlbums() {
  return useQuery({
    queryKey: ['albums'],
    queryFn: async () => {
      const { data } = await api.get<Album[]>('/albums')
      return data
    },
  })
}

// Get single album with photos
export function useAlbum(id: string | null) {
  return useQuery({
    queryKey: ['album', id],
    queryFn: async () => {
      const { data } = await api.get<AlbumDetails>(`/albums/${id}`)
      return data
    },
    enabled: !!id,
  })
}

// Create album
export function useCreateAlbum() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (album: { name: string; description?: string }) => {
      const { data } = await api.post<Album>('/albums', album)
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['albums'] })
    },
  })
}

// Update album
export function useUpdateAlbum() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({
      id,
      ...album
    }: {
      id: string
      name: string
      description?: string
      coverPhotoId?: string
    }) => {
      await api.put(`/albums/${id}`, album)
    },
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['album', id] })
      queryClient.invalidateQueries({ queryKey: ['albums'] })
    },
  })
}

// Delete album
export function useDeleteAlbum() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/albums/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['albums'] })
    },
  })
}

// Add photo to album
export function useAddPhotoToAlbum() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ albumId, photoId }: { albumId: string; photoId: string }) => {
      await api.post(`/albums/${albumId}/photos`, { photoId })
    },
    onSuccess: (_, { albumId }) => {
      queryClient.invalidateQueries({ queryKey: ['album', albumId] })
      queryClient.invalidateQueries({ queryKey: ['albums'] })
    },
  })
}

// Add multiple photos to album
export function useAddPhotosToAlbum() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ albumId, photoIds }: { albumId: string; photoIds: string[] }) => {
      await Promise.all(
        photoIds.map((photoId) => api.post(`/albums/${albumId}/photos`, { photoId }))
      )
    },
    onSuccess: (_, { albumId }) => {
      queryClient.invalidateQueries({ queryKey: ['album', albumId] })
      queryClient.invalidateQueries({ queryKey: ['albums'] })
    },
  })
}

// Remove photo from album
export function useRemovePhotoFromAlbum() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ albumId, photoId }: { albumId: string; photoId: string }) => {
      await api.delete(`/albums/${albumId}/photos/${photoId}`)
    },
    onSuccess: (_, { albumId }) => {
      queryClient.invalidateQueries({ queryKey: ['album', albumId] })
      queryClient.invalidateQueries({ queryKey: ['albums'] })
    },
  })
}
