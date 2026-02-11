# Tags & Photo Info Enhancement Implementation Plan

**Project**: MyPhotoBooth v1.4.0
**Status**: Planning
**Created**: 2025-02-11

---

## Overview

Enhance Tags and Photo Info features to improve photo organization and editing capabilities.

**Key Features**:
1. Tag badges on photo thumbnails (2-3 visible + "+X more")
2. Fix `/tags/:id` route with backend endpoint
3. Editable description in lightbox info panel
4. Interactive tag chips (view, remove, autocomplete to add)
5. Photo count per tag in tags list

---

## Architecture Context

**Backend**: ASP.NET Core 10 + CQRS (MediatR 14.0)
**Frontend**: React 18 + TypeScript + Vite
**State**: TanStack Query (server) + Zustand (client)
**Testing**: xUnit + Moq + FluentAssertions + Testcontainers

**Existing Backend Infrastructure**:
- `GetTagPhotosQuery` handler exists but not exposed via controller
- `AddTagsToPhotoCommand` and `RemoveTagsFromPhotoCommand` handlers exist
- `UpdatePhotoCommand` for description already exists
- Tag DTOs: `TagResponse`, `CreateTagRequest`

**Existing Frontend Infrastructure**:
- `useTags()`, `useSearchTags()`, `useCreateTag()`, `useDeleteTag()` hooks
- `PhotoGrid.tsx` - grid component (needs modification)
- `LightboxInfo.tsx` - info panel (needs enhancement)
- `TagList.tsx` - tags list (needs photo count)
- `TagPhotos.tsx` - placeholder page (needs full implementation)

---

## Backend Changes

### 1. New Commands/Queries

#### 1.1 Remove Single Tag from Photo Command

**File**: `src/MyPhotoBooth.Application/Features/Tags/Commands/RemoveTagFromPhotoCommand.cs`

```csharp
public record RemoveTagFromPhotoCommand(
    Guid PhotoId,
    Guid TagId,
    string UserId
) : ICommand;
```

**Handler**: `src/MyPhotoBooth.Application/Features/Tags/Handlers/RemoveTagFromPhotoCommandHandler.cs`

**Validator**: `src/MyPhotoBooth.Application/Features/Tags/Validators/RemoveTagFromPhotoCommandValidator.cs`

---

#### 1.2 Get Tags with Photo Count Query

**File**: `src/MyPhotoBooth.Application/Features/Tags/Queries/GetTagsWithPhotoCountQuery.cs`

```csharp
public record GetTagsWithPhotoCountQuery(string UserId) : IQuery<List<TagWithPhotoCountResponse>>;
```

**Response DTO** (update `TagDTOs.cs`):

```csharp
public class TagWithPhotoCountResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int PhotoCount { get; set; }
}
```

**Handler**: `src/MyPhotoBooth.Application/Features/Tags/Handlers/GetTagsWithPhotoCountQueryHandler.cs`

---

#### 1.3 Update GetTagPhotosQuery to Return Paginated Photos

**Modify**: `src/MyPhotoBooth.Application/Features/Tags/Queries/GetTagPhotosQuery.cs`

```csharp
public record GetTagPhotosQuery(
    Guid TagId,
    string UserId,
    int Page = 1,
    int PageSize = 50
) : IQuery<PaginatedResult<PhotoListResponse>>;
```

**Handler Update**: Add pagination support to `GetTagPhotosQueryHandler.cs`

---

### 2. New Validators

| Validator | Purpose |
|-----------|---------|
| `RemoveTagFromPhotoCommandValidator` | Validate PhotoId, TagId format |

---

### 3. Controller Endpoints

**File**: `src/MyPhotoBooth.API/Controllers/TagsController.cs`

#### Existing (already implemented):
- `GET /api/tags` - List all tags
- `GET /api/tags/{id}` - Get tag by ID
- `GET /api/tags/{id}/photos` - Get photos by tag (already exists!)
- `POST /api/tags` - Create tag
- `DELETE /api/tags/{id}` - Delete tag
- `GET /api/tags/search` - Search tags

