# Bulk Operations for Photos - Implementation Plan

## 1. Technical Architecture

### Backend (ASP.NET Core 10 with CQRS/MediatR)

**New Commands:**
```
Application/Features/Photos/Commands/
├── BulkDeletePhotosCommand.cs
├── BulkToggleFavoritePhotosCommand.cs
├── BulkAddPhotosToAlbumCommand.cs
├── BulkRemovePhotosFromAlbumCommand.cs
└── BulkDownloadPhotosCommand.cs
```

**New Handlers:**
```
Application/Features/Photos/Handlers/
├── BulkDeletePhotosCommandHandler.cs
├── BulkToggleFavoritePhotosCommandHandler.cs
├── BulkAddPhotosToAlbumCommandHandler.cs
├── BulkRemovePhotosFromAlbumCommandHandler.cs
└── BulkDownloadPhotosCommandHandler.cs
```

**New Validators:**
```
Application/Features/Photos/Validators/
├── BulkDeletePhotosCommandValidator.cs
├── BulkToggleFavoritePhotosCommandValidator.cs
├── BulkAddPhotosToAlbumCommandValidator.cs
├── BulkRemovePhotosFromAlbumCommandValidator.cs
└── BulkDownloadPhotosCommandValidator.cs
```

**New Controller Endpoints:**
```csharp
// PhotosController.cs additions
POST /api/photos/bulk/delete
POST /api/photos/bulk/favorite
POST /api/photos/bulk/add-to-album
POST /api/photos/bulk/remove-from-album
GET  /api/photos/bulk/download?photoIds=guid1,guid2
```

**New DTOs:**
```csharp
// Common/DTOs/
BulkOperationResultDto.cs
BulkOperationProgressDto.cs
```

### Database Changes

**No schema changes required** - Using existing Photo and FavoritePhoto entities.

**Potential Performance Optimization (Optional):**
```sql
-- Consider adding index for bulk delete operations
CREATE INDEX CONCURRENTLY IX_Photos_UserId_Id ON "Photos"("UserId", "Id");
```

### Frontend (React + TypeScript + TanStack Query)

**New Components:**
```
client/src/components/bulk/
├── BulkActionsBar.tsx          // Floating action bar
├── BulkActionProgress.tsx       // Progress indicator for operations
├── SelectAllCheckbox.tsx        // Select/deselect all toggle
└── ConfirmBulkActionModal.tsx   // Confirmation dialog
```

**Enhanced Components:**
```
client/src/components/photos/
├── PhotoGrid.tsx                // Add drag selection support
└── SelectionBar.tsx             // Enhanced with more actions
```

**New Hooks:**
```
client/src/hooks/
├── useBulkOperations.ts         // Bulk operation mutations
└── useBulkDownload.ts           // ZIP download handling
```

**Enhanced State:**
```
client/src/stores/
└── selectionStore.ts            // Already exists, add undo capability
```

**New Utilities:**
```
client/src/utils/
└── downloadUtils.ts             // ZIP download utilities
```

## 2. API Design

### Bulk Delete Photos

**Request:**
```csharp
public record BulkDeletePhotosCommand(
    List<Guid> PhotoIds,
    string UserId
) : ICommand<BulkOperationResultDto>;
```

**Response:**
```csharp
public record BulkOperationResultDto(
    int TotalCount,
    int SuccessCount,
    int FailedCount,
    List<BulkOperationErrorDto> Errors
);

public record BulkOperationErrorDto(
    Guid PhotoId,
    string ErrorMessage
);
```

**Endpoint:**
```csharp
[HttpPost("bulk/delete")]
public async Task<IActionResult> BulkDelete(
    [FromBody] BulkDeletePhotosCommand command,
    CancellationToken cancellationToken)
{
    var result = await _mediator.Send(command, cancellationToken);
    return result.ToHttpResponse();
}
```

### Bulk Toggle Favorite

**Request:**
```csharp
public record BulkToggleFavoritePhotosCommand(
    List<Guid> PhotoIds,
    bool IsFavorite,  // true = add to favorites, false = remove
    string UserId
) : ICommand<BulkOperationResultDto>;
```

### Bulk Add to Album

**Request:**
```csharp
public record BulkAddPhotosToAlbumCommand(
    Guid AlbumId,
    List<Guid> PhotoIds,
    string UserId
) : ICommand<BulkOperationResultDto>;
```

### Bulk Remove from Album

**Request:**
```csharp
public record BulkRemovePhotosFromAlbumCommand(
    Guid AlbumId,
    List<Guid> PhotoIds,
    string UserId
) : ICommand<BulkOperationResultDto>;
```

### Bulk Download (ZIP)

