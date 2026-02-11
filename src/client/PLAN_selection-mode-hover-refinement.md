# Plan: Refine Photo Hover Behavior Based on Selection Mode

**Date:** 2026-02-11
**Component:** PhotoGrid.tsx
**Related:** selectionStore.ts

---

## Executive Summary

Refine photo hover effects so they only appear when in selection mode. When not in selection mode, photos should have minimal hover effects (clean, simple appearance). The favorite indicator should remain visible on hover regardless of mode.

---

## Current Behavior Analysis

### Active Hover Effects (Always On)

Located in `/src/client/src/components/photos/PhotoGrid.tsx` lines 253-458:

| Effect | Location | Trigger | Current Behavior |
|--------|----------|---------|------------------|
| Container lift | Line 257 | `isHovered` | `-translate-y-1` class |
| Shadow boost | Line 254 | `isHovered` | `hover:shadow-xl` |
| Photo scale | Lines 315-319 | `isHovered` | `scale-105` on image |
| Gradient overlay | Lines 324-330 | `isHovered` | Black gradient fade-in |
| Checkbox appearance | Lines 349-354 | `isHovered` OR `isSelected` | Shows with scale animation |
| Photo filename | Lines 425-440 | `isHovered` | Text overlay at bottom |
| Shine effect | Lines 443-457 | `isHovered` | Sweeping light animation |
| Favorite indicator | Lines 337-346 | Always (when `isFavorite` true) | Heart icon in corner |

### State Sources

- `isSelectionMode` from `useSelectionStore()` (line 26)
- `hoveredPhotoId` local state (line 27)
- `selectedIds` from `useSelectionStore()` (line 26)

---

## Desired Behavior Design

### When NOT in Selection Mode (`isSelectionMode = false`)

| Element | Behavior |
|---------|----------|
| **Container** | No lift, minimal shadow change |
| **Photo** | No scale (keep `scale-100`) |
| **Gradient overlay** | Hidden |
| **Checkbox** | Hidden completely |
| **Filename** | Hidden |
| **Shine effect** | Hidden |
| **Favorite indicator** | **SHOW on hover** (existing behavior) |

**Result:** Clean, minimal appearance. Only favorite heart appears on hover.

### When IN Selection Mode (`isSelectionMode = true`)

| Element | Behavior |
|---------|----------|
| **Container** | Lift effect (`-translate-y-1`) |
| **Photo** | Scale to `105` |
| **Gradient overlay** | Show |
| **Checkbox** | Show on hover + always show when selected |
| **Filename** | Show on hover |
| **Shine effect** | Show |
| **Favorite indicator** | Show on hover (existing) |

**Result:** Full hover effects, clear selection affordance.

---

## Implementation Approach: Hybrid Strategy

### Rationale

The **Hybrid Approach** is recommended because:

1. **Performance:** CSS-based conditional classes avoid re-renders
2. **Maintainability:** Clear separation with readable conditional classes
3. **Flexibility:** Easy to adjust individual effects
4. **Accessibility:** Favorite indicator stays accessible

### Pattern

```tsx
// Base classes
const containerClasses = [
  "relative group cursor-pointer overflow-hidden rounded-xl shadow-sm",
  // Conditional: shadow boost only in selection mode
  isSelectionMode && isHovered ? "hover:shadow-xl" : "hover:shadow-md",
  // Conditional: lift only in selection mode
  isSelectionMode && isHovered ? "-translate-y-1" : "translate-y-0",
].filter(Boolean).join(" ")
```

---

## Step-by-Step Implementation

### Step 1: Add Selection Mode to Photo Render Function

**File:** `PhotoGrid.tsx`
**Location:** Line 228 (inside `render: { photo: ... }`)

```tsx
// Add isSelectionMode to destructured values for clarity
const { isSelectionMode } = useSelectionStore()
```

Already available at component level - pass to render context via closure.

### Step 2: Refine Container Hover Effects

**File:** `PhotoGrid.tsx`
**Location:** Lines 253-258 (container div)

**Current:**
```tsx
<div
  className={`relative group cursor-pointer overflow-hidden rounded-xl shadow-sm hover:shadow-xl transition-all duration-300 ease-out ${
    isLongPressed ? 'scale-95' : ''
  } ${
    isHovered ? '-translate-y-1' : 'translate-y-0'
  }`}
```

**New:**
```tsx
<div
  className={`relative group cursor-pointer overflow-hidden rounded-xl shadow-sm transition-all duration-300 ease-out ${
    isLongPressed ? 'scale-95' : ''
  } ${
    // Only lift in selection mode
    isSelectionMode && isHovered ? '-translate-y-1' : 'translate-y-0'
  } ${
    // Shadow boost only in selection mode, minimal shadow otherwise
    isSelectionMode && isHovered ? 'shadow-xl' : isHovered ? 'shadow-md' : 'shadow-sm'
  }`}
```

### Step 3: Refine Photo Scale

**File:** `PhotoGrid.tsx`
**Location:** Lines 315-319 (AuthenticatedImage)