#### New/Modified Endpoints:

```csharp
// Modify existing endpoint to support pagination
[HttpGet("{id}/photos")]
public async Task<IActionResult> GetTagPhotos(
    Guid id,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    CancellationToken cancellationToken)

// New: Get tags with photo count
[HttpGet("with-count")]
public async Task<IActionResult> GetTagsWithCount(CancellationToken cancellationToken)

// New: Remove single tag from photo
[HttpDelete("{tagId}/photos/{photoId}")]
public async Task<IActionResult> RemoveTagFromPhoto(
    Guid tagId,
    Guid photoId,
    CancellationToken cancellationToken)
```

**File**: `src/MyPhotoBooth.API/Controllers/PhotosController.cs`

#### New Endpoints:

```csharp
// Remove single tag from photo (alternative route)
[HttpDelete("{photoId}/tags/{tagId}")]
public async Task<IActionResult> RemoveTagFromPhoto(
    Guid photoId,
    Guid tagId,
    CancellationToken cancellationToken)
```

---

### 4. Database Changes

**No migrations required** - existing schema supports all features.

**Entity Updates**:
- Consider adding computed property for `PhotoCount` on `Tag` entity (optional)

---

### 5. Repository Changes

**File**: `src/MyPhotoBooth.Application/Interfaces/ITagRepository.cs`

```csharp
Task<TagWithPhotoCountResponse?> GetTagWithPhotoCountAsync(Guid tagId, CancellationToken cancellationToken);
Task<List<TagWithPhotoCountResponse>> GetTagsWithPhotoCountAsync(string userId, CancellationToken cancellationToken);
```

---

## Frontend Changes

### 1. Type Updates

**File**: `src/client/src/types/index.ts`

```typescript
// Update Tag interface to include photoCount
export interface Tag {
  id: string
  name: string
  createdAt: string
  photoCount?: number  // Add this
}
```

---

### 2. New Components

#### 2.1 TagChip Component

**File**: `src/client/src/components/tags/TagChip.tsx`

```typescript
interface TagChipProps {
  tag: Tag
  onRemove?: () => void
  onClick?: () => void
  variant?: 'default' | 'light' | 'dark'
}
```

**Features**:
- Clickable tag name
- X button for removal (optional)
- Variant styling for different contexts

---

#### 2.2 TagAutocomplete Component

**File**: `src/client/src/components/tags/TagAutocomplete.tsx`

```typescript
interface TagAutocompleteProps {
  selectedTags: Tag[]
  onTagAdd: (tag: Tag) => void
  onTagRemove: (tagId: string) => void
  excludedTagIds?: string[]
  placeholder?: string
}
```

**Features**:
- Search/create new tags
- Dropdown with existing tags
- Show "Create new tag" option for non-matching input
- Keyboard navigation

---

#### 2.3 EditableDescription Component

**File**: `src/client/src/components/photos/EditableDescription.tsx`

```typescript
interface EditableDescriptionProps {
  description: string | null
  onSave: (description: string) => Promise<void>
  readOnly?: boolean
}
```

**Features**:
- View mode with edit button
- Edit mode with textarea
- Save/Cancel buttons
- Optimistic updates

---

### 3. Modified Components

#### 3.1 PhotoGrid.tsx

**Changes**:
1. Add tag chips overlay on thumbnails
2. Show 2-3 tags + "+X more" indicator
3. Make tags clickable to filter

**Implementation**:
- Photo type needs `tags` array (add to API response)
- Use `TagChip` component (light variant)
- Position: Bottom-left or bottom-right overlay

---

#### 3.2 LightboxInfo.tsx

**Changes**:
1. Replace read-only description with `EditableDescription`
2. Replace static tags with interactive `TagChip` components
3. Add `TagAutocomplete` for adding new tags
4. Update local state on tag changes

