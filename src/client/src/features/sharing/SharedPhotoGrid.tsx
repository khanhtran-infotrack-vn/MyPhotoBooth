import type { SharedPhoto } from '../../types'
import { getSharedThumbnailUrl } from '../../hooks/useSharedContent'

interface SharedPhotoGridProps {
  token: string
  photos: SharedPhoto[]
  onPhotoClick: (photo: SharedPhoto, index: number) => void
}

export function SharedPhotoGrid({ token, photos, onPhotoClick }: SharedPhotoGridProps) {
  if (photos.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-gray-500">
        <svg className="w-20 h-20 mb-4 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
            d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
          />
        </svg>
        <h3 className="text-xl font-semibold text-gray-700">No photos</h3>
      </div>
    )
  }

  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-2">
      {photos.map((photo, index) => (
        <div
          key={photo.id}
          className="relative group cursor-pointer overflow-hidden rounded-lg bg-gray-100 aspect-square"
          onClick={() => onPhotoClick(photo, index)}
        >
          <img
            src={getSharedThumbnailUrl(token, photo.id)}
            alt={photo.fileName}
            className="w-full h-full object-cover transition-transform duration-200 group-hover:scale-105"
            loading="lazy"
          />
          <div className="absolute inset-0 bg-black/0 group-hover:bg-black/10 transition-colors" />
        </div>
      ))}
    </div>
  )
}
