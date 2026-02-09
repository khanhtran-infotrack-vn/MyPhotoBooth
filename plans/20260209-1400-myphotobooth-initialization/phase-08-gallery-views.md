# Phase 08: Gallery Views

## Context Links

- [Main Plan](./plan.md)
- [Tech Stack - Gallery & Image Components](../../docs/tech-stack.md)
- [React SPA Report - Section 4: Photo Gallery Components](../reports/260209-researcher-to-initializer-react-spa-architecture-report.md)
- [Photo Management Features Report - Sections 2-3](../reports/260209-researcher-to-initializer-photo-management-features-report.md)

---

## Overview

| Field | Value |
|-------|-------|
| Date | 2026-02-09 |
| Priority | High - Primary user-facing feature |
| Status | pending |
| Progress | 0% |
| Estimated Effort | 8-10 hours |
| Depends On | Phase 04 (API), Phase 05 (Albums/Tags API), Phase 06 (Frontend Foundation) |

---

## Key Insights

- Virtualized rendering (TanStack Virtual or react-visual-grid) is essential for handling thousands of photos
- Section-based loading (300-500 photos per section) with unloading of invisible sections prevents browser slowdown
- Lazy loading with `loading="lazy"` and IntersectionObserver reduces initial load time
- Progressive image loading (blur-up technique) improves perceived performance significantly
- Lightbox needs touch gestures, zoom, and keyboard navigation for usability
- Timeline view groups photos by capture date (from EXIF), not upload date
- Newest photos displayed first (standard pattern)

---

## Requirements

1. Build photo grid gallery with virtualized rendering
2. Implement lightbox for full-screen photo viewing
3. Create album list and album detail views
4. Build timeline view with chronological grouping
5. Implement lazy loading and progressive image loading
6. Add photo selection mode for bulk operations
7. Implement sorting, filtering, and search
8. Support responsive layout across devices

---

## Architecture

### Page Components
```
GalleryPage         -> Main photo grid with sorting/filtering
AlbumsListPage      -> Grid of album cards with cover photos
AlbumDetailPage     -> Photos within a specific album
TimelinePage        -> Photos grouped by year/month
```

### Shared Components
```
PhotoGrid           -> Virtualized grid of photo thumbnails
PhotoCard           -> Individual thumbnail with selection overlay
Lightbox            -> Full-screen viewer with navigation
PhotoInfo           -> Metadata panel (EXIF, tags, albums)
FilterBar           -> Sort/filter controls
TagFilter           -> Tag selection for filtering
SearchBar           -> Text search for descriptions
Pagination          -> Page controls or infinite scroll trigger
```

### Data Fetching (TanStack Query)
```typescript
// Query keys factory
const photoKeys = {
  all: ['photos'] as const,
  lists: () => [...photoKeys.all, 'list'] as const,
  list: (filters: PhotoFilters) => [...photoKeys.lists(), filters] as const,
  details: () => [...photoKeys.all, 'detail'] as const,
  detail: (id: string) => [...photoKeys.details(), id] as const,
}

// Hooks
usePhotos(filters)     -> paginated photo list
usePhoto(id)           -> single photo details
useAlbums()            -> album list
useAlbumPhotos(id)     -> photos in album
useTimeline(year, month) -> timeline groups
useTags()              -> tag list for filtering
```

---

## Related Code Files