**New Props**:
```typescript
interface LightboxInfoProps {
  photo: Photo
  details?: PhotoDetails | null
  onClose: () => void
  onDescriptionSave: (description: string) => Promise<void>
  onTagAdd: (tagId: string) => Promise<void>
  onTagRemove: (tagId: string) => Promise<void>
}
```

---

#### 3.3 TagList.tsx

**Changes**:
1. Show photo count per tag
2. Update API call to fetch tags with count
3. Style: Badge or number next to tag name

---

#### 3.4 TagPhotos.tsx

**Changes**:
1. Implement full photo gallery view
2. Fetch photos by tag ID using existing backend endpoint
3. Reuse `PhotoGrid` component
4. Show tag name and count in header

**Implementation**:
- Fetch tag details first
- Fetch photos with pagination
- Handle empty state

---

### 4. New Hooks

**File**: `src/client/src/hooks/useTagPhotos.ts`

```typescript
export function useTagPhotos(tagId: string, pageSize = 50) {
  return useInfiniteQuery({
    queryKey: ['tag', tagId, 'photos'],
    queryFn: async ({ pageParam = 1 }) => {
      const { data } = await api.get(`/tags/${tagId}/photos`, {
        params: { page: pageParam, pageSize }
      })
      return data
    },
    getNextPageParam: (lastPage) => {
      if (lastPage.page < lastPage.totalPages) {
        return lastPage.page + 1
      }
      return undefined
    },
    initialPageParam: 1,
  })
}

export function useRemoveTagFromPhoto() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ photoId, tagId }: { photoId: string; tagId: string }) => {
      await api.delete(`/photos/${photoId}/tags/${tagId}`)
    },
    onSuccess: (_, { photoId }) => {
      queryClient.invalidateQueries({ queryKey: ['photo', photoId] })
      queryClient.invalidateQueries({ queryKey: ['photos'] })
    },
  })
}
```

---

### 5. Route Updates

**File**: `src/client/src/App.tsx`

Existing route already exists:
- `/tags/:id` -> `TagPhotos` component (needs implementation)

---

## Implementation Order

### Phase 1: Backend Foundation (Priority: High)

| Task | Description | Dependencies |
|------|-------------|--------------|
| B1.1 | Create `RemoveTagFromPhotoCommand` + Handler + Validator | None |
| B1.2 | Create `GetTagsWithPhotoCountQuery` + Handler | None |
| B1.3 | Update `GetTagPhotosQuery` to support pagination | None |
| B1.4 | Add controller endpoints for new commands/queries | B1.1, B1.2, B1.3 |
| B1.5 | Add repository methods for tag photo counts | B1.2 |

**Estimated Time**: 4-6 hours

---

### Phase 2: Backend Testing (Priority: High - TFD Mandatory)

| Task | Description | Dependencies |
|------|-------------|--------------|
| T1 | Unit tests: `RemoveTagFromPhotoCommandValidator` | B1.1 |
| T2 | Unit tests: `RemoveTagFromPhotoCommandHandler` | B1.1 |
| T3 | Unit tests: `GetTagsWithPhotoCountQueryHandler` | B1.2 |
| T4 | Unit tests: `GetTagPhotosQueryHandler` (pagination) | B1.3 |
| T5 | Integration tests: `DELETE /api/tags/{tagId}/photos/{photoId}` | B1.1, B1.4 |
| T6 | Integration tests: `GET /api/tags/with-count` | B1.2, B1.4 |
| T7 | Integration tests: `GET /api/tags/{id}/photos` (paginated) | B1.3, B1.4 |

**Estimated Time**: 3-4 hours

---

### Phase 3: Frontend Components (Priority: Medium)