**Endpoint:**
```csharp
[HttpGet("bulk/download")]
public async Task<IActionResult> BulkDownload(
    [FromQuery] string photoIds,
    CancellationToken cancellationToken)
{
    var ids = photoIds.Split(',').Select(Guid.Parse).ToList();
    var query = new BulkDownloadPhotosQuery(ids, GetUserId());
    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
        return File(result.Value.ZipStream, "application/zip", "photos.zip");

    return result.ToHttpResponse();
}
```

## 3. Frontend Components

### Component Hierarchy

```
PhotoGallery.tsx
├── PhotoGrid.tsx
│   ├── DateGroupHeader.tsx
│   │   └── SelectAllCheckbox.tsx
│   └── PhotoItem (rendered by RowsPhotoAlbum)
│       └── Checkbox + Visual Selection
├── BulkActionsBar.tsx (floating bottom bar)
│   ├── Selection Count
│   ├── Delete Button
│   ├── Favorite/Unfavorite Button
│   ├── Add to Album Button
│   ├── Download Button
│   └── Cancel Button
└── BulkActionProgress.tsx (modal overlay)
    └── Operation Status + Errors
```

### BulkActionsBar Component

```typescript
interface BulkActionsBarProps {
  selectedIds: Set<string>
  onAction: (action: BulkAction, photoIds: string[]) => void
  availableActions: BulkAction[]
}

type BulkAction =
  | 'delete'
  | 'addFavorite'
  | 'removeFavorite'
  | 'addToAlbum'
  | 'removeFromAlbum'
  | 'download'
```

### BulkActionProgress Component

```typescript
interface BulkActionProgressProps {
  operation: BulkOperation
  progress: number
  total: number
  errors: BulkError[]
  onComplete: () => void
  onCancel: () => void
}

interface BulkOperation {
  type: BulkAction
  status: 'pending' | 'in-progress' | 'completed' | 'failed'
}
```

## 4. Implementation Steps

### Phase 1: Backend Foundation

**Step 1.1: Create DTOs**
- Create `BulkOperationResultDto` in `Common/DTOs/`
- Create `BulkOperationErrorDto` in `Common/DTOs/`

**Step 1.2: Implement Commands**
- `BulkDeletePhotosCommand` - Delete multiple photos
- `BulkToggleFavoritePhotosCommand` - Add/remove favorites
- `BulkAddPhotosToAlbumCommand` - Add to existing album
- `BulkRemovePhotosFromAlbumCommand` - Remove from album

**Step 1.3: Implement Handlers**
```csharp
// Pattern for all bulk handlers
public class BulkDeletePhotosCommandHandler : ICommandHandler<BulkDeletePhotosCommand, BulkOperationResultDto>
{
    private readonly AppDbContext _context;
    private readonly IPhotoStorageService _storage;

    public async Task<Result<BulkOperationResultDto>> Handle(
        BulkDeletePhotosCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<BulkOperationErrorDto>();
        var photos = await _context.Photos
            .Where(p => request.PhotoIds.Contains(p.Id) && p.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var successCount = 0;
        foreach (var photo in photos)
        {
            try
            {
                // Delete files
                _storage.DeletePhoto(photo.FilePath, photo.ThumbnailPath);

                // Remove from database
                _context.Photos.Remove(photo);
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add(new(photo.Id, ex.Message));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new BulkOperationResultDto(
            request.PhotoIds.Count,
            successCount,
            errors.Count,
            errors
        ));
    }
}
```

**Step 1.4: Implement Validators**
```csharp
public class BulkDeletePhotosCommandValidator : AbstractValidator<BulkDeletePhotosCommand>
{
    public BulkDeletePhotosCommandValidator()
    {
        RuleFor(x => x.PhotoIds)
            .NotEmpty().WithMessage(Errors.Photos.PhotoIdsRequired)
            .Must(x => x.Count <= 100).WithMessage(Errors.Photos.BulkOperationLimit);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(Errors.Auth.UserIdRequired);
    }
}
```

**Step 1.5: Add Controller Endpoints**
- Add bulk endpoints to `PhotosController.cs`
- Use authorization attributes
- Add route attributes

**Step 1.6: Implement ZIP Download (Optional - Phase 2)**
- Create `BulkDownloadPhotosQuery`
- Use `System.IO.Compression.ZipArchive`
- Stream response to avoid memory issues

### Phase 2: Frontend Core Features

**Step 2.1: Enhance SelectionStore**
```typescript
// Add undo capability
interface SelectionState {
  // ... existing
  lastSelection?: {
    ids: string[]
    action: BulkAction
    timestamp: number
  }
  undoLastAction: () => void
  canUndo: boolean
}
```