| File | Action | Purpose |
|------|--------|---------|
| `src/client/src/features/gallery/GalleryPage.tsx` | Create | Main gallery view |
| `src/client/src/features/gallery/PhotoGrid.tsx` | Create | Virtualized photo grid |
| `src/client/src/features/gallery/PhotoCard.tsx` | Create | Thumbnail card |
| `src/client/src/features/gallery/FilterBar.tsx` | Create | Sorting and filtering |
| `src/client/src/features/gallery/hooks/usePhotos.ts` | Create | Photo list query |
| `src/client/src/features/gallery/hooks/usePhoto.ts` | Create | Single photo query |
| `src/client/src/features/lightbox/Lightbox.tsx` | Create | Full-screen viewer |
| `src/client/src/features/lightbox/LightboxControls.tsx` | Create | Navigation/zoom controls |
| `src/client/src/features/lightbox/PhotoInfo.tsx` | Create | Metadata panel |
| `src/client/src/features/albums/AlbumsListPage.tsx` | Create | Album grid |
| `src/client/src/features/albums/AlbumDetailPage.tsx` | Create | Album photo view |
| `src/client/src/features/albums/AlbumCard.tsx` | Create | Album preview card |
| `src/client/src/features/albums/hooks/useAlbums.ts` | Create | Albums query |
| `src/client/src/features/albums/hooks/useAlbumPhotos.ts` | Create | Album photos query |
| `src/client/src/features/gallery/TimelinePage.tsx` | Create | Timeline view |
| `src/client/src/features/gallery/TimelineGroup.tsx` | Create | Year/month group |
| `src/client/src/features/gallery/hooks/useTimeline.ts` | Create | Timeline query |
| `src/client/src/components/TagFilter.tsx` | Create | Tag selection component |
| `src/client/src/components/SearchBar.tsx` | Create | Search input |

---

## Implementation Steps

1. **Create TanStack Query hooks for data fetching**
   - `usePhotos(filters)`: Fetch paginated photos with sorting and filtering
   - `usePhoto(id)`: Fetch single photo with metadata, tags, albums
   - `useAlbums()`: Fetch user's album list with counts and cover photos
   - `useAlbumPhotos(albumId, page)`: Fetch paginated photos in an album
   - `useTimeline(year?, month?)`: Fetch timeline-grouped photos
   - `useTags()`: Fetch tag list with photo counts
   - Configure staleTime and gcTime per query type

2. **Build PhotoGrid with virtualization**
   - Use TanStack Virtual for windowed rendering
   - Calculate grid columns based on container width and thumbnail size
   - Render only visible rows plus small overscan buffer
   - Support variable grid sizes: small (150px), medium (200px), large (300px) thumbnails
   - Implement infinite scroll: load next page when user scrolls near bottom
   - Handle window resize to recalculate grid layout

3. **Build PhotoCard component**
   - Display thumbnail image with `loading="lazy"`
   - Show photo date on hover overlay
   - Selection mode: checkbox overlay, selected state highlight
   - Click: open Lightbox (normal mode) or toggle selection (selection mode)
   - Progressive loading: show low-quality placeholder, fade in thumbnail

4. **Build Lightbox component**
   - Full-screen overlay with dark background
   - Display full-resolution image (fetched on demand)
   - Navigation: left/right arrows, keyboard (arrow keys, Escape)
   - Zoom: scroll wheel, pinch on touch devices
   - Touch gestures: swipe left/right to navigate
   - Info panel: toggle sidebar with EXIF data, tags, description, album membership
   - Actions: download, delete, add to album, edit tags
   - Close: Escape key, click outside, close button
   - Preload adjacent images for smooth navigation

5. **Build GalleryPage**
   - FilterBar at top with: sort options (date, name), order (asc/desc), view mode toggle
   - TagFilter: multi-select tag chips for filtering
   - SearchBar: text search for photo descriptions
   - PhotoGrid rendering results
   - Empty state: "No photos yet" with upload call-to-action
   - Loading state: skeleton grid

6. **Build AlbumsListPage**
   - Grid of AlbumCard components
   - Each card shows: cover photo, album name, photo count, creation date
   - "Create Album" button/card
   - Create album modal: name, description fields
   - Empty state: "No albums yet" with creation prompt

7. **Build AlbumDetailPage**
   - Album header: name, description, photo count, edit/delete buttons
   - PhotoGrid of album photos with custom sort order
   - "Add Photos" button opening a photo picker modal
   - Drag-and-drop reordering within the album
   - Set cover photo option (right-click menu or action)

8. **Build TimelinePage**
   - Grouped by year and month: "February 2026" header with photo count
   - Each group shows PhotoGrid of that month's photos
   - Sticky year headers as user scrolls
   - Navigation sidebar: year/month quick jump
   - Infinite scroll loading older months
   - Empty months skipped