**Current:**
```tsx
className={`w-full h-full object-cover ${
  isHovered
    ? 'scale-105 transition-transform duration-500 cubic-bezier(0.34, 1.56, 0.64, 1)'
    : 'scale-100 transition-transform duration-700 cubic-bezier(0.4, 0, 0.2, 1)'
}`}
```

**New:**
```tsx
className={`w-full h-full object-cover ${
  // Only scale in selection mode
  isSelectionMode && isHovered
    ? 'scale-105 transition-transform duration-500 cubic-bezier(0.34, 1.56, 0.64, 1)'
    : 'scale-100 transition-transform duration-700 cubic-bezier(0.4, 0, 0.2, 1)'
}`}
```

### Step 4: Hide Gradient Overlay When Not in Selection Mode

**File:** `PhotoGrid.tsx`
**Location:** Lines 324-330 (gradient overlay div)

**Current:**
```tsx
<div
  className={`absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent ${
    isHovered
      ? 'opacity-100 transition-opacity duration-500 ease-out'
      : 'opacity-0 transition-opacity duration-300 ease-in'
  }`}
/>
```

**New:**
```tsx
{/* Gradient overlay - only in selection mode */}
{isSelectionMode && (
  <div
    className={`absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent ${
      isHovered
        ? 'opacity-100 transition-opacity duration-500 ease-out'
        : 'opacity-0 transition-opacity duration-300 ease-in'
    }`}
  />
)}
```

### Step 5: Hide Filename When Not in Selection Mode

**File:** `PhotoGrid.tsx`
**Location:** Lines 425-440 (photo info overlay)

**Current:**
```tsx
<div
  className={`absolute bottom-0 left-0 right-0 p-3 text-white ${
    isHovered
      ? 'opacity-100 translate-y-0 transition-all duration-300 ease-out'
      : 'opacity-0 translate-y-2 transition-all duration-200 ease-in'
  }`}
  // ... styles ...
>
  <p className="text-sm font-medium truncate drop-shadow-lg">
    {originalPhoto?.originalFileName}
  </p>
</div>
```

**New:**
```tsx
{/* Photo info overlay - only in selection mode */}
{isSelectionMode && (
  <div
    className={`absolute bottom-0 left-0 right-0 p-3 text-white ${
      isHovered
        ? 'opacity-100 translate-y-0 transition-all duration-300 ease-out'
        : 'opacity-0 translate-y-2 transition-all duration-200 ease-in'
    }`}
    style={{
      transitionTimingFunction: isHovered
        ? 'cubic-bezier(0.34, 1.56, 0.64, 1)'
        : 'cubic-bezier(0.4, 0, 1, 1)',
    }}
  >
    <p className="text-sm font-medium truncate drop-shadow-lg">
      {originalPhoto?.originalFileName}
    </p>
  </div>
)}
```

### Step 6: Hide Shine Effect When Not in Selection Mode

**File:** `PhotoGrid.tsx`
**Location:** Lines 443-457 (shine effect div)

**Current:**
```tsx
<div
  className={`absolute inset-0 pointer-events-none ${
    isHovered
      ? 'opacity-100 transition-opacity duration-300 ease-out'
      : 'opacity-0 transition-opacity duration-500 ease-in'
  }`}
  // ... styles ...
/>
```

**New:**
```tsx
{/* Shine effect - only in selection mode */}
{isSelectionMode && (
  <div
    className={`absolute inset-0 pointer-events-none ${
      isHovered
        ? 'opacity-100 transition-opacity duration-300 ease-out'
        : 'opacity-0 transition-opacity duration-500 ease-in'
    }`}
    style={{
      background: 'linear-gradient(105deg, transparent 40%, rgba(255,255,255,0.25) 45%, rgba(255,255,255,0.25) 50%, transparent 55%)',
      backgroundSize: '200% 100%',
      backgroundPosition: isHovered ? '100% 0' : '-100% 0',
      transition: isHovered
        ? 'background-position 0.6s cubic-bezier(0.4, 0, 0.2, 1), opacity 0.3s ease-out'
        : 'background-position 0s linear, opacity 0.5s ease-in',
    }}
  />
)}
```

### Step 7: Modify Checkbox Visibility

**File:** `PhotoGrid.tsx`
**Location:** Lines 348-422 (checkbox div and button)

**Current:**
```tsx
<div
  className={`absolute top-3 left-3 z-10 ${
    isSelected || isHovered
      ? 'opacity-100 scale-100'
      : 'opacity-0 scale-90 group-hover:opacity-100 group-hover:scale-100'
  } transition-all duration-300 cubic-bezier(0.4, 0, 0.2, 1)`}
>
```

**New:**
```tsx
{/* Selection checkbox - only show in selection mode, or when selected */}
{(isSelectionMode || isSelected) && (
  <div
    className={`absolute top-3 left-3 z-10 ${
      isSelected || (isSelectionMode && isHovered)
        ? 'opacity-100 scale-100'
        : 'opacity-0 scale-90'
    } transition-all duration-300 cubic-bezier(0.4, 0, 0.2, 1)`}
  >
    {/* ... button content ... */}
  </div>
)}
```