**Step 2.2: Create Bulk Operations Hook**
```typescript
// hooks/useBulkOperations.ts
export function useBulkOperations() {
  const queryClient = useQueryClient()

  const deletePhotos = useMutation({
    mutationFn: async (photoIds: string[]) => {
      const { data } = await api.post('/photos/bulk/delete', { photoIds })
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['photos'] })
      queryClient.invalidateQueries({ queryKey: ['favorites'] })
    }
  })

  const toggleFavorites = useMutation({
    mutationFn: async ({ photoIds, isFavorite }: { photoIds: string[], isFavorite: boolean }) => {
      const { data } = await api.post('/photos/bulk/favorite', { photoIds, isFavorite })
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['photos'] })
      queryClient.invalidateQueries({ queryKey: ['favorites'] })
    }
  })

  const addToAlbum = useMutation({
    mutationFn: async ({ albumId, photoIds }: { albumId: string, photoIds: string[] }) => {
      const { data } = await api.post('/photos/bulk/add-to-album', { albumId, photoIds })
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['albums'] })
      queryClient.invalidateQueries({ queryKey: ['album-photos'] })
    }
  })

  return { deletePhotos, toggleFavorites, addToAlbum }
}
```

**Step 2.3: Create BulkActionsBar Component**
- Floating bottom bar
- Shows selection count
- Action buttons with icons
- Keyboard shortcuts support

**Step 2.4: Add Drag Selection**
- Enhance PhotoGrid with mouse drag selection
- Use useRef to track drag state
- Calculate intersection with photo elements

**Step 2.5: Create BulkActionProgress Modal**
- Shows progress bar
- Displays errors
- Cancel button
- Auto-dismiss on success

### Phase 3: Integration & Polish

**Step 3.1: Add to PhotoGallery**
- Import BulkActionsBar
- Show/hide based on selection count
- Handle action callbacks
- Refresh data after operations

**Step 3.2: Add to AlbumDetail Page**
- Same bulk actions
- Add "Remove from Album" action
- Context-aware actions

**Step 3.3: Add Undo Capability**
- Store last operation in Zustand
- Show toast with undo button
- Implement undo logic (restore deleted photos, toggle back)

**Step 3.4: Add Keyboard Shortcuts**
- Ctrl+A to select all
- Escape to deselect all
- Delete key to delete selected
- Ctrl+D to download

**Step 3.5: Performance Optimizations**
- Limit selection to 100 photos
- Debounce selection updates
- Virtualization for large grids

### Phase 4: Download Feature (Optional)

**Step 4.1: Backend ZIP Generation**
```csharp
public class BulkDownloadPhotosQueryHandler : IQueryHandler<BulkDownloadPhotosQuery, Stream>
{
    public async Task<Result<Stream>> Handle(BulkDownloadPhotosQuery request, CancellationToken ct)
    {
        var photos = await _context.Photos
            .Where(p => request.PhotoIds.Contains(p.Id) && p.UserId == request.UserId)
            .ToListAsync(ct);

        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var photo in photos)
            {
                var entry = archive.CreateEntry(photo.OriginalFileName);
                await using (var entryStream = entry.Open())
                await using (var fileStream = File.OpenRead(photo.FilePath))
                {
                    await fileStream.CopyToAsync(entryStream, ct);
                }
            }
        }

        memoryStream.Position = 0;
        return Result.Success<Stream>(memoryStream);
    }
}
```

**Step 4.2: Frontend Download Hook**
```typescript
export function useBulkDownload() {
  const download = async (photoIds: string[]) => {
    const response = await api.get('/photos/bulk/download', {
      params: { photoIds: photoIds.join(',') },
      responseType: 'blob'
    })

    const url = URL.createObjectURL(response.data)
    const link = document.createElement('a')
    link.href = url
    link.download = `photos-${Date.now()}.zip`
    link.click()
    URL.revokeObjectURL(url)
  }

  return { download }
}
```

## 5. Edge Cases & Considerations

### Concurrent Operations
- **Problem**: User performs bulk operation while another is in progress
- **Solution**: Disable action buttons during operations, show loading state

### Permission Checks
- **Problem**: User selects photos they don't own (via API manipulation)
- **Solution**: Always filter by UserId in handlers, return 403 for unauthorized

### Large Selections
- **Problem**: Selecting 1000+ photos causes performance issues
- **Solution**: Hard limit of 100 photos per bulk operation, show warning

### Partial Failures
- **Problem**: Some operations succeed, some fail
- **Solution**: Return detailed error list, allow retry of failed items

### Network Failures
- **Problem**: Request times out during large operations
- **Solution**: Implement chunking (process in batches of 50)

### Album Constraints
- **Problem**: Adding photos to full album or deleted album
- **Solution**: Validate album exists and has capacity before operation

### File System Errors
- **Problem**: File deletion fails due to locks or permissions
- **Solution**: Log errors, continue with remaining items, report failures

### Undo Limitations
- **Problem**: Undo not possible after page refresh
- **Solution**: Store undo state in localStorage with expiration

## 6. Testing Strategy

