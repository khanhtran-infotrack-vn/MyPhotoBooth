# Phase 04: Photo Management API

## Context Links

- [Main Plan](./plan.md)
- [Tech Stack - File Storage & Image Processing](../../docs/tech-stack.md)
- [ASP.NET Core API Report - Sections 4-6](../reports/260209-researcher-to-initializer-aspnet-core-api-report.md)
- [Photo Management Features Report - Sections 1, 4](../reports/260209-researcher-to-initializer-photo-management-features-report.md)

---

## Overview

| Field | Value |
|-------|-------|
| Date | 2026-02-09 |
| Priority | High - Core feature of the application |
| Status | pending |
| Progress | 0% |
| Estimated Effort | 6-8 hours |
| Depends On | Phase 02 (Database), Phase 03 (Authentication) |

---

## Key Insights

- Use streaming for files >5MB to avoid memory exhaustion; buffered (IFormFile) for smaller files
- File signatures (magic numbers) must be validated, not just extensions -- attackers can rename files
- GUID-based storage keys prevent path traversal and ensure uniqueness
- Files stored outside wwwroot prevent direct serving and enforce authorization checks
- SixLabors.ImageSharp handles EXIF extraction, auto-rotation, and thumbnail generation cross-platform
- Thumbnail generation (300x300 max) is critical for gallery performance
- EXIF metadata stored as JSONB in PostgreSQL enables flexible querying without schema migrations

---

## Requirements

1. Implement photo upload endpoint with file validation and streaming support
2. Create image processing pipeline (EXIF extraction, auto-rotation, thumbnail generation)
3. Implement local file storage service with organized directory structure
4. Create photo CRUD endpoints (list with pagination, get details, update metadata, delete)
5. Implement photo download/serve endpoint with authorization
6. Build timeline endpoint for chronological photo browsing
7. Support bulk photo upload

---

## Architecture

### Upload Flow
```
Client -> POST /api/photos (multipart/form-data)
  -> Validate MIME type and file signature
  -> Generate GUID storage key
  -> Stream file to temp location
  -> Process image:
     1. Extract EXIF metadata
     2. Auto-rotate based on orientation
     3. Generate thumbnail (300x300)
     4. Compress if >2MB (JPEG quality 85)
  -> Move to permanent storage: /photos/{userId}/{year}/{month}/{guid}.jpg
  -> Save photo record to database
  -> Return photo metadata with URLs
```

### Storage Structure
```
{StorageRoot}/
  photos/
    {userId}/
      {year}/
        {month}/
          {guid}.jpg           # Original (processed)
          {guid}_thumb.jpg     # Thumbnail (300x300 max)
```

### API Endpoints
```
POST   /api/photos                          # Upload photo(s)
GET    /api/photos?page=1&pageSize=50       # List photos (paginated)
GET    /api/photos/{id}                     # Get photo details
PUT    /api/photos/{id}                     # Update description/metadata
DELETE /api/photos/{id}                     # Delete photo + files
GET    /api/photos/{id}/file                # Serve original image
GET    /api/photos/{id}/thumbnail           # Serve thumbnail
GET    /api/timeline?year=2026&month=2      # Timeline view
```

---

## Related Code Files

| File | Action | Purpose |
|------|--------|---------|
| `src/MyPhotoBooth.Application/Interfaces/IFileStorageService.cs` | Create | Storage abstraction |
| `src/MyPhotoBooth.Application/Interfaces/IImageProcessingService.cs` | Create | Image processing abstraction |
| `src/MyPhotoBooth.Application/Photos/Commands/UploadPhotoCommand.cs` | Create | Upload command/handler |
| `src/MyPhotoBooth.Application/Photos/Commands/UpdatePhotoCommand.cs` | Create | Update command/handler |
| `src/MyPhotoBooth.Application/Photos/Commands/DeletePhotoCommand.cs` | Create | Delete command/handler |
| `src/MyPhotoBooth.Application/Photos/Queries/GetPhotoQuery.cs` | Create | Get single photo |
| `src/MyPhotoBooth.Application/Photos/Queries/ListPhotosQuery.cs` | Create | List with pagination |
| `src/MyPhotoBooth.Application/Photos/Queries/TimelineQuery.cs` | Create | Timeline grouping |
| `src/MyPhotoBooth.Application/Common/DTOs/PhotoDTOs.cs` | Create | Photo DTOs |
| `src/MyPhotoBooth.Infrastructure/Storage/LocalFileStorageService.cs` | Create | Local file system implementation |
| `src/MyPhotoBooth.Infrastructure/Storage/ImageProcessingService.cs` | Create | ImageSharp processing |
| `src/MyPhotoBooth.API/Controllers/PhotosController.cs` | Create | Photo endpoints |

---

## Implementation Steps

1. **Define service interfaces in Application layer**
   - `IFileStorageService`: SaveFileAsync(stream, storageKey), DeleteFileAsync(storageKey), GetFileStreamAsync(storageKey), FileExistsAsync(storageKey)
   - `IImageProcessingService`: ProcessImageAsync(stream) -> ProcessedImageResult (containing original stream, thumbnail stream, ExifData, dimensions)
   - Define `ProcessedImageResult` record with all extracted data

2. **Implement LocalFileStorageService**
   - Configure base storage path from appsettings (outside wwwroot)
   - Build directory path: `{basePath}/photos/{userId}/{year}/{month}/`
   - Create directories if they don't exist
   - Save files using FileStream with async I/O
   - Delete both original and thumbnail on photo deletion
   - Return file streams for serving (with content type)

