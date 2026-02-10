import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../lib/api'
import type { Photo, PhotoListResponse } from '../types'

export function useFavorites(pageSize = 50) {
  return useInfiniteQuery({
    queryKey: ['photos', 'favorites'],
    queryFn: async ({ pageParam = 1 }) => {
      const { data } = await api.get<PhotoListResponse>('/photos/favorites', {
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

export function useToggleFavorite() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (photoId: string) => {
      const { data } = await api.post<{ value: boolean }>(`/photos/${photoId}/favorite`)
      return data.value // boolean: true = added, false = removed
    },
    onSuccess: (_, photoId) => {
      // Invalidate all photo queries
      queryClient.invalidateQueries({ queryKey: ['photos'] })
      queryClient.invalidateQueries({ queryKey: ['photo', photoId] })
    },
  })
}

export function getAllFavoritesFromPages(
  pages: { items: Photo[] }[] | undefined
): Photo[] {
  if (!pages) return []
  return pages.flatMap((page) => page.items)
}