### Unit Tests (Backend)

**Validators:**
- `BulkDeletePhotosCommandValidatorTests`
  - Empty photoIds list should fail
  - More than 100 photoIds should fail
  - Empty userId should fail
  - Valid request should pass

**Handlers:**
- `BulkDeletePhotosCommandHandlerTests`
  - Delete all photos successfully
  - Partial deletion with errors
  - User can only delete own photos
  - Transaction rollback on error

- `BulkToggleFavoritePhotosCommandHandlerTests`
  - Add multiple photos to favorites
  - Remove multiple photos from favorites
  - Mixed add/remove operations

- `BulkAddPhotosToAlbumCommandHandlerTests`
  - Add photos to existing album
  - Handle duplicate photos in album
  - Validate album ownership

### Integration Tests

**API Endpoints:**
```csharp
public class BulkOperationsApiTests : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task BulkDelete_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var client = await GetAuthenticatedClient();
        var photos = await CreateTestPhotos(5);

        // Act
        var response = await client.PostAsJsonAsync("/api/photos/bulk/delete",
            new { photoIds = photos.Select(p => p.Id) });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        result.SuccessCount.Should().Be(5);
    }

    [Fact]
    public async Task BulkDelete_WithUnauthorizedPhoto_ReturnsPartialSuccess()
    {
        // Test that user cannot delete photos owned by another user
    }
}
```

### E2E Tests (Frontend)

**Selection:**
- Select single photo
- Select multiple photos via checkboxes
- Drag selection across multiple photos
- Select all / Deselect all
- Selection persists across filter changes

**Bulk Actions:**
- Bulk delete shows confirmation
- Bulk delete removes photos from grid
- Bulk favorite updates heart icons
- Bulk add to album opens modal
- Progress indicator shows correct count

**Error Handling:**
- Network error shows retry option
- Partial failure shows error count
- Large selection shows warning

**Download:**
- ZIP download creates correct file
- ZIP contains all selected photos
- Download progress indicator

### Performance Tests

- Bulk delete 100 photos: < 2 seconds
- Bulk favorite 100 photos: < 1 second
- Bulk add to album 100 photos: < 2 seconds
- ZIP download 50 photos (100MB total): < 10 seconds

## 7. Error Messages

**Add to `Application/Common/Errors.cs`:**
```csharp
public static class Photos
{
    public const string PhotoIdsRequired = "At least one photo ID is required";
    public const string BulkOperationLimit = "Cannot perform bulk operation on more than 100 photos";
    public const string PhotosNotFound = "One or more photos were not found";
    public const string NotPhotoOwner = "You do not have permission to modify these photos";
    public const string BulkOperationFailed = "Bulk operation completed with errors. Please review the error details.";
}
```

## 8. User Experience Flow

```
1. User enters selection mode (clicks checkbox)
2. User selects photos (checkboxes or drag)
3. BulkActionsBar appears with count
4. User clicks action (e.g., "Add to Album")
5. Confirm modal appears (for delete)
6. Progress modal shows operation status
7. On success: Toast notification + grid refresh
8. On partial failure: Error modal with retry option
9. Selection clears after successful operation
```

## 9. Accessibility

- Keyboard navigation: Tab to checkboxes, Space to select
- Screen reader announcements for selection count
- ARIA labels on all bulk action buttons
- Focus management in modals
- Skip to main content link when selection is active

## 10. Performance Considerations

**Frontend:**
- Use useMemo for expensive calculations
- Debounce rapid selection changes
- Virtualize photo grid for 1000+ photos
- Lazy load thumbnails for selected photos

**Backend:**
- Use async/await throughout
- Batch database operations
- Use transactions for data consistency
- Implement cancellation token support
- Consider background jobs for very large operations (1000+)

## 11. Implementation Decisions (User Choices)

### 1. ZIP Download Strategy: Synchronous
- **Decision**: Synchronous ZIP generation with 50 photo limit
- Implementation in Phase 4 will use synchronous ZIP creation
- Frontend shows loading indicator during download
- Limit to 50 photos for bulk download to prevent timeout

### 2. Mobile Selection Support: Included
- **Decision**: Include mobile long-press selection in Phase 1
- Add touch event handlers to PhotoGrid component
- Long-press (500ms) toggles selection on mobile
- Visual feedback with ripple effect on touch

### 3. Undo Scope: 5 Minutes Persisted
- **Decision**: Store undo state with 5-minute expiration
- Store last operation in Zustand with timestamp
- Check expiration on each action/undo attempt
- Persist to localStorage for cross-tab survival

## 12. Future Enhancements

- Bulk tag assignment
- Bulk description editing
- Bulk move to different storage location
- Bulk share link creation
- Selection presets (select all from today, this week, etc.)
- Smart selection (select all faces, select all landscapes)
