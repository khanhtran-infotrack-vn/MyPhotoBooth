# Selection Mode State Management Improvement Plan

**Date:** 2026-02-11
**Status:** Planning
**Priority:** Medium
**Type:** State Management Refactoring

## Executive Summary

The selection mode feature has multiple entry points and inconsistent state management across different views. This plan proposes consolidating the selection state management, improving UX consistency, and handling edge cases like navigation, filter changes, and cross-view selections.

---

## Current State Analysis

### 1. State Management (`selectionStore.ts`)

**Current State:**
- Zustand store with persistence to localStorage
- `isSelectionMode` boolean flag
- `selectedIds` Set<string> for selected photo IDs
- `lastSelection` for undo functionality (5-minute window)
- Methods: `toggleSelectionMode()`, `enterSelectionMode()`, `exitSelectionMode()`, `toggleSelection()`, `selectMultiple()`, `selectAll()`, `clearSelection()`

**Issues Identified:**
1. **Auto-selection mode activation**: `toggleSelection()` automatically sets `isSelectionMode = true` when selecting any item
2. **Auto-exit on empty**: `toggleSelection()` and `deselectMultiple()` automatically set `isSelectionMode = false` when no items selected
3. **Persistence only for undo**: Only `lastSelection` is persisted, not `isSelectionMode` or `selectedIds`
4. **No context awareness**: Store doesn't know which view/route the selection belongs to

### 2. Entry Points for Selection Mode

| Entry Point | Location | Behavior |
|-------------|----------|----------|
| **Select Button** | PhotoGallery toolbar | Explicit toggle via `toggleSelectionMode()` |
| **Long Press** | PhotoGrid (500ms) | Calls `enterSelectionMode()` + `toggleSelection()` |
| **Drag Selection** | PhotoGrid mouse move | Calls `enterSelectionMode()` on first move |
| **Checkbox Click** | PhotoGrid overlay | Calls `toggleSelection()` (auto-enters mode) |
| **Date Group Select** | DateGroupHeader | Calls `selectMultiple()` (auto-enters mode) |

### 3. Views Using Selection Mode

| View | Uses Selection Toggle? | Uses Bulk Actions? | Uses SelectionBar? |
|------|------------------------|-------------------|-------------------|
| PhotoGallery | Yes | Yes | Yes |
| AlbumDetail | No | No | No |
| TagPhotos | No | No | No |

### 4. Component Relationships

```
PhotoGallery (Photos View)
  ├─ SelectionToggleButton ──────┐
  ├─ PhotoGrid                   │
  │   └─ DateGroupHeader ────────┼──> useSelectionStore
  ├─ BulkActionsBar ─────────────┤
  └─ SelectionBar ───────────────┘

AlbumDetail
  └─ PhotoGrid
      └─ DateGroupHeader ───────────> useSelectionStore (selection works but no UI controls!)

TagPhotos
  └─ PhotoGrid
      └─ DateGroupHeader ───────────> useSelectionStore (selection works but no UI controls!)
```

---

## Issues & Edge Cases

### Critical Issues

1. **Orphaned Selection State**
   - User selects photos in PhotoGallery
   - Navigates to AlbumDetail or TagPhotos
   - Selection state persists but no UI to exit/clear it
   - User can click photos in selection mode but has no way to exit

2. **Selection State Persists Across Filter Changes**
   - User selects photos under "All Photos" filter
   - Changes to "Favorites" filter
   - Selected IDs may reference photos not visible in current view
   - No clear visual feedback for "selected but not visible" items

3. **Double Action Bar Problem**
   - Both `SelectionBar` (dark, old style) and `BulkActionsBar` (white, new style) exist
   - PhotoGallery shows BOTH when selection is active
   - Inconsistent UX and potential confusion

4. **Auto-Mode Toggle Conflicts**
   - Clicking a checkbox in PhotoGrid auto-enters selection mode
   - But the Select button shows "Cancel" state
   - User can exit mode via button OR by deselecting all items
   - Two different exit paths = inconsistent UX

