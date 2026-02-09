# Phase 07: Photo Upload UI

## Context Links

- [Main Plan](./plan.md)
- [Tech Stack - File Upload](../../docs/tech-stack.md)
- [React SPA Report - Section 5: File Upload Strategy](../reports/260209-researcher-to-initializer-react-spa-architecture-report.md)
- [Photo Management Features Report - Section 1: Core Upload Functionality](../reports/260209-researcher-to-initializer-photo-management-features-report.md)

---

## Overview

| Field | Value |
|-------|-------|
| Date | 2026-02-09 |
| Priority | High - Primary user interaction |
| Status | pending |
| Progress | 0% |
| Estimated Effort | 5-6 hours |
| Depends On | Phase 04 (Photo Management API), Phase 06 (Frontend Foundation) |

---

## Key Insights

- Drag-and-drop upload with visual feedback (highlighted drop zones, semi-transparent previews) significantly improves UX
- Chunked uploads (500KB chunks) enable progress tracking, pause/resume, and recovery from failures
- File preview before upload lets users confirm their selection and remove unwanted files
- Background uploads allow users to continue browsing while files process
- Upload queue with per-file status (pending, uploading, complete, error) provides clear feedback
- Client-side validation (format, size) before upload saves bandwidth and server resources

---

## Requirements

1. Implement drag-and-drop upload zone with visual feedback
2. Support multiple file selection via file picker and drag-and-drop
3. Show file previews (thumbnails) before upload begins
4. Implement chunked upload with per-file progress tracking
5. Display upload queue with individual file status
6. Support canceling individual uploads
7. Client-side file validation (type, size)
8. Background upload capability (continue browsing while uploading)
9. Handle upload errors with retry option

---

## Architecture

### Upload Flow
```
1. User drops/selects files
2. Client validates each file (type, size)
3. Preview thumbnails generated client-side
4. Files added to upload queue (Zustand store)
5. Upload starts automatically (or on confirmation)
6. Each file uploads via chunked POST to /api/photos
7. Progress tracked per file (0-100%)
8. On success: thumbnail from server replaces local preview
9. On error: show error message with retry button
10. User can navigate away; uploads continue in background
```

### Component Structure
```
<UploadPage>
  <DropZone>                    # Drag-and-drop area
    <DropOverlay />             # Visual feedback during drag
    <FileInput />               # Hidden file input for click-to-browse
  </DropZone>
  <UploadPreview>               # Grid of selected file previews
    <PreviewCard />             # Individual file with remove button
  </UploadPreview>
  <UploadQueue>                 # Active upload progress
    <UploadItem />              # Per-file progress bar and status
  </UploadQueue>
</UploadPage>

<UploadIndicator />             # Persistent indicator in header (background uploads)
```

### Zustand Upload Store
```typescript
interface UploadStore {
  queue: UploadItem[]           // Files pending/uploading
  addFiles: (files: File[]) => void
  removeFile: (id: string) => void
  startUpload: () => void
  cancelUpload: (id: string) => void
  retryUpload: (id: string) => void
  updateProgress: (id: string, progress: number) => void
  setStatus: (id: string, status: UploadStatus) => void
}

type UploadStatus = 'pending' | 'validating' | 'uploading' | 'processing' | 'complete' | 'error'
```

---

## Related Code Files

| File | Action | Purpose |
|------|--------|---------|
| `src/client/src/features/upload/UploadPage.tsx` | Create | Upload page |
| `src/client/src/features/upload/DropZone.tsx` | Create | Drag-and-drop component |
| `src/client/src/features/upload/UploadPreview.tsx` | Create | File preview grid |
| `src/client/src/features/upload/PreviewCard.tsx` | Create | Individual preview |
| `src/client/src/features/upload/UploadQueue.tsx` | Create | Upload progress list |
| `src/client/src/features/upload/UploadItem.tsx` | Create | Per-file progress |
| `src/client/src/features/upload/UploadIndicator.tsx` | Create | Header indicator |
| `src/client/src/features/upload/useUpload.ts` | Create | Upload logic hook |
| `src/client/src/features/upload/validators.ts` | Create | File validation |
| `src/client/src/stores/uploadStore.ts` | Create | Upload state management |
| `src/client/src/lib/uploadApi.ts` | Create | Chunked upload API |
| `src/client/src/components/AppLayout.tsx` | Modify | Add UploadIndicator |

---

## Implementation Steps

1. **Create file validation utilities**
   - Validate MIME types: image/jpeg, image/png, image/webp, image/heic
   - Validate file size: maximum 20MB per file
   - Validate file extension as secondary check
   - Return descriptive error messages per file
   - Batch validation: skip invalid files, report them to user

2. **Create upload store (Zustand)**
   - Track upload queue as array of UploadItem objects
   - Each UploadItem: id (generated), file, preview (object URL), progress, status, error
   - `addFiles()`: validate, generate previews, add to queue
   - `removeFile()`: revoke object URL, remove from queue
   - `updateProgress()`: update specific file progress
   - `setStatus()`: update specific file status
   - Computed selectors: activeUploads, completedCount, hasErrors, overallProgress