| Task | Description | Dependencies |
|------|-------------|--------------|
| F1.1 | Create `TagChip.tsx` component | None |
| F1.2 | Create `TagAutocomplete.tsx` component | None |
| F1.3 | Create `EditableDescription.tsx` component | None |
| F1.4 | Update `types/index.ts` with photoCount | None |

**Estimated Time**: 4-5 hours

---

### Phase 4: Frontend Integration (Priority: Medium)

| Task | Description | Dependencies |
|------|-------------|--------------|
| F2.1 | Update `PhotoGrid.tsx` to show tag badges | F1.1 |
| F2.2 | Update `LightboxInfo.tsx` with editable fields | F1.2, F1.3 |
| F2.3 | Update `TagList.tsx` to show photo counts | F1.1 |
| F2.4 | Implement `TagPhotos.tsx` full page | None |
| F2.5 | Create `useTagPhotos.ts` hook | None |
| F2.6 | Create `useRemoveTagFromPhoto()` hook | None |

**Estimated Time**: 5-6 hours

---

### Phase 5: Integration & Polish (Priority: Low)

| Task | Description | Dependencies |
|------|-------------|--------------|
| I1 | Update Photo API to include tags in list response | B1.2 |
| I2 | Add loading states and error handling | F2.x |
| I3 | Add animations for tag add/remove | F2.2 |
| I4 | Responsive design testing | All |
| I5 | Accessibility audit (ARIA labels, keyboard nav) | All |

**Estimated Time**: 3-4 hours

---

## Testing Strategy

### Backend Tests (Test-First Development)

**Test Database Strategy**: **Mocks/In-Memory** (Recommended)

**Reasoning**: Faster TFD cycle, no Docker dependency for unit tests. Use Testcontainers only for integration tests.

**Coverage Targets**:
- Validators: 100%
- Handlers: 80%+
- API Endpoints: 100% (integration)

**Unit Tests** (`tests/MyPhotoBooth.UnitTests/Features/Tags/`):

```csharp
// RemoveTagFromPhotoCommandValidatorTests.cs
- Valid_Guid_Passes()
- Invalid_PhotoId_Fails()
- Invalid_TagId_Fails()

// RemoveTagFromPhotoCommandHandlerTests.cs
- Remove_Tag_Success()
- Remove_Tag_From_NonExistent_Photo_Fails()
- Remove_Tag_From_Unauthorized_Photo_Fails()
- Remove_Tag_That_IsNotAttached_Success()

// GetTagsWithPhotoCountQueryHandlerTests.cs
- Get_Tags_With_Count_Returns_Correct_Counts()
- Get_Tags_For_User_Returns_Only_User_Tags()
- Tag_With_No_Photos_Returns_Zero()

// GetTagPhotosQueryHandlerTests.cs (pagination)
- Get_Photos_By_Tag_Returns_Paginated_Result()
- Page_Exceeding_Total_Returns_Empty()
- Invalid_TagId_Returns_Error()
```

**Integration Tests** (`tests/MyPhotoBooth.IntegrationTests/Tags/`):

```csharp
// RemoveTagFromPhotoEndpointTests.cs
- Delete_Removes_Tag_From_Photo()
- Delete_NonExistent_Tag_Returns_404()
- Delete_Unauthorized_Returns_403()

// GetTagsWithCountEndpointTests.cs
- Get_Tags_Returns_Photo_Counts()
- Get_Tags_Empty_List_Returns_Empty()

// GetTagPhotosEndpointTests.cs
- Get_Photos_By_Tag_Returns_Paginated()
- Get_Photos_By_Invalid_Tag_Returns_404()
```

---

### Frontend Testing

**Approach**: Manual testing + React Testing Library for critical components

**Test Cases**:

1. **TagChip Component**:
   - Renders tag name correctly
   - Triggers onClick when clicked
   - Triggers onRemove when X button clicked

2. **TagAutocomplete Component**:
   - Shows dropdown on input
   - Filters tags based on search
   - Creates new tag when non-matching input submitted