### UX Issues

1. **No Visual Feedback for Selection Mode Entry**
   - Long press animation exists but is subtle
   - Drag selection has no "entering selection mode" indicator
   - Checkbox appears on hover, but no clear "selection mode active" indicator

2. **Missing Keyboard Shortcuts**
   - No ESC key to exit selection mode
   - No Cmd+A to select all visible photos
   - No Cmd+D to deselect all

3. **Mobile Touch Considerations**
   - Long press (500ms) may conflict with system gestures
   - No "swipe to select" pattern
   - Touch targets may be small on mobile

### Data Integrity Issues

1. **Stale Selected IDs**
   - Selected photo IDs remain after photos are deleted
   - No validation that selected IDs still exist in current view

2. **Cross-View Selection Leakage**
   - Select photos in PhotoGallery
   - Navigate to AlbumDetail
   - Store still has IDs from PhotoGallery
   - If user somehow selects photos in AlbumDetail, IDs get mixed

---

## Proposed Solution

### Design Principles

1. **Single Source of Truth** - Selection state is global but view-aware
2. **Explicit Mode Control** - Selection mode is only entered explicitly (no auto-enter)
3. **Clear on Context Change** - Clear selection when view/filter/context changes
4. **Consistent UX** - Same selection behavior across all views
5. **Graceful Degradation** - Selection works even without UI controls

### Architecture Changes

#### 1. Enhanced Selection Store

```typescript
interface SelectionState {
  // Current selection
  isSelectionMode: boolean
  selectedIds: Set<string>

  // Context awareness
  currentContext: SelectionContext | null
  allowedContexts: Set<string>  // Which contexts allow selection

  // Actions
  enterSelectionMode: (context?: SelectionContext) => void
  exitSelectionMode: () => void
  toggleSelectionMode: () => void
  toggleSelection: (id: string) => void
  selectMultiple: (ids: string[]) => void
  deselectMultiple: (ids: string[]) => void
  selectAll: (ids: string[]) => void
  clearSelection: () => void

  // Context management
  setContext: (context: SelectionContext) => void
  clearContext: () => void
  isSelectionAllowedInContext: (context: string) => boolean

  // Undo (keep existing)
  lastSelection: LastSelection | null
  saveLastAction: (action: string) => void
  undoLastAction: () => { ids: string[]; canUndo: boolean }
  canUndo: () => boolean
}

interface SelectionContext {
  view: 'gallery' | 'album' | 'tags' | 'shared'
  filter?: 'all' | 'favorites' | 'recent' | 'search'
  entityId?: string  // album ID, tag ID, etc.
}
```

#### 2. Selection Mode Behavior Rules

| Action | Before | After |
|--------|--------|-------|
| Click checkbox when NOT in selection mode | Auto-enters mode | NO OP (or show tooltip) |
| Long press on photo | Enters mode + selects photo | Enters mode + selects photo |
| Drag selection | Enters mode + selects range | Enters mode + selects range |
| Click Select button | Enters mode | Enters mode |
| Deselect all items | Exits mode | Stays in mode (explicit exit required) |
| Navigate away | Keeps selection | Clears selection + exits mode |
| Change filter | Keeps selection | Clears selection + exits mode |
| Delete selected photos | Clears selection | Clears selection + exits mode |

#### 3. Component Refactoring

**Remove Duplicate Components:**
- Deprecate `SelectionBar.tsx` (old dark style)
- Use only `BulkActionsBar.tsx` (new white style)
- Consolidate functionality

**Add Selection Mode Indicator:**
```typescript
// New component: SelectionModeIndicator
// Shows when selection mode is active
// Displays count of selected items
// Shows "exit" button
// Works across all views
```

**Context Provider Pattern:**
```typescript
// Wrap each view with SelectionContext provider
<SelectionContextProvider value={{ view: 'gallery', filter: 'all' }}>
  <PhotoGallery />
</SelectionContextProvider>

<SelectionContextProvider value={{ view: 'album', entityId: albumId }}>
  <AlbumDetail />
</SelectionContextProvider>
```

