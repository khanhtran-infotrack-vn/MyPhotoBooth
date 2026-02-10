import { useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../lib/api'
import { useSelectionStore } from '../stores/selectionStore'
import { useToast } from '../hooks/useToast'

interface BulkOperationResult {
  successCount: number
  failedCount: number
  errors: Array<{
    photoId: string
    fileName: string
    errorMessage: string
  }>
}

export function useBulkOperations() {
  const queryClient = useQueryClient()
  const { clearSelection } = useSelectionStore()
  const { showToast } = useToast()

  const bulkDelete = useMutation({
    mutationFn: async (photoIds: string[]) => {
      const { data } = await api.post<BulkOperationResult>('/photos/bulk/delete', {
        photoIds
      })
      return data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['photos'] })
      queryClient.invalidateQueries({ queryKey: ['favorites'] })
      queryClient.invalidateQueries({ queryKey: ['albums'] })

      if (data.failedCount === 0) {
        showToast(`Deleted ${data.successCount} photo${data.successCount > 1 ? 's' : ''}`, 'success')
      } else {
        showToast(`Deleted ${data.successCount}, failed ${data.failedCount}`, 'warning')
      }

      clearSelection()
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to delete photos', 'error')
    }
  })

  const bulkToggleFavorite = useMutation({
    mutationFn: async ({ photoIds, favorite }: { photoIds: string[]; favorite: boolean }) => {
      const { data } = await api.post<BulkOperationResult>('/photos/bulk/favorite', {
        photoIds,
        favorite
      })
      return data
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['photos'] })
      queryClient.invalidateQueries({ queryKey: ['favorites'] })

      const action = variables.favorite ? 'added to' : 'removed from'
      if (data.failedCount === 0) {
        showToast(`${data.successCount} photo${data.successCount > 1 ? 's' : ''} ${action} favorites`, 'success')
      } else {
        showToast(`Processed ${data.successCount}, failed ${data.failedCount}`, 'warning')
      }

      clearSelection()
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to update favorites', 'error')
    }
  })

  const bulkAddToAlbum = useMutation({
    mutationFn: async ({ photoIds, albumId }: { photoIds: string[]; albumId: string }) => {
      const { data } = await api.post<BulkOperationResult>('/photos/bulk/add-to-album', {
        photoIds,
        albumId
      })
      return data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['albums'] })
      queryClient.invalidateQueries({ queryKey: ['album'] })

      if (data.failedCount === 0) {
        showToast(`Added ${data.successCount} photo${data.successCount > 1 ? 's' : ''} to album`, 'success')
      } else {
        showToast(`Added ${data.successCount}, failed ${data.failedCount}`, 'warning')
      }

      clearSelection()
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to add photos to album', 'error')
    }
  })

  const bulkRemoveFromAlbum = useMutation({
    mutationFn: async ({ photoIds, albumId }: { photoIds: string[]; albumId: string }) => {
      const { data } = await api.post<BulkOperationResult>('/photos/bulk/remove-from-album', {
        photoIds,
        albumId
      })
      return data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['albums'] })
      queryClient.invalidateQueries({ queryKey: ['album'] })

      if (data.failedCount === 0) {
        showToast(`Removed ${data.successCount} photo${data.successCount > 1 ? 's' : ''} from album`, 'success')
      } else {
        showToast(`Removed ${data.successCount}, failed ${data.failedCount}`, 'warning')
      }

      clearSelection()
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to remove photos from album', 'error')
    }
  })

  return {
    bulkDelete: bulkDelete.mutateAsync,
    bulkToggleFavorite: (photoIds: string[], favorite: boolean) =>
      bulkToggleFavorite.mutateAsync({ photoIds, favorite }),
    bulkAddToAlbum: bulkAddToAlbum.mutateAsync,
    bulkRemoveFromAlbum: bulkRemoveFromAlbum.mutateAsync,
    isLoading:
      bulkDelete.isPending ||
      bulkToggleFavorite.isPending ||
      bulkAddToAlbum.isPending ||
      bulkRemoveFromAlbum.isPending
  }
}