**Note:** Checkbox shows when:
1. In selection mode AND hovered
2. Photo is selected (regardless of mode, so selected photos show their state)

### Step 8: Verify Favorite Indicator (No Changes)

**File:** `PhotoGrid.tsx`
**Location:** Lines 337-346

The favorite indicator should **remain unchanged** - it already shows on hover as desired:

```tsx
{isFavorite && (
  <div className="absolute top-3 right-3 z-10">
    <div className="w-7 h-7 rounded-lg bg-white/90 backdrop-blur-md shadow-lg flex items-center justify-center transition-all duration-300 ease-out hover:scale-110 hover:bg-white">
      {/* ... heart icon ... */}
    </div>
  </div>
)}
```

---

## Testing Checklist

### Visual Testing

- [ ] **Non-selection mode hover:**
  - [ ] No lift effect on container
  - [ ] No scale on photo
  - [ ] No gradient overlay
  - [ ] No checkbox appears
  - [ ] No filename overlay
  - [ ] No shine effect
  - [ ] **Favorite heart still appears on hover**

- [ ] **Selection mode hover:**
  - [ ] Container lifts (`-translate-y-1`)
  - [ ] Photo scales to 105%
  - [ ] Gradient overlay fades in
  - [ ] Checkbox appears with scale animation
  - [ ] Filename appears at bottom
  - [ ] Shine effect sweeps
  - [ ] Favorite heart appears

- [ ] **Selection mode transitions:**
  - [ ] Entering selection mode: hover effects become active
  - [ ] Exiting selection mode: hover effects deactivate
  - [ ] Already selected photos: checkbox remains visible after exit

### Interaction Testing

- [ ] Long press to enter selection mode still works
- [ ] Drag selection still works
- [ ] Clicking photo in non-selection mode opens lightbox
- [ ] Clicking photo in selection mode toggles selection
- [ ] ESC key exits selection mode
- [ ] Cmd/Ctrl+A selects all

### Edge Cases

- [ ] Rapid mode toggling doesn't break animations
- [ ] Hovering during mode transition is handled gracefully
- [ ] Selected photos maintain visibility of checkbox after mode exit

### Mobile/Touch

- [ ] Hover effects don't interfere with touch interactions
- [ ] Long press still triggers selection mode
- [ ] No hover effects on touch devices (expected behavior)

---

## Performance Considerations

### Re-render Optimization

The `isSelectionMode` state comes from Zustand store. To avoid unnecessary re-renders:

1. **Zustand selector is already optimized** - the component uses `useSelectionStore()` which subscribes to all changes
2. **Consider:** If performance issues arise, use selector:
   ```tsx
   const isSelectionMode = useSelectionStore(state => state.isSelectionMode)
   ```

### CSS vs Conditional Rendering

| Approach | Performance | Trade-off |
|----------|-------------|-----------|
| Conditional classes | Better (no DOM changes) | CSS complexity |
| Conditional rendering | Good (removes DOM nodes) | More re-renders |

**Recommended:** Conditional rendering for large elements (gradient, shine, filename), conditional classes for container effects.

---

## Alternative Approaches Considered

### Approach 1: Pure CSS (Rejected)

Use CSS modifier classes like `.selection-mode` on parent container.

**Pros:** No React re-renders
**Cons:** Complex state synchronization, harder to maintain

### Approach 2: Separate Components (Rejected)

Create `PhotoThumbnail` and `SelectablePhotoThumbnail` components.

**Pros:** Clear separation
**Cons:** Code duplication, complex swapping logic

### Approach 3: Hybrid (Selected)

Conditional classes for container + conditional rendering for overlays.

**Pros:** Best balance of performance and maintainability
**Cons:** Slightly more verbose JSX

---

## File Changes Summary

| File | Lines Changed | Type |
|------|---------------|------|
| `PhotoGrid.tsx` | ~30 lines (lines 253-457) | Modifications |

**No new files required.**

---

## Rollback Plan

If issues arise, revert by:

1. Remove `isSelectionMode &&` conditions
2. Restore original hover classes
3. Git revert to commit before changes

---

## Open Questions

1. **Should selected photos show checkbox in non-selection mode?**
   - **Recommended:** Yes, maintains visual consistency
   - **Alternative:** No, cleaner appearance

2. **Should entering selection mode animate existing hovered photos?**
   - **Current:** Effects apply immediately on next hover
   - **Alternative:** Animate in all photos when mode changes (complex)

3. **Should there be a subtle hover effect in non-selection mode?**
   - **Current:** Minimal shadow change (`shadow-md`)
   - **Alternative:** No hover at all (may feel unresponsive)

---

## Success Criteria

1. Hover effects only active in selection mode
2. Favorite indicator still shows on hover in both modes
3. Smooth transitions when toggling modes
4. No performance degradation
5. Mobile touch interactions unaffected