3. **EditableDescription Component**:
   - Shows description in view mode
   - Switches to edit mode on click
   - Saves description on save button click

4. **PhotoGrid Integration**:
   - Shows tag badges on hover
   - Shows "+X more" when >3 tags
   - Clicking tag navigates to tag page

5. **LightboxInfo Integration**:
   - Editable description saves correctly
   - Adding tag updates UI
   - Removing tag updates UI

---

## Edge Cases & Considerations

### 1. Tag Management

| Scenario | Handling |
|----------|----------|
| Tag has no photos | Show count of 0, still allow deletion |
| All tags removed from photo | Show empty state in lightbox |
| Concurrent tag edits | Last write wins (EF Core handles) |
| Tag name already exists | Backend validation returns error |
| User tries to add same tag twice | Backend ignores duplicate |

### 2. Photo Grid Tag Display

| Scenario | Handling |
|----------|----------|
| Photo has no tags | No tag overlay shown |
| Photo has 1-3 tags | Show all tags |
| Photo has 4+ tags | Show 2 tags + "+X more" |
| Tag name too long | Truncate with ellipsis |

### 3. Lightbox Info Panel

| Scenario | Handling |
|----------|----------|
| Description is null | Show "Add description..." placeholder |
| Saving description fails | Revert to original, show error toast |
| Adding tag fails | Show error, keep tag in dropdown |
| Removing tag fails | Restore tag to list, show error |

### 4. Tag Photos Page

| Scenario | Handling |
|----------|----------|
| Tag doesn't exist | Show 404 error with back button |
| Tag has no photos | Show empty state with "No photos with this tag" |
| User navigates away and back | Restore scroll position |

---

## Dependencies Between Tasks

```
                    +---------+
                    |   B1.x  |
                    | Backend |
                    +----+----+
                         |
          +--------------+--------------+
          |              |              |
       +--+--+       +---+---+     +----+----+
       |  T1  |       |   T2   |     |   T3    |
       | Unit |       | Unit   |     |  Unit   |
       +------+------+   +-----+     +----+----+
              |              |              |
          +---+--------------+--------------+----+
          |                                        |
     +----+----+                            +-----+-----+
     |  F1.x   |                            |   F2.x    |
     | Front   |                            | Integration|
     +----+----+                            +-----+-----+
          |                                        |
          +--------------+---------------+---------+
                         |
                    +----+----+
                    |   I1-I5 |
                    | Polish  |
                    +---------+
```

---

## Performance Considerations

1. **Tag Counts**: Cache photo counts or use computed property
2. **Photo Grid Tags**: Include tags in photo list API response (avoid N+1)
3. **Tag Autocomplete**: Debounce search input (300ms)
4. **Pagination**: Use existing infinite scroll pattern for tag photos

---

## Unresolved Questions

1. Should tag badges on PhotoGrid be always visible or only on hover?
   - Recommendation: Always visible but subtle, more prominent on hover

2. Should we allow inline tag creation in autocomplete or require modal?
   - Recommendation: Inline creation for smoother UX

3. Should description changes trigger re-upload/processing?
   - No, description is metadata only

4. How should we handle tag deletions when viewing TagPhotos page?
   - Redirect to tags list with toast notification

5. Should we implement optimistic updates for tag operations?
   - Yes, for better UX

---

## File Changes Summary

### Backend Files (14 new/modified)

**New Files (7)**:
- `Application/Features/Tags/Commands/RemoveTagFromPhotoCommand.cs`
- `Application/Features/Tags/Handlers/RemoveTagFromPhotoCommandHandler.cs`
- `Application/Features/Tags/Validators/RemoveTagFromPhotoCommandValidator.cs`
- `Application/Features/Tags/Queries/GetTagsWithPhotoCountQuery.cs`
- `Application/Features/Tags/Handlers/GetTagsWithPhotoCountQueryHandler.cs`
- `Application/Features/Tags/Validators/GetTagsWithPhotoCountQueryValidator.cs`
- `Application/Common/DTOs/TagDTOs.cs` (add TagWithPhotoCountResponse)

