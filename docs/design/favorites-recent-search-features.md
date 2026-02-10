# Implementation Plan: Favorites, Recently Added, and Search Photos

**Project**: MyPhotoBooth v1.4.0
**Date**: 2026-02-10
**Status**: Planning

## Executive Summary

This document outlines the implementation plan for three missing features in MyPhotoBooth:
1. **Favorites** - Users can mark photos as favorites and filter to view only favorites
2. **Recently Added** - View photos sorted by upload date (newest first)
3. **Search Photos** - Search photos by filename, description, tags, or album names

All features will follow the existing Clean Architecture + CQRS patterns with MediatR, FluentValidation, and Result<T> error handling.

---

## Current State Analysis

### Existing UI Elements
From `PhotoGallery.tsx` (lines 103-122):
- **Filter Pills** already exist but are non-functional:
  - "All Photos" - Currently active
  - "Favorites" - Star icon, no functionality
  - "Recently Added" - Clock icon, no functionality
- **Search Bar** (lines 73-99) - Client-side only, filters by filename

### Backend Architecture
- **CQRS Pattern**: Commands (writes) and Queries (reads) separated
- **MediatR**: Request/response dispatch via `ISender.Send()`
- **FluentValidation**: Declarative validation for all commands/queries
- **Result<T>**: CSharpFunctionalExtensions for error handling
- **Repository Pattern**: IPhotoRepository with EF Core implementation

### Database Schema
- **Photo** entity exists with: Id, OriginalFileName, Description, UploadedAt, UserId
- **Tag** entity with many-to-many PhotoTag junction
- **Album** entity with many-to-many AlbumPhoto junction
- Indexes: UserId, (UserId, CapturedAt)

### Frontend Architecture
- **TanStack Query**: Server state with useInfiniteQuery for photos
- **Zustand**: Client state (auth, sidebar)
- **React Router v6**: Routing with ProtectedRoute
- **TypeScript**: Full type safety

---

## Feature 1: Favorites

### Database Changes

#### New Entity: FavoritePhoto

```csharp
// Domain/Entities/FavoritePhoto.cs
namespace MyPhotoBooth.Domain.Entities;

public class FavoritePhoto
{
    public Guid Id { get; set; }
    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
```

#### Migration
```bash
dotnet ef migrations add AddFavoritePhotos --project src/MyPhotoBooth.Infrastructure --startup-project src/MyPhotoBooth.API
```

