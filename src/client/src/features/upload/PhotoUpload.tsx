import { useState, useRef, useEffect } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import api from '../../lib/api'

interface PhotoUploadProps {
  onClose: () => void
}

export function PhotoUpload({ onClose }: PhotoUploadProps) {
  const [uploading, setUploading] = useState(false)
  const [uploadProgress, setUploadProgress] = useState(0)
  const [totalFiles, setTotalFiles] = useState(0)
  const [completedFiles, setCompletedFiles] = useState(0)
  const [error, setError] = useState('')
  const [dragActive, setDragActive] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const queryClient = useQueryClient()

  // Close on Escape key
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && !uploading) {
        onClose()
      }
    }
    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [onClose, uploading])

  const handleFiles = async (files: FileList) => {
    if (!files || files.length === 0) return

    setUploading(true)
    setError('')
    setTotalFiles(files.length)
    setCompletedFiles(0)
    setUploadProgress(0)

    try {
      const filesArray = Array.from(files)
      for (let i = 0; i < filesArray.length; i++) {
        const file = filesArray[i]
        const formData = new FormData()
        formData.append('file', file)

        await api.post('/photos', formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
        })

        const completed = i + 1
        setCompletedFiles(completed)
        setUploadProgress(Math.round((completed / filesArray.length) * 100))
      }

      // Invalidate queries to refresh the gallery
      await queryClient.invalidateQueries({ queryKey: ['photos'] })

      if (fileInputRef.current) fileInputRef.current.value = ''
      onClose()
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } }
      setError(error.response?.data?.message || 'Upload failed. Please try again.')
    } finally {
      setUploading(false)
      setUploadProgress(0)
      setTotalFiles(0)
      setCompletedFiles(0)
    }
  }

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      handleFiles(e.target.files)
    }
  }

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true)
    } else if (e.type === 'dragleave') {
      setDragActive(false)
    }
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setDragActive(false)

    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      handleFiles(e.dataTransfer.files)
    }
  }

  const handleClick = () => {
    fileInputRef.current?.click()
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/50 backdrop-blur-sm"
        onClick={!uploading ? onClose : undefined}
      />

      {/* Modal */}
      <div className="relative w-full max-w-lg bg-white rounded-2xl shadow-2xl animate-scale-in">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">Upload Photos</h2>
          <button
            onClick={onClose}
            disabled={uploading}
            className="btn-icon text-gray-400 hover:text-gray-600 disabled:opacity-50"
          >
            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
          {error && (
            <div className="flex items-center gap-3 px-4 py-3 mb-4 text-sm font-medium text-red-700 bg-red-50 rounded-lg">
              <svg className="w-5 h-5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                  d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              {error}
            </div>
          )}

          <div
            onDragEnter={handleDrag}
            onDragLeave={handleDrag}
            onDragOver={handleDrag}
            onDrop={handleDrop}
            onClick={!uploading ? handleClick : undefined}
            className={`
              relative border-2 border-dashed rounded-xl p-12 text-center transition-all duration-200
              ${uploading ? 'cursor-not-allowed' : 'cursor-pointer'}
              ${dragActive
                ? 'border-primary-500 bg-primary-50'
                : 'border-gray-300 hover:border-gray-400 hover:bg-gray-50'
              }
            `}
          >
            <input
              ref={fileInputRef}
              type="file"
              accept="image/*"
              multiple
              onChange={handleFileChange}
              disabled={uploading}
              className="hidden"
            />

            {uploading ? (
              <div className="space-y-4">
                <div className="w-16 h-16 mx-auto border-4 border-gray-200 border-t-primary-600 rounded-full animate-spin" />
                <div>
                  <p className="text-lg font-semibold text-gray-900">
                    Uploading {completedFiles} of {totalFiles}...
                  </p>
                  <p className="text-sm text-gray-500 mt-1">{uploadProgress}% complete</p>
                </div>
                <div className="w-48 h-2 mx-auto bg-gray-200 rounded-full overflow-hidden">
                  <div
                    className="h-full bg-primary-600 transition-all duration-300"
                    style={{ width: `${uploadProgress}%` }}
                  />
                </div>
              </div>
            ) : (
              <>
                <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-primary-100 flex items-center justify-center">
                  <svg className="w-8 h-8 text-primary-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                      d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                    />
                  </svg>
                </div>
                <h3 className="text-lg font-semibold text-gray-900">
                  {dragActive ? 'Drop your photos here' : 'Drag and drop photos'}
                </h3>
                <p className="text-sm text-gray-500 mt-2 mb-4">
                  or click to browse from your computer
                </p>
                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation()
                    handleClick()
                  }}
                  className="btn-primary"
                >
                  Choose Files
                </button>
                <p className="text-xs text-gray-400 mt-4">
                  Supports JPEG, PNG, GIF, WebP (max 50MB each)
                </p>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

// Keep default export for backward compatibility
export default PhotoUpload
