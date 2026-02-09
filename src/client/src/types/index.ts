// Photo types
export interface Photo {
  id: string
  originalFileName: string
  width: number
  height: number
  capturedAt: string | null
  uploadedAt: string
  thumbnailPath: string
}

export interface PhotoDetails extends Photo {
  fileSize: number
  description: string | null
  exifData: string | null
  tags: string[]
}

export interface PhotoGroup {
  date: string
  photos: Photo[]
}

// Pagination
export interface PaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

// Album types
export interface Album {
  id: string
  name: string
  description: string | null
  coverPhotoId: string | null
  photoCount: number
  createdAt: string
  updatedAt: string
}

export interface AlbumWithCover extends Album {
  coverPhoto?: Photo
}

// Tag types
export interface Tag {
  id: string
  name: string
  photoCount: number
}

// API response types
export type PhotoListResponse = PaginatedResponse<Photo>