3. **Implement ImageProcessingService using SixLabors.ImageSharp**
   - Load image from stream using `Image.LoadAsync()`
   - Extract EXIF metadata: DateTimeOriginal, CameraModel, FocalLength, ISO, Aperture, GPS coordinates
   - Map EXIF data to `ExifData` domain object
   - Auto-rotate based on EXIF orientation tag
   - Generate thumbnail: `image.Clone(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(300, 300) }))`
   - Compress original if file size >2MB (JPEG quality 85)
   - Return processed streams and metadata

4. **Implement file validation**
   - Create `FileValidationService` or static helper
   - Whitelist MIME types: image/jpeg, image/png, image/webp, image/heic
   - Validate file signatures (magic numbers): JPEG (FF D8 FF), PNG (89 50 4E 47), WebP (52 49 46 46)
   - Enforce maximum file size: 20MB per photo
   - Return descriptive validation error messages

5. **Create photo upload command/handler**
   - Accept IFormFile (or stream for large files)
   - Validate file (type, size, signature)
   - Call ImageProcessingService to process image
   - Call FileStorageService to save original and thumbnail
   - Create Photo entity with all metadata
   - Save to database via repository
   - Return PhotoDTO with URLs

6. **Create photo list query with pagination**
   - Accept page, pageSize, sortBy (capturedAt, uploadedAt), order (asc, desc)
   - Filter by userId (from JWT claims)
   - Return paginated result with total count
   - Include thumbnail URLs in response

7. **Create timeline query**
   - Group photos by year/month based on CapturedAt
   - Accept optional year/month filter parameters
   - Return grouped results: `{ year: 2026, month: 2, photos: [...], count: 15 }`
   - Order by CapturedAt descending within groups

8. **Create PhotosController**
   - All endpoints require authentication (`[Authorize]`)
   - Upload: `POST /api/photos` with `[RequestSizeLimit(20_971_520)]`
   - List: `GET /api/photos` with query parameters
   - Details: `GET /api/photos/{id}` with ownership check
   - Update: `PUT /api/photos/{id}` with ownership check
   - Delete: `DELETE /api/photos/{id}` with ownership check and file cleanup
   - Serve file: `GET /api/photos/{id}/file` returning FileStreamResult
   - Serve thumbnail: `GET /api/photos/{id}/thumbnail` returning FileStreamResult
   - Timeline: `GET /api/timeline` with year/month parameters

9. **Configure file upload limits**
   - Set `RequestSizeLimit` on upload endpoint (20MB)
   - Configure Kestrel `MaxRequestBodySize` in Program.cs
   - Add multipart form options for large files

10. **Add caching headers for served images**
    - Set `Cache-Control: private, max-age=86400` for photo responses
    - Use ETag based on file modification time for conditional requests

---

## Todo List

- [ ] Create IFileStorageService interface
- [ ] Create IImageProcessingService interface
- [ ] Create ProcessedImageResult record
- [ ] Create photo request/response DTOs
- [ ] Implement LocalFileStorageService
- [ ] Implement ImageProcessingService with ImageSharp
- [ ] Implement file validation (MIME types, magic numbers, size)
- [ ] Create UploadPhotoCommand and handler
- [ ] Create UpdatePhotoCommand and handler
- [ ] Create DeletePhotoCommand and handler
- [ ] Create GetPhotoQuery and handler
- [ ] Create ListPhotosQuery with pagination
- [ ] Create TimelineQuery with chronological grouping
- [ ] Create PhotosController with all endpoints
- [ ] Configure upload size limits in Program.cs
- [ ] Add caching headers for served images
- [ ] Register services in dependency injection
- [ ] Test photo upload with valid JPEG
- [ ] Test file validation rejects invalid types
- [ ] Test thumbnail generation produces correct dimensions
- [ ] Test EXIF metadata extraction
- [ ] Test pagination returns correct results
- [ ] Test timeline grouping
- [ ] Test photo deletion removes files from disk
- [ ] Test ownership authorization (cannot access others' photos)

---

## Success Criteria

- Photos can be uploaded via POST /api/photos and are stored on the local file system
- Thumbnails are generated automatically at 300x300 max dimensions
- EXIF metadata (capture date, camera info) is extracted and stored as JSONB
- Images are auto-rotated based on EXIF orientation
- Invalid file types are rejected with descriptive errors
- File size limit of 20MB is enforced
- Photos are listed with pagination (default 50 per page)
- Timeline endpoint groups photos by year/month
- Only the photo owner can view, update, or delete their photos
- Deleting a photo removes both the database record and files from disk
- Photo serving endpoint returns correct content type and caching headers

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Large files causing memory issues | Medium | High | Use streaming for files >5MB |
| ImageSharp failing on corrupt images | Medium | Medium | Wrap in try-catch, return validation error |
| HEIC format not supported by ImageSharp | Medium | Low | Document limitation, convert client-side or use Magick.NET |
| Disk space exhaustion | Low | High | Monitor storage usage, set quotas per user |
| File permission issues on storage directory | Medium | Medium | Verify write permissions at startup |
| Concurrent upload race conditions | Low | Low | GUID storage keys ensure uniqueness |

---

## Security Considerations

- Never use client-provided filenames for storage (use GUID-based keys)
- Validate file content (magic numbers), not just Content-Type header
- Store files outside wwwroot to prevent direct URL access
- Serve files through an authorized endpoint that checks ownership
- Strip sensitive EXIF data (GPS) if user requests it
- Set maximum file size limits at both Kestrel and endpoint levels
- Scan files for malicious content in production (antivirus integration)
- Rate limit upload endpoint to prevent abuse

---

## Next Steps

After completing this phase, proceed to:
- [Phase 05: Album & Tag Management](./phase-05-album-tag-management.md) - Build album and tag CRUD operations
- [Phase 07: Photo Upload UI](./phase-07-photo-upload-ui.md) - Frontend upload interface (after Phase 06)