**Modified Files (7)**:
- `Application/Features/Tags/Queries/GetTagPhotosQuery.cs` (add pagination)
- `Application/Features/Tags/Handlers/GetTagPhotosQueryHandler.cs` (pagination)
- `Application/Interfaces/ITagRepository.cs` (new methods)
- `Infrastructure/Persistence/Repositories/TagRepository.cs` (implementation)
- `API/Controllers/TagsController.cs` (new endpoints)
- `API/Controllers/PhotosController.cs` (new endpoint)

### Frontend Files (11 new/modified)

**New Files (6)**:
- `src/components/tags/TagChip.tsx`
- `src/components/tags/TagAutocomplete.tsx`
- `src/components/photos/EditableDescription.tsx`
- `src/hooks/useTagPhotos.ts`
- `src/hooks/useRemoveTagFromPhoto.ts`

**Modified Files (5)**:
- `src/types/index.ts` (add photoCount to Tag)
- `src/components/photos/PhotoGrid.tsx` (add tag badges)
- `src/components/lightbox/LightboxInfo.tsx` (editable fields)
- `src/features/tags/TagList.tsx` (show photo count)
- `src/features/tags/TagPhotos.tsx` (full implementation)

### Test Files (14 new)

**Unit Tests (7)**:
- `tests/MyPhotoBooth.UnitTests/Features/Tags/Commands/RemoveTagFromPhotoCommandHandlerTests.cs`
- `tests/MyPhotoBooth.UnitTests/Features/Tags/Commands/RemoveTagFromPhotoCommandValidatorTests.cs`
- `tests/MyPhotoBooth.UnitTests/Features/Tags/Queries/GetTagsWithPhotoCountQueryHandlerTests.cs`
- `tests/MyPhotoBooth.UnitTests/Features/Tags/Queries/GetTagsWithPhotoCountQueryValidatorTests.cs`
- `tests/MyPhotoBooth.UnitTests/Features/Tags/Queries/GetTagPhotosQueryHandlerTests.cs`

**Integration Tests (7)**:
- `tests/MyPhotoBooth.IntegrationTests/Tags/RemoveTagFromPhotoEndpointTests.cs`
- `tests/MyPhotoBooth.IntegrationTests/Tags/GetTagsWithCountEndpointTests.cs`
- `tests/MyPhotoBooth.IntegrationTests/Tags/GetTagPhotosEndpointTests.cs`

---

## Estimated Total Time

| Phase | Time |
|-------|------|
| Backend Development | 4-6 hours |
| Backend Testing (TFD) | 3-4 hours |
| Frontend Components | 4-5 hours |
| Frontend Integration | 5-6 hours |
| Integration & Polish | 3-4 hours |
| **Total** | **19-25 hours** |

---

## Acceptance Criteria

- [ ] Backend: Remove single tag from photo endpoint works
- [ ] Backend: Get tags with photo count returns correct counts
- [ ] Backend: Get tag photos supports pagination
- [ ] Frontend: PhotoGrid shows 2-3 tag badges with "+X more"
- [ ] Frontend: Tag badges are clickable to filter
- [ ] Frontend: `/tags/:id` page shows photos for that tag
- [ ] Frontend: Tags list shows photo count per tag
- [ ] Frontend: Lightbox description is editable with save
- [ ] Frontend: Lightbox tags have X button to remove
- [ ] Frontend: Lightbox has autocomplete to add new tags
- [ ] Tests: 70%+ coverage on new handlers
- [ ] Tests: 100% coverage on new validators
- [ ] Tests: All new endpoints covered by integration tests

---

## Next Steps

Would you like me to:
1. Use @implementer agent to implement this plan
2. Use @brainstormer agent to brainstorm alternative approaches