#### AppDbContext Configuration
```csharp
public DbSet<FavoritePhoto> FavoritePhotos => Set<FavoritePhoto>();

builder.Entity<FavoritePhoto>(entity =>
{
    entity.HasKey(fp => fp.Id);
    entity.Property(fp => fp.UserId).IsRequired();

    // One user per photo (prevent duplicates)
    entity.HasIndex(fp => new { fp.UserId, fp.PhotoId }).IsUnique();

    entity.HasIndex(fp => fp.UserId); // For listing all favorites

    entity.HasOne(fp => fp.Photo)
        .WithMany()
        .HasForeignKey(fp => fp.PhotoId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(fp => fp.User)
        .WithMany()
        .HasForeignKey(fp => fp.UserId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

### Backend Implementation

#### Commands

**ToggleFavoritePhotoCommand**
```csharp
// Application/Features/Photos/Commands/ToggleFavoritePhotoCommand.cs
public record ToggleFavoritePhotoCommand(Guid PhotoId, string UserId) : ICommand<bool>;
```

**Handler** (`ToggleFavoritePhotoCommandHandler.cs`):
```csharp
public async Task<Result<bool>> Handle(ToggleFavoritePhotoCommand request, CancellationToken ct)
{
    var existing = await _context.FavoritePhotos
        .FirstOrDefaultAsync(fp => fp.UserId == request.UserId && fp.PhotoId == request.PhotoId, ct);

    if (existing != null)
    {
        _context.FavoritePhotos.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return Result.Success(false); // Removed from favorites
    }
    else
    {
        var favorite = new FavoritePhoto
        {
            Id = Guid.NewGuid(),
            PhotoId = request.PhotoId,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };
        _context.FavoritePhotos.Add(favorite);
        await _context.SaveChangesAsync(ct);
        return Result.Success(true); // Added to favorites
    }
}
```

**Validator** (`ToggleFavoritePhotoCommandValidator.cs`):
```csharp
public ToggleFavoritePhotoCommandValidator()
{
    RuleFor(x => x.PhotoId).NotEmpty();
    RuleFor(x => x.UserId).NotEmpty();
}
```

#### Queries

**GetFavoritePhotosQuery**
```csharp
// Application/Features/Photos/Queries/GetFavoritePhotosQuery.cs
public record GetFavoritePhotosQuery(
    string UserId,
    int Page = 1,
    int PageSize = 50
) : IQuery<PaginatedResult<PhotoListResponse>>;
```

**Handler** (`GetFavoritePhotosQueryHandler.cs`):
```csharp
public async Task<Result<PaginatedResult<PhotoListResponse>>> Handle(
    GetFavoritePhotosQuery request,
    CancellationToken ct)
{
    var skip = (request.Page - 1) * request.PageSize;

    var photosQuery = _context.FavoritePhotos
        .Where(fp => fp.UserId == request.UserId)
        .OrderByDescending(fp => fp.CreatedAt)
        .Select(fp => fp.Photo);

    var photos = await photosQuery
        .Skip(skip)
        .Take(request.PageSize)
        .Select(p => new PhotoListResponse { ... })
        .ToListAsync(ct);

    var totalCount = await _context.FavoritePhotos
        .CountAsync(fp => fp.UserId == request.UserId, ct);

    return Result.Success(PaginatedResult<PhotoListResponse>.Create(...));
}
```

**Validator** (`GetFavoritePhotosQueryValidator.cs`):
```csharp
public GetFavoritePhotosQueryValidator()
{
    RuleFor(x => x.UserId).NotEmpty();
    RuleFor(x => x.Page).GreaterThan(0);
    RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
}
```

#### Controller Endpoint

```csharp
// PhotosController.cs
[HttpPost("{id}/favorite")]
public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken ct)
{
    var command = new ToggleFavoritePhotoCommand(id, GetUserId());
    var result = await _mediator.Send(command, ct);
    return result.ToHttpResponse();
}

[HttpGet("favorites")]
public async Task<IActionResult> GetFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
{
    var query = new GetFavoritePhotosQuery(GetUserId(), page, pageSize);
    var result = await _mediator.Send(query, ct);
    return result.ToHttpResponse();
}
```

#### Repository Method (Optional)
```csharp
// IPhotoRepository.cs
Task<bool> IsFavoriteAsync(Guid photoId, string userId, CancellationToken ct = default);
```

### Frontend Implementation

#### Type Updates

```typescript
// types/index.ts
export interface Photo {
  id: string
  originalFileName: string
  width: number
  height: number
  capturedAt: string | null
  uploadedAt: string
  thumbnailPath: string
  isFavorite?: boolean  // NEW
}
```

#### Hook: useFavorites

```typescript
// hooks/useFavorites.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../lib/api'