3. **Implement DropZone component**
   - Full-area drop zone with visual feedback
   - onDragEnter/onDragOver: show overlay with "Drop photos here" message
   - onDragLeave: hide overlay
   - onDrop: extract files, call addFiles()
   - Prevent browser default file open behavior
   - Include hidden `<input type="file" multiple accept="image/*">` for click-to-browse
   - Large visible "Browse files" button as alternative to drag-and-drop
   - Support paste from clipboard (Ctrl+V)

4. **Implement UploadPreview component**
   - Grid of thumbnail previews for selected files
   - Generate previews using `URL.createObjectURL()` (revoke on cleanup)
   - Each preview card shows: thumbnail, filename, file size
   - Remove button on each card (with confirmation for large selections)
   - Show validation errors on invalid files (red border, error icon)
   - "Upload All" and "Clear All" action buttons

5. **Implement chunked upload API**
   - Use UpChunk library or custom implementation
   - Chunk size: 500KB for optimal balance
   - For simple implementation (MVP): use FormData with standard upload
   - Track upload progress via XMLHttpRequest or Axios onUploadProgress
   - Handle network errors with automatic retry (3 attempts)
   - Support cancellation via AbortController
   - On completion, invalidate TanStack Query photo cache

6. **Implement useUpload hook**
   - Orchestrates file selection, validation, and upload
   - Manages concurrent uploads (limit to 3 simultaneous)
   - Calls upload API for each file in queue
   - Updates store with progress and status
   - Handles completion: invalidate queries, show success notification
   - Handles errors: set error status, enable retry

7. **Implement UploadQueue component**
   - List of files currently uploading or recently completed
   - Each item shows: filename, progress bar (percentage), status icon
   - Status indicators: spinner (uploading), checkmark (complete), X (error)
   - Retry button on failed uploads
   - Cancel button on active uploads
   - "Clear completed" button to clean up the list

8. **Implement UploadIndicator for header**
   - Small indicator in app header showing upload status
   - Shows count: "Uploading 3 of 10 photos..."
   - Progress bar showing overall progress
   - Clicking opens full upload queue view
   - Persists across page navigation (background uploads)

9. **Add upload notifications**
   - Toast notification when all uploads complete
   - Error notification with count of failed uploads
   - Notification linking to upload page for details

10. **Handle edge cases**
    - Duplicate file detection (warn user, allow override)
    - Browser tab close during upload (onBeforeUnload warning)
    - Loss of network during upload (pause and prompt retry)
    - Very large batches (100+ files) with performance considerations

---

## Todo List

- [ ] Create file validation utilities (type, size, extension)
- [ ] Create upload store with Zustand
- [ ] Implement DropZone with drag-and-drop visual feedback
- [ ] Implement click-to-browse file selection
- [ ] Implement clipboard paste support
- [ ] Create UploadPreview grid with thumbnail generation
- [ ] Create PreviewCard with remove functionality
- [ ] Implement chunked/progressive upload API client
- [ ] Implement useUpload hook with concurrent upload management
- [ ] Create UploadQueue with per-file progress bars
- [ ] Create UploadItem with status indicators and retry
- [ ] Create UploadIndicator for header (background uploads)
- [ ] Implement upload cancellation via AbortController
- [ ] Add cache invalidation on successful upload
- [ ] Add toast notifications for upload completion
- [ ] Handle browser close warning during active uploads
- [ ] Test drag-and-drop with multiple files
- [ ] Test file validation rejects invalid types
- [ ] Test upload progress tracking accuracy
- [ ] Test cancel and retry functionality
- [ ] Test background upload continues during navigation
- [ ] Test large batch upload (50+ files)

---

## Success Criteria

- Users can drag and drop photos onto the upload zone
- Users can click to browse and select multiple files
- File previews display before upload begins
- Invalid files (wrong type, too large) are flagged immediately with error messages
- Upload progress is visible per-file with percentage
- Users can cancel individual uploads
- Failed uploads can be retried
- Uploads continue in background when navigating to other pages
- Header indicator shows overall upload progress
- Gallery refreshes automatically after uploads complete
- The interface handles 50+ file selections without freezing

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Memory issues with many file previews | Medium | Medium | Use URL.createObjectURL and revoke after upload |
| Browser crash with very large files | Low | High | Validate file size client-side, chunk uploads |
| Upload fails silently | Low | High | Per-file error handling, retry mechanism |
| Slow preview generation | Medium | Low | Generate previews in batches, show placeholder first |
| CORS issues with chunked upload | Medium | Medium | Configure CORS on API to accept multipart requests |
| Lost uploads on page refresh | Medium | Medium | Warn before closing, consider IndexedDB for state |

---

## Security Considerations

- Validate file types client-side (UX) but always validate server-side (security)
- Revoke object URLs after use to prevent memory leaks
- Do not trust client-reported file sizes or types; server validates independently
- Sanitize filenames displayed in the UI (prevent XSS in filenames)
- Limit concurrent uploads to prevent browser resource exhaustion
- AbortController ensures cancelled uploads do not continue in background

---

## Next Steps

After completing this phase, proceed to:
- [Phase 08: Gallery Views](./phase-08-gallery-views.md) - Build the photo gallery, lightbox, album, and timeline views
