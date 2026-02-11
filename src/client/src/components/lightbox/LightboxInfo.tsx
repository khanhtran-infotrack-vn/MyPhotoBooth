import { useNavigate } from 'react-router-dom'
import type { Photo, PhotoDetails } from '../../types'
import { EditableDescription } from '../photos/EditableDescription'
import { useUpdatePhotoDescription, useRemoveTagFromPhoto, useTags, type Tag } from '../../hooks/useTags'
import { TagChip } from '../tags'

interface LightboxInfoProps {
  photo: Photo
  details?: PhotoDetails | null
  onClose: () => void
}

export function LightboxInfo({ photo, details, onClose }: LightboxInfoProps) {
  const navigate = useNavigate()
  const updateDescription = useUpdatePhotoDescription()
  const removeTagFromPhoto = useRemoveTagFromPhoto()
  const { data: allTags = [] } = useTags()

  // Convert tag names to Tag objects for display
  const photoTags: Tag[] = (details?.tags || []).map(tagName => {
    const tag = allTags.find(t => t.name === tagName)
    return tag || {
      id: '',
      name: tagName,
      createdAt: '',
      photoCount: 0
    }
  })

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return `${bytes} B`
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
  }

  interface RationalNumber {
    Numerator: number
    Denominator: number
  }

  const formatExifValue = (value: any): string => {
    if (typeof value === 'object' && value !== null && 'Numerator' in value && 'Denominator' in value) {
      const rational = value as RationalNumber
      return rational.Denominator === 1
        ? rational.Numerator.toString()
        : `${rational.Numerator}/${rational.Denominator}`
    }
    return String(value)
  }

  const parseExifData = (exifJson: string | null): Record<string, any> => {
    if (!exifJson) return {}
    try {
      return JSON.parse(exifJson)
    } catch {
      return {}
    }
  }

  const exif = parseExifData(details?.exifData ?? null)

  const handleDescriptionSave = async (newDescription: string | null) => {
    await updateDescription.mutateAsync({ photoId: photo.id, description: newDescription })
  }

  const handleTagRemove = async (tagId: string) => {
    if (tagId) {
      await removeTagFromPhoto.mutateAsync({ photoId: photo.id, tagId })
    }
  }

  const handleTagClick = (tagId: string) => {
    if (tagId) {
      navigate(`/tags/${tagId}`)
    }
  }

  return (
    <div
      className="absolute top-0 right-0 bottom-0 w-80 bg-gray-900/95 backdrop-blur-sm z-30
           border-l border-white/10 animate-slide-in-right-panel overflow-y-auto"
      onClick={(e) => e.stopPropagation()}
    >
      {/* Header */}
      <div className="sticky top-0 flex items-center justify-between p-4 bg-gray-900/95 border-b border-white/10">
        <h3 className="text-white font-semibold">Info</h3>
        <button
          onClick={onClose}
          className="p-1 text-white/70 hover:text-white transition-colors"
        >
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div className="p-4 space-y-6">
        {/* Basic info */}
        <section>
          <h4 className="text-xs font-semibold text-white/50 uppercase tracking-wider mb-3">
            Details
          </h4>
          <div className="space-y-2">
            <InfoRow label="Filename" value={photo.originalFileName} />
            <InfoRow
              label="Date taken"
              value={
                photo.capturedAt
                  ? new Date(photo.capturedAt).toLocaleString()
                  : 'Unknown'
              }
            />
            <InfoRow
              label="Date uploaded"
              value={new Date(photo.uploadedAt).toLocaleString()}
            />
            {details && (
              <>
                <InfoRow label="Size" value={formatFileSize(details.fileSize)} />
                <InfoRow label="Dimensions" value={`${photo.width} × ${photo.height}`} />
              </>
            )}
          </div>
        </section>

        {/* Description - Editable */}
        {details && (
          <EditableDescription
            description={details.description}
            onSave={handleDescriptionSave}
          />
        )}

        {/* Tags - Interactive */}
        {details && (
          <section>
            <h4 className="text-xs font-semibold text-white/50 uppercase tracking-wider mb-3">
              Tags
            </h4>
            {photoTags.length > 0 ? (
              <div className="flex flex-wrap gap-2">
                {photoTags.map((tag) => (
                  <TagChip
                    key={tag.name}
                    tag={tag}
                    onClick={tag.id ? () => handleTagClick(tag.id) : undefined}
                    onRemove={tag.id ? () => handleTagRemove(tag.id) : undefined}
                    variant="dark"
                    size="sm"
                  />
                ))}
              </div>
            ) : (
              <p className="text-white/40 text-sm italic">No tags</p>
            )}
          </section>
        )}

        {/* EXIF data */}
        {Object.keys(exif).length > 0 && (
          <section>
            <h4 className="text-xs font-semibold text-white/50 uppercase tracking-wider mb-3">
              Camera info
            </h4>
            <div className="space-y-2">
              {exif.Make && <InfoRow label="Camera" value={`${exif.Make} ${exif.Model || ''}`} />}
              {exif.FocalLength && <InfoRow label="Focal length" value={formatExifValue(exif.FocalLength)} />}
              {exif.FNumber && <InfoRow label="Aperture" value={`f/${formatExifValue(exif.FNumber)}`} />}
              {exif.ExposureTime && <InfoRow label="Shutter speed" value={formatExifValue(exif.ExposureTime)} />}
              {exif.ISO && <InfoRow label="ISO" value={formatExifValue(exif.ISO)} />}
            </div>
          </section>
        )}
      </div>
    </div>
  )
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between text-sm">
      <span className="text-white/50">{label}</span>
      <span className="text-white/90 text-right truncate ml-2 max-w-[180px]">{value}</span>
    </div>
  )
}
