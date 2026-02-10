import { useMutation } from '@tanstack/react-query'
import api from '../lib/api'

export function useBulkDownload() {
  const downloadMutation = useMutation({
    mutationFn: async (photoIds: string[]) => {
      const photoIdsParam = photoIds.join(',')
      const response = await api.get(`/photos/bulk/download?photoIds=${photoIdsParam}`, {
        responseType: 'blob'
      })
      return response
    },
    onSuccess: (response, _photoIds) => {
      // Create download link
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url

      // Get filename from Content-Disposition header or use default
      const contentDisposition = response.headers['content-disposition']
      let filename = `photos-${Date.now()}.zip`

      if (contentDisposition) {
        const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/)
        if (filenameMatch && filenameMatch[1]) {
          filename = filenameMatch[1].replace(/['"]/g, '')
        }
      }

      link.setAttribute('download', filename)
      document.body.appendChild(link)
      link.click()
      link.remove()
      window.URL.revokeObjectURL(url)
    },
    onError: (error: any) => {
      console.error('Download failed:', error)
      throw error
    }
  })

  return {
    downloadPhotos: downloadMutation.mutateAsync,
    isLoading: downloadMutation.isPending
  }
}
