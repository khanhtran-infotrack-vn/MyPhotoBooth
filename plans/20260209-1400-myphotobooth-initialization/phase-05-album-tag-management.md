# Phase 05: Album & Tag Management

## Context Links

- [Main Plan](./plan.md)
- [Tech Stack - API Design](../../docs/tech-stack.md)
- [ASP.NET Core API Report - Section 6: API Endpoint Design](../reports/260209-researcher-to-initializer-aspnet-core-api-report.md)
- [Photo Management Features Report - Section 1: Organization Systems](../reports/260209-researcher-to-initializer-photo-management-features-report.md)

---

## Overview

| Field | Value |
|-------|-------|
| Date | 2026-02-09 |
| Priority | High - Key organizational feature |
| Status | pending |
| Progress | 0% |
| Estimated Effort | 4-5 hours |
| Depends On | Phase 02 (Database), Phase 03 (Authentication) |

---

## Key Insights

- Albums use virtual organization: the same photo can belong to multiple albums without file duplication
- Tags provide flat, user-defined categorization with autocomplete from existing tags
- AlbumPhoto junction table includes SortOrder for custom ordering within albums
- Cover photos are optional and reference an existing photo in the album
- Tag names should be unique per user to prevent duplicates
- Autocomplete for tags improves UX and encourages consistent naming

---

## Requirements

1. Implement album CRUD endpoints (create, list, get, update, delete)
2. Implement adding/removing photos from albums
3. Implement album cover photo selection
4. Implement tag CRUD operations
5. Implement adding/removing tags on photos
6. Implement tag search with autocomplete
7. Support custom sort ordering within albums

---

## Architecture

### Album Operations
```
POST   /api/albums                              # Create album
GET    /api/albums                              # List user's albums
GET    /api/albums/{id}                         # Get album with photos
PUT    /api/albums/{id}                         # Update album name/description
DELETE /api/albums/{id}                         # Delete album (not photos)
POST   /api/albums/{id}/photos                  # Add photos to album
DELETE /api/albums/{id}/photos/{photoId}         # Remove photo from album
PUT    /api/albums/{id}/cover/{photoId}          # Set cover photo
PUT    /api/albums/{id}/reorder                  # Reorder photos in album
```

### Tag Operations
```
GET    /api/tags                                 # List user's tags
GET    /api/tags/search?q=vac                    # Search/autocomplete tags
POST   /api/photos/{photoId}/tags                # Add tag(s) to photo
DELETE /api/photos/{photoId}/tags/{tagId}         # Remove tag from photo
GET    /api/photos?tags=vacation,family           # Filter photos by tags
DELETE /api/tags/{id}                            # Delete tag (remove from all photos)
```

### Data Flow
```
Album:
  - Deleting an album removes AlbumPhoto records but NOT the actual photos
  - Photos can exist in 0..N albums simultaneously
  - Each album belongs to exactly one user

Tag:
  - Tags are user-scoped (each user has their own tag namespace)
  - Deleting a tag removes all PhotoTag associations
  - "Get or Create" pattern: adding a tag by name creates it if it doesn't exist
```

---

## Related Code Files

| File | Action | Purpose |
|------|--------|---------|
| `src/MyPhotoBooth.Application/Albums/Commands/CreateAlbumCommand.cs` | Create | Create album |
| `src/MyPhotoBooth.Application/Albums/Commands/UpdateAlbumCommand.cs` | Create | Update album |
| `src/MyPhotoBooth.Application/Albums/Commands/DeleteAlbumCommand.cs` | Create | Delete album |
| `src/MyPhotoBooth.Application/Albums/Commands/AddPhotosToAlbumCommand.cs` | Create | Add photos |
| `src/MyPhotoBooth.Application/Albums/Commands/RemovePhotoFromAlbumCommand.cs` | Create | Remove photo |
| `src/MyPhotoBooth.Application/Albums/Commands/SetCoverPhotoCommand.cs` | Create | Set cover |
| `src/MyPhotoBooth.Application/Albums/Commands/ReorderAlbumPhotosCommand.cs` | Create | Reorder |
| `src/MyPhotoBooth.Application/Albums/Queries/ListAlbumsQuery.cs` | Create | List albums |
| `src/MyPhotoBooth.Application/Albums/Queries/GetAlbumQuery.cs` | Create | Get album detail |
| `src/MyPhotoBooth.Application/Tags/Commands/AddTagsToPhotoCommand.cs` | Create | Tag a photo |
| `src/MyPhotoBooth.Application/Tags/Commands/RemoveTagFromPhotoCommand.cs` | Create | Untag photo |
| `src/MyPhotoBooth.Application/Tags/Commands/DeleteTagCommand.cs` | Create | Delete tag |
| `src/MyPhotoBooth.Application/Tags/Queries/ListTagsQuery.cs` | Create | List tags |
| `src/MyPhotoBooth.Application/Tags/Queries/SearchTagsQuery.cs` | Create | Autocomplete |
| `src/MyPhotoBooth.Application/Common/DTOs/AlbumDTOs.cs` | Create | Album DTOs |
| `src/MyPhotoBooth.Application/Common/DTOs/TagDTOs.cs` | Create | Tag DTOs |
| `src/MyPhotoBooth.API/Controllers/AlbumsController.cs` | Create | Album endpoints |
| `src/MyPhotoBooth.API/Controllers/TagsController.cs` | Create | Tag endpoints |

---

## Implementation Steps

1. **Create album DTOs**
   - `CreateAlbumRequest`: Name (required, max 100 chars), Description (optional, max 500 chars)
   - `UpdateAlbumRequest`: Name, Description
   - `AlbumResponse`: Id, Name, Description, CoverPhotoUrl, PhotoCount, CreatedAt, UpdatedAt
   - `AlbumDetailResponse`: extends AlbumResponse with Photos list (paginated)
   - `AddPhotosRequest`: PhotoIds (list of GUIDs)
   - `ReorderPhotosRequest`: PhotoIds in desired order