### Key Changes by File

#### `stores/selectionStore.ts`
1. Add `currentContext: SelectionContext | null`
2. Add `allowedContexts: Set<string>` = `['gallery']` (only PhotoGallery initially)
3. Add `setContext()` and `clearContext()` methods
4. Modify `toggleSelection()` to NOT auto-enter selection mode
5. Modify `clearSelection()` to NOT auto-exit selection mode
6. Add `clearAll()` helper that clears selection AND exits mode
7. Remove persistence (selection should not survive page refresh)

#### `components/photos/PhotoGrid.tsx`
1. Remove auto-enter behavior from checkbox click
2. Keep long press and drag selection as-is (they're explicit user actions)
3. Add keyboard listener for ESC key
4. Add check for `isSelectionAllowedInContext()` before enabling selection

#### `features/gallery/PhotoGallery.tsx`
1. Add `SelectionContextProvider` wrapper
2. Remove `SelectionBar` component
3. Keep only `BulkActionsBar`
4. Clear selection on filter change
5. Clear selection on search query change

#### `features/albums/AlbumDetail.tsx`
1. Add `SelectionContextProvider` wrapper
2. Add `SelectionToggleButton` (reuse from PhotoGallery)
3. Add `BulkActionsBar`
4. Initially disable selection (not in `allowedContexts`)

#### `features/tags/TagPhotos.tsx`
1. Add `SelectionContextProvider` wrapper
2. Add `SelectionToggleButton`
3. Add `BulkActionsBar`
4. Initially disable selection (not in `allowedContexts`)

#### `components/photos/SelectionBar.tsx`
1. **DEPRECATE** - Mark as deprecated
2. Create migration note to use `BulkActionsBar` instead

---

## Implementation Plan

### Phase 1: Foundation (Core Store Changes)

**Tasks:**
1. [ ] Update `SelectionState` interface with context awareness
2. [ ] Add `setContext()`, `clearContext()`, `isSelectionAllowedInContext()` methods
3. [ ] Modify `toggleSelection()` to remove auto-enter behavior
4. [ ] Modify `clearSelection()` to remove auto-exit behavior
5. [ ] Add `clearAll()` method for combined clear + exit
6. [ ] Remove localStorage persistence from selection store

**Estimated Time:** 2-3 hours

### Phase 2: Context Integration

**Tasks:**
1. [ ] Create `SelectionContext` type definition
2. [ ] Create `SelectionContextProvider` component
3. [ ] Wrap PhotoGallery with provider
4. [ ] Wrap AlbumDetail with provider
5. [ ] Wrap TagPhotos with provider
6. [ ] Add context change listeners to clear selection

**Estimated Time:** 2-3 hours

### Phase 3: UI Consistency

**Tasks:**
1. [ ] Deprecate `SelectionBar.tsx`
2. [ ] Create reusable `SelectionToggleButton` component (extract from PhotoGallery)
3. [ ] Add `SelectionToggleButton` to AlbumDetail
4. [ ] Add `SelectionToggleButton` to TagPhotos
5. [ ] Remove duplicate `SelectionBar` from PhotoGallery
6. [ ] Add `BulkActionsBar` to AlbumDetail
7. [ ] Add `BulkActionsBar` to TagPhotos

**Estimated Time:** 2-3 hours

### Phase 4: Behavior Refinement

**Tasks:**
1. [ ] Update PhotoGrid checkbox click (no auto-enter)
2. [ ] Add ESC key handler to exit selection mode
3. [ ] Add keyboard shortcuts (Cmd+A, Cmd+D)
4. [ ] Clear selection on filter change in PhotoGallery
5. [ ] Clear selection on route change
6. [ ] Add validation to remove stale selected IDs

**Estimated Time:** 2-3 hours

### Phase 5: Enable Selection in All Views

**Tasks:**
1. [ ] Add 'album' to `allowedContexts`
2. [ ] Add 'tags' to `allowedContexts`
3. [ ] Test selection in AlbumDetail
4. [ ] Test selection in TagPhotos
5. [ ] Fix any context-specific issues

**Estimated Time:** 1-2 hours

### Phase 6: Polish & Testing

**Tasks:**
1. [ ] Add loading states for bulk operations
2. [ ] Add error handling for failed operations
3. [ ] Add undo/redo for bulk operations
4. [ ] Test mobile touch interactions
5. [ ] Test keyboard shortcuts
6. [ ] Test edge cases (delete while selected, etc.)
7. [ ] Update documentation

**Estimated Time:** 3-4 hours

**Total Estimated Time:** 14-18 hours

---

## Open Questions

1. **Should selection mode be enabled in AlbumDetail and TagPhotos?**
   - Pro: Consistent UX across all photo grid views
   - Con: More complexity, may not be commonly used
   - **Recommendation:** Enable but hide toggle button initially, can be added later

2. **Should selections persist across tab switches?**
   - Current: Yes (Zustand store is global)
   - Proposed: No (clear on context change)
   - **Recommendation:** Clear on context change to avoid confusion

3. **Should we show "selected but not visible" items in bulk action bar?**
   - Example: Select 5 photos, change filter, 2 are no longer visible
   - **Recommendation:** Yes, show count but indicate some are hidden

4. **Should undo work across context changes?**
   - Current: Yes (5-minute window)
   - Proposed: No (clear on context change)
   - **Recommendation:** Keep undo within same context only

---

## Success Criteria

- [ ] Selection mode can only be entered explicitly (button, long press, drag)
- [ ] Selection mode persists even when all items deselected (explicit exit required)
- [ ] Selection clears when changing views/filters
- [ ] ESC key exits selection mode
- [ ] No duplicate action bars shown
- [ ] All photo grid views have consistent selection UX
- [ ] Selected IDs are validated against current view
- [ ] Selection state does not persist across page refreshes

---

## Alternative Approaches Considered

### Alternative 1: Per-View Selection Stores
**Description:** Create separate selection stores for each view (gallery, albums, tags)

**Pros:**
- Complete isolation between views
- Simpler mental model

**Cons:**
- Code duplication
- Can't share selections across views
- More boilerplate

**Verdict:** Not recommended - single store with context awareness is better

### Alternative 2: URL-Based Selection State
**Description:** Store selection state in URL query params

**Pros:**
- Shareable selection state
- Back button works
- Refresh-safe

**Cons:**
- URL clutter for large selections
- Complex encoding/decoding
- Limited URL length

**Verdict:** Not recommended - overkill for this use case

### Alternative 3: Keep Current Auto-Enter Behavior
**Description:** Keep checkbox clicks auto-entering selection mode

**Pros:**
- Fewer clicks for users
- Faster selection workflow

**Cons:**
- Confusing when not expecting selection mode
- Harder to understand current state
- Conflicts with Select button behavior

**Verdict:** Not recommended - explicit is better than implicit

---

## Dependencies

- None - this is a self-contained state management refactor
- Can be implemented incrementally without breaking existing features

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Breaking existing user workflows | Medium | Medium | Phase rollout, test thoroughly |
| Performance issues with large selections | Low | Low | Limit selection count (already 50 for download) |
| Mobile touch gesture conflicts | Medium | Medium | Test on real devices, adjust long press duration |
| State desync across components | Low | High | Use Zustand subscriptions for reactivity |

---

## Related Issues

- None filed yet

---

## References

- Current implementation: `src/client/src/stores/selectionStore.ts`
- PhotoGallery: `src/client/src/features/gallery/PhotoGallery.tsx`
- PhotoGrid: `src/client/src/components/photos/PhotoGrid.tsx`
- BulkActionsBar: `src/client/src/components/bulk/BulkActionsBar.tsx`
- SelectionBar: `src/client/src/components/photos/SelectionBar.tsx` (to be deprecated)
