import { useQuery } from '@tanstack/react-query'
import { useDebounce } from './useDebounce'
import api from '../lib/api'
import type { PhotoListResponse } from '../types'

export function usePhotoSearch(searchTerm: string, pageSize = 50) {
  const debouncedSearchTerm = useDebounce(searchTerm, 300)

  return useQuery({
    queryKey: ['photos', 'search', debouncedSearchTerm],
    queryFn: async () => {
      if (!debouncedSearchTerm.trim() || debouncedSearchTerm.length < 2) {
        return { items: [], totalCount: 0, page: 1, pageSize, totalPages: 0 } as PhotoListResponse
      }
      const { data } = await api.get<PhotoListResponse>('/photos/search', {
        params: { q: debouncedSearchTerm, page: 1, pageSize },
      })
      return data
    },
    enabled: debouncedSearchTerm.length >= 2,
    staleTime: 30000, // Cache results for 30 seconds
  })
}