2. **Create tag DTOs**
   - `AddTagsRequest`: TagNames (list of strings)
   - `TagResponse`: Id, Name, PhotoCount
   - `TagSearchResponse`: list of TagResponse with matching names

3. **Implement album commands**
   - `CreateAlbumCommand`: Validate name uniqueness per user, create Album entity
   - `UpdateAlbumCommand`: Verify ownership, update properties
   - `DeleteAlbumCommand`: Verify ownership, remove album and AlbumPhoto records (cascade)
   - `AddPhotosToAlbumCommand`: Verify album ownership, verify photo ownership, create AlbumPhoto records with sequential SortOrder
   - `RemovePhotoFromAlbumCommand`: Verify ownership, remove AlbumPhoto record
   - `SetCoverPhotoCommand`: Verify album and photo ownership, update CoverPhotoId
   - `ReorderAlbumPhotosCommand`: Accept ordered list of PhotoIds, update SortOrder values

4. **Implement album queries**
   - `ListAlbumsQuery`: Get all albums for current user, include photo count and cover photo URL, ordered by CreatedAt descending
   - `GetAlbumQuery`: Get album with photos (paginated), ordered by SortOrder, include photo thumbnails

5. **Implement tag commands**
   - `AddTagsToPhotoCommand`: For each tag name, get-or-create tag (per user), create PhotoTag if not exists
   - `RemoveTagFromPhotoCommand`: Verify photo ownership, remove PhotoTag record
   - `DeleteTagCommand`: Verify tag ownership, remove tag and all PhotoTag associations

6. **Implement tag queries**
   - `ListTagsQuery`: Get all tags for current user with photo counts, ordered alphabetically
   - `SearchTagsQuery`: Search tags by prefix (case-insensitive), limit to 10 results for autocomplete

7. **Create AlbumsController**
   - All endpoints require `[Authorize]`
   - Validate ownership in every command (user can only manage their own albums)
   - Return 404 for non-existent resources, 403 for unauthorized access
   - Return 409 if adding a photo that's already in the album

8. **Create TagsController**
   - Endpoint for listing tags with photo counts
   - Search/autocomplete endpoint with query parameter
   - Tag management via PhotosController routes (POST/DELETE on photos/{id}/tags)

9. **Update PhotosController**
   - Add tag filtering to photo list endpoint: `GET /api/photos?tags=vacation,family`
   - Filter uses AND logic (photos must have all specified tags)
   - Include tags in photo detail response

10. **Add integration between albums and photo deletion**
    - When a photo is deleted (Phase 04), also remove from all AlbumPhoto records
    - If deleted photo was a cover photo, set CoverPhotoId to null

---

## Todo List

- [ ] Create album request/response DTOs
- [ ] Create tag request/response DTOs
- [ ] Implement CreateAlbumCommand and handler
- [ ] Implement UpdateAlbumCommand and handler
- [ ] Implement DeleteAlbumCommand and handler
- [ ] Implement AddPhotosToAlbumCommand and handler
- [ ] Implement RemovePhotoFromAlbumCommand and handler
- [ ] Implement SetCoverPhotoCommand and handler
- [ ] Implement ReorderAlbumPhotosCommand and handler
- [ ] Implement ListAlbumsQuery and handler
- [ ] Implement GetAlbumQuery with paginated photos
- [ ] Implement AddTagsToPhotoCommand (get-or-create pattern)
- [ ] Implement RemoveTagFromPhotoCommand
- [ ] Implement DeleteTagCommand
- [ ] Implement ListTagsQuery with photo counts
- [ ] Implement SearchTagsQuery for autocomplete
- [ ] Create AlbumsController with all endpoints
- [ ] Create TagsController
- [ ] Update PhotosController with tag filtering
- [ ] Handle cascading updates on photo deletion
- [ ] Test album CRUD operations
- [ ] Test adding/removing photos from albums
- [ ] Test tag autocomplete search
- [ ] Test tag filtering on photo list
- [ ] Test ownership authorization

---

## Success Criteria

- Albums can be created, listed, updated, and deleted
- Photos can be added to and removed from albums
- Cover photo can be set on an album
- Photos can be reordered within an album
- Tags can be added to photos with autocomplete suggestions
- Tag search returns matching results as the user types
- Photo list can be filtered by one or more tags
- Deleting an album does not delete the photos themselves
- Deleting a photo removes it from all albums and tags
- Only the owner can manage their albums and tags
- Album list includes photo count and cover photo thumbnail

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| N+1 query problem on album listings | High | Medium | Use `.Include()` and projection queries |
| Tag name inconsistency (case variations) | Medium | Low | Normalize tag names to lowercase on creation |
| Large albums slowing down queries | Low | Medium | Paginate photos within albums |
| Orphaned tags after photo deletion | Low | Low | Periodic cleanup or cascade delete in PhotoTag |
| Reorder endpoint with partial list | Medium | Medium | Validate all album photo IDs are provided |

---

## Security Considerations

- Every operation must verify the requesting user owns the album/tag/photo
- Prevent adding another user's photos to your album
- Album names and tag names should be sanitized (strip HTML, limit length)
- Rate limit tag creation to prevent tag spam
- Validate that PhotoIds in batch operations belong to the current user

---

## Next Steps

After completing this phase, proceed to:
- [Phase 08: Gallery Views](./phase-08-gallery-views.md) - Frontend album and tag browsing (after Phase 06)
- [Phase 09: Integration & Testing](./phase-09-integration-testing.md) - Once all backend APIs are complete