export function useFavorites(pageSize = 50) {
  return useInfiniteQuery({
    queryKey: ['photos', 'favorites'],
    queryFn: async ({ pageParam = 1 }) => {
      const { data } = await api.get('/photos/favorites', {
        params: { page: pageParam, pageSize },
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

export function useToggleFavorite() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (photoId: string) => {
      const { data } = await api.post(`/photos/${photoId}/favorite`)
      return data // boolean: true = added, false = removed
    },
    onSuccess: (_, photoId) => {
      // Invalidate all photo queries
      queryClient.invalidateQueries({ queryKey: ['photos'] })
    },
  })
}
```

#### Component Updates

**PhotoGallery.tsx Changes**:
```tsx
const [filterType, setFilterType] = useState<'all' | 'favorites' | 'recent'>('all')
const { data: favoritesData, isLoading: isLoadingFavorites } = useFavorites()

// Conditionally fetch based on filter
const { data, isLoading } = useMemo(() => {
  if (filterType === 'favorites') {
    return { data: favoritesData, isLoading: isLoadingFavorites }
  }
  return { data: allPhotosData, isLoading: isLoadingAllPhotos }
}, [filterType, favoritesData, allPhotosData])

// Filter pills handlers
<button
  onClick={() => setFilterType('favorites')}
  className={filterType === 'favorites' ? 'hero-filter-pill-minimal-active' : 'hero-filter-pill-minimal'}
>
  Favorites
</button>
```

**LightboxActions.tsx Changes**:
```tsx
const toggleFavorite = useToggleFavorite()
const [isTogglingFavorite, setIsTogglingFavorite] = useState(false)

// Add favorite button before download
<ActionButton
  onClick={async () => {
    setIsTogglingFavorite(true)
    try {
      await toggleFavorite.mutateAsync(photo.id)
    } finally {
      setIsTogglingFavorite(false)
    }
  }}
  isLoading={isTogglingFavorite}
  title={photo.isFavorite ? 'Remove from favorites' : 'Add to favorites'}
  variant={photo.isFavorite ? 'active' : 'default'}
  icon={
    <svg className="w-5 h-5" fill={photo.isFavorite ? 'currentColor' : 'none'} viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z" />
    </svg>
  }
/>
```

### Testing Strategy

#### Unit Tests
- `ToggleFavoritePhotoCommandValidatorTests` (5 tests)
- `GetFavoritePhotosQueryValidatorTests` (4 tests)
- `ToggleFavoritePhotoCommandHandlerTests` (6 tests):
  - Add favorite successfully
  - Remove favorite (toggle off)
  - Photo not found
  - Invalid user
- `GetFavoritePhotosQueryHandlerTests` (4 tests):
  - Returns paginated favorites
  - Empty favorites list
  - Pagination works correctly

#### Integration Tests
- `POST /api/photos/{id}/favorite` - Add/remove favorite
- `GET /api/photos/favorites` - List favorites with pagination
- Test with Testcontainers PostgreSQL

---

## Feature 2: Recently Added

### Analysis

**Current Behavior**:
- `GetByUserIdAsync` in `PhotoRepository` already sorts by `UploadedAt` DESC (line 28)
- Timeline feature exists at `/api/photos/timeline` but filters by `CapturedAt`

**Implementation**:
- "Recently Added" filter will use the existing `/api/photos` endpoint with sort by `UploadedAt` DESC
- No database changes needed (index on UploadedAt may be added for performance)

### Database Changes (Performance Optimization)

```csharp
// Add index for recently added queries
entity.HasIndex(p => new { p.UserId, p.UploadedAt });
```

Migration:
```bash
dotnet ef migrations add AddUploadedAtIndex --project src/MyPhotoBooth.Infrastructure --startup-project src/MyPhotoBooth.API
```

### Backend Implementation

#### Query Enhancement

**Update GetPhotosQuery** to support sort option:
```csharp
// Application/Features/Photos/Queries/GetPhotosQuery.cs
public enum PhotoSortOrder
{
    UploadedAtDesc,  // Recently added (default)
    CapturedAtDesc,  // Timeline
    FileNameAsc      // A-Z
}

public record GetPhotosQuery(
    int Page = 1,
    int PageSize = 50,
    Guid? AlbumId = null,
    string? Search = null,
    string? UserId = null,
    PhotoSortOrder SortBy = PhotoSortOrder.UploadedAtDesc  // NEW
) : IQuery<PaginatedResult<PhotoListResponse>>;
```

**Update GetPhotosQueryHandler**:
```csharp
var photosQuery = _context.Photos
    .Where(p => p.UserId == userId)
    .AsQueryable();

// Apply sorting
photosQuery = request.SortBy switch
{
    PhotoSortOrder.CapturedAtDesc => photosQuery.OrderByDescending(p => p.CapturedAt ?? p.UploadedAt),
    PhotoSortOrder.FileNameAsc => photosQuery.OrderBy(p => p.OriginalFileName),
    _ => photosQuery.OrderByDescending(p => p.UploadedAt)
};

var photos = await photosQuery
    .Skip(skip)
    .Take(request.PageSize)
    .Include(p => p.PhotoTags)
    .ThenInclude(pt => pt.Tag)
    .ToListAsync(cancellationToken);
```

**Validator Update**:
```csharp
public GetPhotosQueryValidator()
{
    RuleFor(x => x.Page).GreaterThan(0);
    RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    RuleFor(x => x.SortBy).IsInEnum();
}
```

#### Controller Update

```csharp
// PhotosController.cs
[HttpGet]
public async Task<IActionResult> ListPhotos(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    [FromQuery] PhotoSortOrder sortBy = PhotoSortOrder.UploadedAtDesc,
    CancellationToken ct = default)
{
    var query = new GetPhotosQuery(page, pageSize, null, null, GetUserId(), sortBy);
    var result = await _mediator.Send(query, ct);
    return result.ToHttpResponse();
}
```

### Frontend Implementation

#### Type Updates

```typescript
// types/index.ts
export type PhotoSortOrder = 'recent' | 'timeline' | 'name'

export interface PhotoListFilters {
  sortBy?: PhotoSortOrder
  favorites?: boolean
  search?: string
}
```

#### Hook Updates

```typescript
// hooks/usePhotos.ts
export function usePhotos(filters?: PhotoListFilters, pageSize = 50) {
  return useInfiniteQuery({
    queryKey: ['photos', filters],
    queryFn: async ({ pageParam = 1 }) => {
      const { data } = await api.get<PhotoListResponse>('/photos', {
        params: {
          page: pageParam,
          pageSize,
          sortBy: filters?.sortBy === 'timeline' ? 'capturedAtDesc' :
                  filters?.sortBy === 'name' ? 'fileNameAsc' :
                  'uploadedAtDesc'
        },
      })
      return data
    },
    // ...
  })
}
```

#### Component Updates

**PhotoGallery.tsx**:
```tsx
const [filterType, setFilterType] = useState<'all' | 'favorites' | 'recent'>('all')

// Filter pills
<button
  onClick={() => setFilterType('recent')}
  className={filterType === 'recent' ? 'hero-filter-pill-minimal-active' : 'hero-filter-pill-minimal'}
>
  <svg className="w-4 h-4 mr-1.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
  </svg>
  Recently Added
</button>

// In hook call
const { data } = usePhotos(
  filterType === 'recent' ? { sortBy: 'recent' } : undefined
)
```

### Testing Strategy

#### Unit Tests
- `GetPhotosQueryValidatorTests` - Add sortBy validation tests
- `GetPhotosQueryHandlerTests` - Add sort order tests (3 tests)

---

## Feature 3: Search Photos

### Analysis

**Current Behavior**:
- Client-side search in `PhotoGallery.tsx` (lines 29-35) - only searches filename
- Not efficient for large photo collections
- No server-side search capability

**New Requirements**:
- Search by: filename, description, tag names, album names
- Server-side implementation for performance
- Debounced API calls to avoid excessive requests

### Database Changes

**Performance Considerations**:
- For collections < 10,000 photos: LIKE queries are sufficient
- For larger collections: Consider PostgreSQL full-text search with GIN indexes
- Plan for < 10,000 photos: use LIKE with proper indexes

```csharp
// AppDbContext.cs - Add computed columns for search optimization (optional)
// PostgreSQL expression index can be added in migration
```

### Backend Implementation

#### Query

**SearchPhotosQuery**:
```csharp
// Application/Features/Photos/Queries/SearchPhotosQuery.cs
public record SearchPhotosQuery(
    string SearchTerm,
    string UserId,
    int Page = 1,
    int PageSize = 50
) : IQuery<PaginatedResult<PhotoListResponse>>;
```

**Handler** (`SearchPhotosQueryHandler.cs`):
```csharp
public async Task<Result<PaginatedResult<PhotoListResponse>>> Handle(
    SearchPhotosQuery request,
    CancellationToken ct)
{
    var searchTerm = request.SearchTerm.ToLowerInvariant();
    var skip = (request.Page - 1) * request.PageSize;

    // Search in filename and description
    var photosQuery = _context.Photos
        .Where(p => p.UserId == request.UserId)
        .Where(p => p.OriginalFileName.ToLower().Contains(searchTerm) ||
                   (p.Description != null && p.Description.ToLower().Contains(searchTerm)));

    // Also search by tags
    var photosByTags = await _context.PhotoTags
        .Where(pt => pt.Tag.Name.ToLower().Contains(searchTerm))
        .Where(pt => pt.Photo.UserId == request.UserId)
        .Select(pt => pt.PhotoId)
        .ToListAsync(ct);

    // Also search by albums
    var photosByAlbums = await _context.AlbumPhotos
        .Where(ap => ap.Album.Name.ToLower().Contains(searchTerm))
        .Where(ap => ap.Album.UserId == request.UserId)
        .Select(ap => ap.PhotoId)
        .ToListAsync(ct);

    var allMatchingPhotoIds = photosByTags
        .Concat(photosByAlbums)
        .ToHashSet();

    // Combine queries
    var photos = await photosQuery
        .Where(p => allMatchingPhotoIds.Contains(p.Id) ||
                   p.OriginalFileName.ToLower().Contains(searchTerm) ||
                   (p.Description != null && p.Description.ToLower().Contains(searchTerm)))
        .OrderByDescending(p => p.UploadedAt)
        .Skip(skip)
        .Take(request.PageSize)
        .Select(p => new PhotoListResponse { ... })
        .ToListAsync(cancellationToken);

    var totalCount = await _context.Photos
        .Where(p => p.UserId == request.UserId)
        .Where(p => allMatchingPhotoIds.Contains(p.Id) ||
                   p.OriginalFileName.ToLower().Contains(searchTerm) ||
                   (p.Description != null && p.Description.ToLower().Contains(searchTerm)))
        .CountAsync(cancellationToken);

    return Result.Success(PaginatedResult<PhotoListResponse>.Create(...));
}
```

**Validator** (`SearchPhotosQueryValidator.cs`):
```csharp
public SearchPhotosQueryValidator()
{
    RuleFor(x => x.SearchTerm).NotEmpty().MinimumLength(2).MaximumLength(100);
    RuleFor(x => x.UserId).NotEmpty();
    RuleFor(x => x.Page).GreaterThan(0);
    RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
}
```

#### Controller Endpoint

```csharp
// PhotosController.cs
[HttpGet("search")]
public async Task<IActionResult> SearchPhotos(
    [FromQuery] string q,  // search term
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    CancellationToken ct = default)
{
    var query = new SearchPhotosQuery(q, GetUserId(), page, pageSize);
    var result = await _mediator.Send(query, ct);
    return result.ToHttpResponse();
}
```

### Frontend Implementation

#### Hook: usePhotoSearch

```typescript
// hooks/usePhotoSearch.ts
import { useQuery } from '@tanstack/react-query'
import { useDebounce } from '../hooks/useDebounce'
import api from '../lib/api'

export function usePhotoSearch(searchTerm: string, pageSize = 50) {
  const debouncedSearchTerm = useDebounce(searchTerm, 300)

  return useQuery({
    queryKey: ['photos', 'search', debouncedSearchTerm],
    queryFn: async () => {
      if (!debouncedSearchTerm.trim() || debouncedSearchTerm.length < 2) {
        return { items: [], totalCount: 0, page: 1, pageSize, totalPages: 0 }
      }
      const { data } = await api.get('/photos/search', {
        params: { q: debouncedSearchTerm, page: 1, pageSize },
      })
      return data
    },
    enabled: debouncedSearchTerm.length >= 2,
    staleTime: 30000, // Cache results for 30 seconds
  })
}

// useDebounce hook
export function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState(value)

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value)
    }, delay)

    return () => {
      clearTimeout(handler)
    }
  }, [value, delay])

  return debouncedValue
}
```

#### Component Updates

**PhotoGallery.tsx**:
```tsx
const [searchQuery, setSearchQuery] = useState('')
const { data: searchResults, isLoading: isSearching } = usePhotoSearch(searchQuery)

