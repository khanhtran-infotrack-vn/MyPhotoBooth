import type { Photo, PhotoDetails } from '../../types'

interface LightboxInfoProps {
 photo: Photo
 details?: PhotoDetails | null
 onClose: () => void
}

export function LightboxInfo({ photo, details, onClose }: LightboxInfoProps) {
 const formatFileSize = (bytes: number) => {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
 }

 const parseExifData = (exifJson: string | null): Record<string, string> => {
  if (!exifJson) return {}
  try {
   return JSON.parse(exifJson)
  } catch {
   return {}
  }
 }

 const exif = parseExifData(details?.exifData ?? null)

 return (
  <div
   className="absolute top-0 right-0 bottom-0 w-80 bg-gray-900/95 backdrop-blur-sm z-30
        border-l border-white/10 animate-slide-in overflow-y-auto"
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

    {/* Description */}
    {details?.description && (
     <section>
      <h4 className="text-xs font-semibold text-white/50 uppercase tracking-wider mb-3">
       Description
      </h4>
      <p className="text-white/80 text-sm">{details.description}</p>
     </section>
    )}

    {/* Tags */}
    {details?.tags && details.tags.length > 0 && (
     <section>
      <h4 className="text-xs font-semibold text-white/50 uppercase tracking-wider mb-3">
       Tags
      </h4>
      <div className="flex flex-wrap gap-2">
       {details.tags.map((tag) => (
        <span
         key={tag}
         className="px-2 py-1 text-xs font-medium text-white/80 bg-white/10 rounded-full"
        >
         {tag}
        </span>
       ))}
      </div>
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
       {exif.FocalLength && <InfoRow label="Focal length" value={exif.FocalLength} />}
       {exif.FNumber && <InfoRow label="Aperture" value={`f/${exif.FNumber}`} />}
       {exif.ExposureTime && <InfoRow label="Shutter speed" value={exif.ExposureTime} />}
       {exif.ISO && <InfoRow label="ISO" value={exif.ISO} />}
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