9. **Implement photo selection mode**
   - Toggle selection mode via toolbar button
   - Click photos to select/deselect (checkbox overlay)
   - Shift+click for range selection
   - "Select All" / "Deselect All" buttons
   - Bulk actions toolbar: add to album, add tags, delete
   - Selection count display
   - Store selection state in Zustand

10. **Implement responsive design**
    - Mobile: single column grid, full-width photos, bottom navigation
    - Tablet: 3-4 column grid, collapsible sidebar
    - Desktop: 4-6 column grid, persistent sidebar
    - Lightbox: touch gestures on mobile, keyboard on desktop
    - Adaptive thumbnail sizes based on viewport width

---

## Todo List

- [ ] Create query key factory for consistent cache keys
- [ ] Implement usePhotos hook with pagination and filtering
- [ ] Implement usePhoto hook for single photo details
- [ ] Implement useAlbums hook
- [ ] Implement useAlbumPhotos hook
- [ ] Implement useTimeline hook
- [ ] Implement useTags hook
- [ ] Build PhotoGrid with TanStack Virtual
- [ ] Build PhotoCard with lazy loading and selection
- [ ] Implement progressive image loading (blur-up)
- [ ] Build Lightbox with keyboard and touch navigation
- [ ] Build LightboxControls (zoom, nav, actions)
- [ ] Build PhotoInfo metadata panel
- [ ] Build GalleryPage with FilterBar and SearchBar
- [ ] Build TagFilter multi-select component
- [ ] Build AlbumsListPage with AlbumCard grid
- [ ] Build AlbumDetailPage with album management
- [ ] Implement create album modal
- [ ] Build TimelinePage with grouped layout
- [ ] Build TimelineGroup with sticky headers
- [ ] Implement photo selection mode with bulk actions
- [ ] Implement infinite scroll pagination
- [ ] Add responsive breakpoints for mobile/tablet/desktop
- [ ] Test gallery with 100+ photos for performance
- [ ] Test lightbox navigation and zoom
- [ ] Test album CRUD operations
- [ ] Test tag filtering
- [ ] Test timeline grouping accuracy
- [ ] Test responsive layout on different screen sizes

---

## Success Criteria

- Gallery displays photo thumbnails in a virtualized grid without performance degradation at 500+ photos
- Photos load lazily as user scrolls, with progressive blur-up placeholders
- Lightbox opens on click with full-resolution image, supports keyboard and touch navigation
- Albums display with cover photos and photo counts
- Album detail shows photos with custom sort order
- Timeline groups photos by year/month based on capture date
- Tag filtering narrows gallery results correctly
- Selection mode enables bulk operations (add to album, tag, delete)
- Infinite scroll loads additional pages smoothly
- Layout adapts correctly to mobile, tablet, and desktop viewports
- No visible jank or dropped frames during normal gallery scrolling

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Performance with thousands of photos | High | High | Virtualization, pagination, lazy loading |
| Lightbox image loading delay | Medium | Medium | Preload adjacent images, show spinner |
| Touch gesture conflicts | Medium | Low | Test on real devices, use established gesture library |
| Inconsistent grid layout during loading | Medium | Low | Fixed-size placeholders, skeleton screens |
| Memory leaks from object URLs | Medium | Medium | Proper cleanup in useEffect return |
| Slow timeline queries | Low | Medium | Database indexes on CapturedAt, limit results per group |

---

## Security Considerations

- Photo URLs should be served through authenticated API endpoints, not direct file paths
- Sanitize photo descriptions displayed in the UI
- Prevent XSS through user-generated content (tags, album names, descriptions)
- Validate photo IDs in bulk operations to prevent accessing other users' photos
- Rate limit gallery API requests to prevent scraping

---

## Next Steps

After completing this phase, proceed to:
- [Phase 09: Integration & Testing](./phase-09-integration-testing.md) - End-to-end testing and performance optimization
