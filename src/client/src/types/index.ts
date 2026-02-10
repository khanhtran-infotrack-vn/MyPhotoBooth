// Photo types
export interface Photo {
  id: string
  originalFileName: string
  width: number
  height: number
  capturedAt: string | null
  uploadedAt: string
  thumbnailPath: string
  isFavorite: boolean
}

export interface PhotoDetails extends Photo {
  fileSize: number
  description: string | null
  exifData: string | null
  tags: string[]
  isFavorite: boolean
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

// Share types
export interface ShareLink {
  id: string
  token: string
  type: 'Photo' | 'Album'
  photoId: string | null
  albumId: string | null
  targetName: string | null
  hasPassword: boolean
  expiresAt: string | null
  allowDownload: boolean
  shareUrl: string
  isActive: boolean
  createdAt: string
}

export interface ShareMetadata {
  type: 'Photo' | 'Album'
  hasPassword: boolean
  isExpired: boolean
  isActive: boolean
}

export interface SharedPhoto {
  id: string
  fileName: string
  width: number
  height: number
  capturedAt: string | null
  uploadedAt: string
  description: string | null
  allowDownload: boolean
}

export interface SharedAlbum {
  name: string
  description: string | null
  allowDownload: boolean
  photos: SharedPhoto[]
}

export interface SharedContent {
  type: 'Photo' | 'Album'
  photo: SharedPhoto | null
  album: SharedAlbum | null
}

// API response types
export type PhotoListResponse = PaginatedResponse<Photo>

// Photo filter and sort types
export type PhotoSortOrder = 'uploadedAtDesc' | 'capturedAtDesc' | 'fileNameAsc'

export interface PhotoListFilters {
  sortBy?: PhotoSortOrder
  favorites?: boolean
  search?: string
}