// Use search results when searching, otherwise use filtered photos
const displayedPhotos = useMemo(() => {
  if (searchQuery.trim().length >= 2) {
    return searchResults?.items ?? []
  }
  return filteredPhotos
}, [searchQuery, searchResults, filteredPhotos])
```

**Enhanced Search Input**:
```tsx
<input
  type="text"
  value={searchQuery}
  onChange={(e) => setSearchQuery(e.target.value)}
  placeholder="Search photos by name, description, tags..."
  className="..."
/>
{isSearching && (
  <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
    <div className="w-4 h-4 border-2 border-gray-400 border-t-primary-600 rounded-full animate-spin" />
  </div>
)}
```

### Testing Strategy

#### Unit Tests
- `SearchPhotosQueryValidatorTests` (6 tests):
  - Empty search term
  - Search term too short (< 2 chars)
  - Search term too long (> 100 chars)
  - Valid search term
- `SearchPhotosQueryHandlerTests` (8 tests):
  - Search by filename
  - Search by description
  - Search by tag name
  - Search by album name
  - Combined search results
  - No results found
  - Pagination works

#### Integration Tests
- `GET /api/photos/search?q=test` - Search endpoint
- Test with various search terms
- Test pagination

---

## Implementation Order & Dependencies

### Phase 1: Infrastructure (Day 1)
1. Add FavoritePhoto entity and migration
2. Update AppDbContext configuration
3. Add database indexes for performance
4. Run migrations

### Phase 2: Backend - Favorites (Day 2)
1. Create ToggleFavoritePhotoCommand + Handler + Validator
2. Create GetFavoritePhotosQuery + Handler + Validator
3. Add controller endpoints
4. Write unit tests (10 tests)
5. Write integration tests (2 tests)

### Phase 3: Backend - Recently Added (Day 3)
1. Add PhotoSortOrder enum to GetPhotosQuery
2. Update GetPhotosQueryHandler
3. Update controller
4. Write unit tests (3 tests)

### Phase 4: Backend - Search (Day 4)
1. Create SearchPhotosQuery + Handler + Validator
2. Add controller endpoint
3. Write unit tests (14 tests)
4. Write integration tests (3 tests)

### Phase 5: Frontend (Days 5-6)
1. Update TypeScript types
2. Create useFavorites hook
3. Create usePhotoSearch hook
4. Create useDebounce hook
5. Update PhotoGallery.tsx
6. Update LightboxActions.tsx
7. Add favorite indicators to PhotoGrid

### Phase 6: Testing & Polish (Day 7)
1. End-to-end testing
2. Performance testing (search with large datasets)
3. UI polish (loading states, empty states)
4. Documentation updates

---

## Performance Considerations

### Database Indexes
```sql
-- New indexes to add
CREATE INDEX IX_Photos_UserId_UploadedAt ON Photos(UserId, UploadedAt DESC);
CREATE INDEX IX_FavoritePhotos_UserId_PhotoId ON FavoritePhotos(UserId, PhotoId);
CREATE INDEX IX_FavoritePhotos_UserId_CreatedAt ON FavoritePhotos(UserId, CreatedAt DESC);
```

### Caching Strategy
- Search results: 30-second staleTime
- Favorites: 5-minute staleTime
- Recently Added: 1-minute staleTime (changes frequently)

### Pagination
- Default page size: 50
- Max page size: 100
- Infinite scroll for UI

### Debouncing
- Search input: 300ms debounce
- Cancel previous requests on new input

---

## Open Questions

1. **Search Scale**: Should we implement PostgreSQL full-text search (GIN indexes) for collections > 10,000 photos?
   - Decision: Start with LIKE queries, add GIN indexes if performance degrades

2. **Favorite Synchronization**: Should favorites sync across devices for the same user?
   - Decision: Yes, stored in database per user

3. **Search Scope**: Should search include EXIF data?
   - Decision: Not in v1.4, consider for future

4. **Bulk Operations**: Should users be able to bulk favorite/unfavorite?
   - Decision: Not in v1.4, consider for future

5. **Search Analytics**: Should we track popular search terms?
   - Decision: Not in v1.4, consider for future

---

## Files to Create

### Backend
```
src/MyPhotoBooth.Domain/Entities/FavoritePhoto.cs
src/MyPhotoBooth.Application/Features/Photos/Commands/ToggleFavoritePhotoCommand.cs
src/MyPhotoBooth.Application/Features/Photos/Commands/ToggleFavoritePhotoCommandHandler.cs
src/MyPhotoBooth.Application/Features/Photos/Commands/ToggleFavoritePhotoCommandValidator.cs
src/MyPhotoBooth.Application/Features/Photos/Queries/GetFavoritePhotosQuery.cs
src/MyPhotoBooth.Application/Features/Photos/Queries/GetFavoritePhotosQueryHandler.cs
src/MyPhotoBooth.Application/Features/Photos/Queries/GetFavoritePhotosQueryValidator.cs
src/MyPhotoBooth.Application/Features/Photos/Queries/SearchPhotosQuery.cs
src/MyPhotoBooth.Application/Features/Photos/Queries/SearchPhotosQueryHandler.cs
src/MyPhotoBooth.Application/Features/Photos/Queries/SearchPhotosQueryValidator.cs
src/MyPhotoBooth.Application/Features/Photos/Queries/GetPhotosQuery.cs (update)
src/MyPhotoBooth.Application/Features/Photos/Queries/GetPhotosQueryHandler.cs (update)
src/MyPhotoBooth.Application/Features/Photos/Queries/GetPhotosQueryValidator.cs (update)
```

### Frontend
```
src/client/src/hooks/useFavorites.ts
src/client/src/hooks/useDebounce.ts
src/client/src/hooks/usePhotoSearch.ts
src/client/src/types/index.ts (update)
src/client/src/features/gallery/PhotoGallery.tsx (update)
src/client/src/components/lightbox/LightboxActions.tsx (update)
src/client/src/components/photos/PhotoGrid.tsx (update)
```

### Tests
```
tests/MyPhotoBooth.UnitTests/Features/Photos/Commands/ToggleFavoritePhotoCommandHandlerTests.cs
tests/MyPhotoBooth.UnitTests/Features/Photos/Commands/ToggleFavoritePhotoCommandValidatorTests.cs
tests/MyPhotoBooth.UnitTests/Features/Photos/Queries/GetFavoritePhotosQueryHandlerTests.cs
tests/MyPhotoBooth.UnitTests/Features/Photos/Queries/GetFavoritePhotosQueryValidatorTests.cs
tests/MyPhotoBooth.UnitTests/Features/Photos/Queries/SearchPhotosQueryHandlerTests.cs
tests/MyPhotoBooth.UnitTests/Features/Photos/Queries/SearchPhotosQueryValidatorTests.cs
tests/MyPhotoBooth.IntegrationTests/Photos/FavoritePhotosEndpointsTests.cs
tests/MyPhotoBooth.IntegrationTests/Photos/SearchPhotosEndpointsTests.cs
```

---

## Success Criteria

- [ ] Users can mark photos as favorites via lightbox action button
- [ ] Favorites filter shows only favorited photos
- [ ] Recently Added filter sorts by UploadDate DESC
- [ ] Search returns results for filename, description, tags, albums
- [ ] Search is debounced (300ms)
- [ ] All new code has unit tests (> 70% coverage)
- [ ] All new endpoints have integration tests
- [ ] UI shows loading states during API calls
- [ ] Empty states display appropriately
- [ ] Performance: < 500ms for search queries on < 10k photos
- [ ] Performance: < 200ms for favorites query

---

## Migration Commands

```bash
# Add FavoritePhotos entity
dotnet ef migrations add AddFavoritePhotos --project src/MyPhotoBooth.Infrastructure --startup-project src/MyPhotoBooth.API

# Add UploadedAt index for performance
dotnet ef migrations add AddUploadedAtIndex --project src/MyPhotoBooth.Infrastructure --startup-project src/MyPhotoBooth.API

# Apply migrations
dotnet ef database update --project src/MyPhotoBooth.API
```

---

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/photos/{id}/favorite` | Toggle favorite status |
| GET | `/api/photos/favorites` | Get user's favorite photos |
| GET | `/api/photos` | Get photos (with sortBy parameter) |
| GET | `/api/photos/search?q={term}` | Search photos |

---

## Test Coverage Targets

| Component | Target Coverage | Tests |
|-----------|----------------|-------|
| Validators | 100% | 15 tests |
| Handlers | 70% | 19 tests |
| API Endpoints | 100% | 5 tests |
| **Total** | **~75%** | **~39 tests** |

---

## Dependencies

### External Packages
No new packages required. Uses existing:
- MediatR 14.0
- FluentValidation 12.1
- CSharpFunctionalExtensions
- Entity Framework Core 10.0

### Internal Dependencies
- Requires existing Photo, Tag, Album entities
- Requires existing IPhotoRepository
- Requires existing authentication infrastructure

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Search performance at scale | High | Add database indexes, consider full-text search for > 10k photos |
| Favorite race condition | Medium | Use unique constraint on (UserId, PhotoId) |
| Search API spam | Low | Implement debouncing on frontend |
| Frontend state sync | Medium | Use TanStack Query invalidation properly |

---

## Future Enhancements (Out of Scope)

1. Smart albums based on AI categorization
2. Advanced search filters (date range, size, dimensions)
3. Search autocomplete/suggestions
4. Face detection and person tagging
5. Map view (location-based search)
6. Advanced favorites (collections, smart collections)
7. Search history
8. Search within favorites

---

**Document Version**: 1.0
**Last Updated**: 2026-02-10
**Author**: Claude (Planning Mode)
