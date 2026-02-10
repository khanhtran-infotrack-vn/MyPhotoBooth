# Slideshow Mode - Implementation Plan

## Implementation Decisions (User Choices)

### Background Music: Skipped
- **Decision**: No background music feature
- Music selector and volume controls removed from settings
- Simplified implementation without audio handling

---

## 1. Technical Architecture

### Backend (ASP.NET Core 10)

**No backend changes required** - Slideshow is a frontend-only feature using existing photo endpoints.

**Optional Backend Enhancement (for future):**
```
Application/Features/Photos/Queries/
└── GetSlideshowPhotosQuery.cs  // Optimized query for slideshow
    // Returns photos with minimal data needed for slideshow
```

### Frontend (React + TypeScript)

**New Components:**
```
client/src/components/slideshow/
├── Slideshow.tsx                // Main slideshow component
├── SlideshowControls.tsx        // Play/pause, navigation, settings
├── SlideshowProgress.tsx        // Progress bar/indicator
├── SlideshowSettings.tsx        // Settings panel (timing, effects, etc.)
└── KenBurnsEffect.tsx          // Ken Burns pan/zoom animation
```

**New Hooks:**
```
client/src/hooks/
├── useSlideshow.ts              // Slideshow state management
└── useKeyboardShortcuts.ts      // Keyboard shortcuts handling
```

**New Store (Zustand):**
```
client/src/stores/
└── slideshowStore.ts            // Slideshow preferences persistence
```

**New Types:**
```
client/src/types/
└── slideshow.ts                 // Slideshow-related types
```

## 2. API Design

### No New Endpoints Required

Using existing endpoints:
- `GET /api/photos` - All photos
- `GET /api/photos/favorites` - Favorite photos
- `GET /api/albums/{id}/photos` - Album photos
- `GET /api/photos/search?q=...` - Search results
- `GET /api/photos/{id}/file` - Full resolution image

### Optional Optimization Endpoint (Future)

**Request:**
```csharp
public record GetSlideshowPhotosQuery(
    string Source,      // "all", "favorites", "album", "search", "tag"
    string? SourceId,   // Album ID, tag ID, or search query
    string UserId,
    int MaxPhotos = 500 // Limit for slideshow
) : IQuery<List<SlideshowPhotoDto>>;
```

**Response:**
```csharp
public record SlideshowPhotoDto(
    Guid Id,
    string OriginalFileName,
    DateTime UploadedAt,
    int Width,
    int Height,
    bool IsFavorite
    // Exclude heavy fields like ExifDataJson
);
```

## 3. Frontend Components

### Component Hierarchy

```
PhotoGallery.tsx (or AlbumDetail.tsx, etc.)
├── SlideshowButton (in header/toolbar)
│   └── Opens Slideshow.tsx
└── PhotoGrid.tsx (hidden during slideshow)

Slideshow.tsx (fullscreen modal)
├── SlideshowProgress.tsx (top/bottom progress bar)
├── KenBurnsEffect.tsx (photo display with animation)
├── SlideshowControls.tsx (bottom control bar)
│   ├── Play/Pause button
│   ├── Previous/Next buttons
│   ├── Progress indicator
│   ├── Settings button
│   └── Exit button
└── SlideshowSettings.tsx (settings panel/modal)
    ├── Timing options (3s, 5s, 10s, 15s, custom)
    ├── Shuffle toggle
    ├── Loop toggle
    ├── Ken Burns effect toggle
    ├── Music selector
    └── Fullscreen toggle
```

### Slideshow Component

```typescript
interface SlideshowProps {
  photos: Photo[]
  initialIndex?: number
  source: SlideshowSource
  sourceId?: string
  onClose: () => void
}

type SlideshowSource =
  | 'all'
  | 'favorites'
  | 'album'
  | 'search'
  | 'tag'
```

**State:**
```typescript
interface SlideshowState {
  isPlaying: boolean
  currentIndex: number
  speed: number        // milliseconds
  shuffle: boolean
  loop: boolean
  kenBurns: boolean
  isFullscreen: boolean
  showSettings: boolean
  showProgress: boolean
}
```

### SlideshowControls Component

```typescript
interface SlideshowControlsProps {
  isPlaying: boolean
  hasPrev: boolean
  hasNext: boolean
  currentIndex: number
  totalCount: number
  progress: number  // 0-100
  onPlayPause: () => void
  onPrevious: () => void
  onNext: () => void
  onSettings: () => void
  onExit: () => void
}
```

### KenBurnsEffect Component

```typescript
interface KenBurnsEffectProps {
  imageUrl: string
  isActive: boolean
  duration: number
  direction: 'zoom-in' | 'zoom-out' | 'pan-left' | 'pan-right'
  onComplete?: () => void
}
```

## 4. Implementation Steps

### Phase 1: Core Slideshow

**Step 1.1: Create Slideshow Types**
```typescript
// types/slideshow.ts
export interface SlideshowConfig {
  speed: number          // milliseconds between photos
  shuffle: boolean
  loop: boolean
  kenBurns: boolean
  progressStyle: 'bar' | 'dots' | 'none'
}

export interface SlideshowState {
  config: SlideshowConfig
  isPlaying: boolean
  currentIndex: number
  isFullscreen: boolean
}

export const DEFAULT_SLIDESHOW_CONFIG: SlideshowConfig = {
  speed: 5000,
  shuffle: false,
  loop: true,
  kenBurns: true,
  progressStyle: 'bar'
}
```

**Step 1.2: Create SlideshowStore**
```typescript
// stores/slideshowStore.ts
import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface SlideshowStore extends SlideshowState {
  setConfig: (config: Partial<SlideshowConfig>) => void
  play: () => void
  pause: () => void
  next: () => void
  previous: () => void
  goToIndex: (index: number) => void
  toggleFullscreen: () => void
}

export const useSlideshowStore = create<SlideshowStore>()(
  persist(
    (set, get) => ({
      config: DEFAULT_SLIDESHOW_CONFIG,
      isPlaying: false,
      currentIndex: 0,
      isFullscreen: false,

      setConfig: (config) => set((state) => ({
        config: { ...state.config, ...config }
      })),

      play: () => set({ isPlaying: true }),
      pause: () => set({ isPlaying: false }),

      next: () => set((state) => ({
        currentIndex: (state.currentIndex + 1) % totalPhotos
      })),

      previous: () => set((state) => ({
        currentIndex: (state.currentIndex - 1 + totalPhotos) % totalPhotos
      })),

      goToIndex: (index) => set({ currentIndex: index }),
      toggleFullscreen: () => set((state) => ({
        isFullscreen: !state.isFullscreen
      }))
    }),
    {
      name: 'slideshow-preferences',
      partialize: (state) => ({ config: state.config })
    }
  )
)
```

**Step 1.3: Create Main Slideshow Component**
```typescript
// components/slideshow/Slideshow.tsx
import { useEffect, useState, useCallback, useRef } from 'react'
import { useSlideshowStore } from '../../stores/slideshowStore'
import { SlideshowControls } from './SlideshowControls'
import { SlideshowProgress } from './SlideshowProgress'
import { KenBurnsEffect } from './KenBurnsEffect'
import api from '../../lib/api'

export function Slideshow({
  photos,
  initialIndex = 0,
  source,
  sourceId,
  onClose
}: SlideshowProps) {
  const {
    config,
    isPlaying,
    currentIndex,
    play,
    pause,
    next,
    previous,
    goToIndex
  } = useSlideshowStore()

  const [shuffledPhotos, setShuffledPhotos] = useState<Photo[]>(photos)
  const [imageUrl, setImageUrl] = useState<string | null>(null)
  const [preloadUrl, setPreloadUrl] = useState<string | null>(null)
  const timerRef = useRef<number>()

  // Shuffle photos if enabled
  useEffect(() => {
    if (config.shuffle) {
      const shuffled = [...photos].sort(() => Math.random() - 0.5)
      setShuffledPhotos(shuffled)
    } else {
      setShuffledPhotos(photos)
    }
  }, [photos, config.shuffle])

  // Auto-advance timer
  useEffect(() => {
    if (!isPlaying) {
      clearInterval(timerRef.current)
      return
    }

    timerRef.current = window.setInterval(() => {
      if (currentIndex < shuffledPhotos.length - 1 || config.loop) {
        next()
      } else {
        pause()
      }
    }, config.speed)

    return () => clearInterval(timerRef.current)
  }, [isPlaying, config.speed, config.loop, currentIndex, next, pause])

  // Load current image
  useEffect(() => {
    const currentPhoto = shuffledPhotos[currentIndex]
    if (!currentPhoto) return

    const loadImage = async () => {
      const response = await api.get(`/photos/${currentPhoto.id}/file`, {
        responseType: 'blob'
      })
      const url = URL.createObjectURL(response.data)
      setImageUrl(url)

      // Cleanup previous URL
      return () => URL.revokeObjectURL(url)
    }

    const cleanup = loadImage()

    // Preload next image
    const nextPhoto = shuffledPhotos[currentIndex + 1]
    if (nextPhoto) {
      api.get(`/photos/${nextPhoto.id}/file`, { responseType: 'blob' })
        .then(response => {
          const url = URL.createObjectURL(response.data)
          setPreloadUrl(url)
        })
    }

    return () => {
      cleanup.then(cleanupFn => cleanupFn?.())
    }
  }, [currentIndex, shuffledPhotos])

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      switch (e.key) {
        case ' ':
          e.preventDefault()
          isPlaying ? pause() : play()
          break
        case 'ArrowLeft':
          previous()
          break
        case 'ArrowRight':
          next()
          break
        case 'Escape':
          onClose()
          break
        case 'f':
          toggleFullscreen()
          break
      }
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [isPlaying, play, pause, next, previous, onClose])

  const currentPhoto = shuffledPhotos[currentIndex]
  const progress = ((currentIndex + 1) / shuffledPhotos.length) * 100

  return (
    <div className="fixed inset-0 z-50 bg-black">
      {/* Ken Burns Effect */}
      {imageUrl && currentPhoto && config.kenBurns && (
        <KenBurnsEffect
          imageUrl={imageUrl}
          isActive={isPlaying}
          duration={config.speed}
          direction={currentIndex % 2 === 0 ? 'zoom-in' : 'zoom-out'}
        />
      )}

      {/* Fallback: Static image */}
      {imageUrl && currentPhoto && !config.kenBurns && (
        <img
          src={imageUrl}
          alt={currentPhoto.originalFileName}
          className="w-full h-full object-contain"
        />
      )}

      {/* Progress Bar */}
      {config.progressStyle !== 'none' && (
        <SlideshowProgress
          current={currentIndex + 1}
          total={shuffledPhotos.length}
          progress={progress}
          style={config.progressStyle}
        />
      )}

      {/* Controls */}
      <SlideshowControls
        isPlaying={isPlaying}
        hasPrev={currentIndex > 0}
        hasNext={currentIndex < shuffledPhotos.length - 1}
        currentIndex={currentIndex}
        totalCount={shuffledPhotos.length}
        onPlayPause={() => isPlaying ? pause() : play()}
        onPrevious={previous}
        onNext={next}
        onSettings={() => {/* Open settings */}}
        onExit={onClose}
      />
    </div>
  )
}
```

**Step 1.4: Create SlideshowControls Component**
```typescript
// components/slideshow/SlideshowControls.tsx
export function SlideshowControls({
  isPlaying,
  hasPrev,
  hasNext,
  currentIndex,
  totalCount,
  onPlayPause,
  onPrevious,
  onNext,
  onSettings,
  onExit
}: SlideshowControlsProps) {
  return (
    <div className="fixed bottom-0 left-0 right-0 z-10 px-6 py-4 bg-gradient-to-t from-black/80 to-transparent">
      <div className="flex items-center justify-between max-w-4xl mx-auto">
        {/* Previous Button */}
        <button
          onClick={onPrevious}
          disabled={!hasPrev}
          className="p-3 rounded-full bg-white/10 hover:bg-white/20 disabled:opacity-30 transition-all"
        >
          <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
        </button>

        {/* Play/Pause Button */}
        <button
          onClick={onPlayPause}
          className="p-4 rounded-full bg-white/20 hover:bg-white/30 transition-all"
        >
          {isPlaying ? (
            <svg className="w-8 h-8 text-white" fill="currentColor" viewBox="0 0 24 24">
              <path d="M6 4h4v16H6V4zm8 0h4v16h-4V4z" />
            </svg>
          ) : (
            <svg className="w-8 h-8 text-white" fill="currentColor" viewBox="0 0 24 24">
              <path d="M8 5v14l11-7z" />
            </svg>
          )}
        </button>

        {/* Next Button */}
        <button
          onClick={onNext}
          disabled={!hasNext}
          className="p-3 rounded-full bg-white/10 hover:bg-white/20 disabled:opacity-30 transition-all"
        >
          <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
          </svg>
        </button>

        {/* Counter */}
        <div className="px-4 py-2 rounded-full bg-white/10">
          <span className="text-white font-medium">
            {currentIndex + 1} / {totalCount}
          </span>
        </div>

        {/* Settings Button */}
        <button
          onClick={onSettings}
          className="p-3 rounded-full bg-white/10 hover:bg-white/20 transition-all"
          title="Settings"
        >
          <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </button>

        {/* Exit Button */}
        <button
          onClick={onExit}
          className="p-3 rounded-full bg-white/10 hover:bg-white/20 transition-all"
          title="Exit (Esc)"
        >
          <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
    </div>
  )
}
```

**Step 1.5: Create KenBurnsEffect Component**
```typescript
// components/slideshow/KenBurnsEffect.tsx
import { useEffect, useState } from 'react'

export function KenBurnsEffect({
  imageUrl,
  isActive,
  duration,
  direction
}: KenBurnsEffectProps) {
  const [scale, setScale] = useState(1)
  const [translate, setTranslate] = useState({ x: 0, y: 0 })

  useEffect(() => {
    if (!isActive) return

    // Reset and start animation
    setScale(direction === 'zoom-in' ? 1 : 1.1)
    setTranslate({ x: 0, y: 0 })

    const startTime = Date.now()

    const animate = () => {
      const elapsed = Date.now() - startTime
      const progress = Math.min(elapsed / duration, 1)

      if (direction === 'zoom-in') {
        setScale(1 + progress * 0.1)
        setTranslate({ x: progress * 20, y: progress * 10 })
      } else {
        setScale(1.1 - progress * 0.1)
        setTranslate({ x: 20 - progress * 20, y: 10 - progress * 10 })
      }

      if (progress < 1) {
        requestAnimationFrame(animate)
      }
    }

    requestAnimationFrame(animate)
  }, [isActive, duration, direction])

  return (
    <div className="absolute inset-0 overflow-hidden">
      <img
        src={imageUrl}
        alt=""
        className="absolute inset-0 w-full h-full object-cover"
        style={{
          transform: `scale(${scale}) translate(${translate.x}px, ${translate.y}px)`,
          transition: 'transform 0.1s linear'
        }}
      />
    </div>
  )
}
```

**Step 1.6: Create SlideshowProgress Component**
```typescript
// components/slideshow/SlideshowProgress.tsx
export function SlideshowProgress({
  current,
  total,
  progress,
  style
}: SlideshowProgressProps) {
  if (style === 'dots') {
    return (
      <div className="fixed top-6 left-1/2 -translate-x-1/2 z-10 flex gap-2">
        {Array.from({ length: total }).map((_, i) => (
          <button
            key={i}
            onClick={() => goToIndex(i)}
            className={`w-2 h-2 rounded-full transition-all ${
              i === current ? 'bg-white scale-125' : 'bg-white/30 hover:bg-white/50'
            }`}
          />
        ))}
      </div>
    )
  }

  return (
    <div className="fixed top-0 left-0 right-0 z-10 h-1 bg-white/10">
      <div
        className="h-full bg-white transition-all duration-300"
        style={{ width: `${progress}%` }}
      />
    </div>
  )
}
```

### Phase 2: Settings & Preferences

**Step 2.1: Create SlideshowSettings Component**
```typescript
// components/slideshow/SlideshowSettings.tsx
export function SlideshowSettings() {
  const { config, setConfig } = useSlideshowStore()

  return (
    <div className="fixed inset-0 z-20 flex items-center justify-center bg-black/60">
      <div className="bg-gray-900 rounded-2xl p-6 w-full max-w-md shadow-2xl">
        <h2 className="text-xl font-bold text-white mb-6">Slideshow Settings</h2>

        {/* Timing */}
        <div className="mb-6">
          <label className="text-sm font-medium text-gray-300 mb-2 block">
            Photo Duration
          </label>
          <div className="grid grid-cols-4 gap-2">
            {[3, 5, 10, 15].map(seconds => (
              <button
                key={seconds}
                onClick={() => setConfig({ speed: seconds * 1000 })}
                className={`py-2 px-4 rounded-lg text-sm font-medium transition-all ${
                  config.speed === seconds * 1000
                    ? 'bg-primary-600 text-white'
                    : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
                }`}
              >
                {seconds}s
              </button>
            ))}
          </div>
        </div>

        {/* Shuffle */}
        <div className="flex items-center justify-between mb-4">
          <span className="text-sm font-medium text-gray-300">Shuffle Photos</span>
          <button
            onClick={() => setConfig({ shuffle: !config.shuffle })}
            className={`w-12 h-6 rounded-full transition-all ${
              config.shuffle ? 'bg-primary-600' : 'bg-gray-700'
            }`}
          >
            <div className={`w-5 h-5 rounded-full bg-white transition-transform ${
              config.shuffle ? 'translate-x-6' : 'translate-x-0.5'
            }`} />
          </button>
        </div>

        {/* Loop */}
        <div className="flex items-center justify-between mb-4">
          <span className="text-sm font-medium text-gray-300">Loop at End</span>
          <button
            onClick={() => setConfig({ loop: !config.loop })}
            className={`w-12 h-6 rounded-full transition-all ${
              config.loop ? 'bg-primary-600' : 'bg-gray-700'
            }`}
          >
            <div className={`w-5 h-5 rounded-full bg-white transition-transform ${
              config.loop ? 'translate-x-6' : 'translate-x-0.5'
            }`} />
          </button>
        </div>

        {/* Ken Burns */}
        <div className="flex items-center justify-between mb-6">
          <span className="text-sm font-medium text-gray-300">Ken Burns Effect</span>
          <button
            onClick={() => setConfig({ kenBurns: !config.kenBurns })}
            className={`w-12 h-6 rounded-full transition-all ${
              config.kenBurns ? 'bg-primary-600' : 'bg-gray-700'
            }`}
          >
            <div className={`w-5 h-5 rounded-full bg-white transition-transform ${
              config.kenBurns ? 'translate-x-6' : 'translate-x-0.5'
            }`} />
          </button>
        </div>

        {/* Progress Style (Last Setting) */}
        <div className="mb-6">
          <label className="text-sm font-medium text-gray-300 mb-2 block">
            Progress Indicator
          </label>
          <select
            value={config.progressStyle}
            onChange={(e) => setConfig({ progressStyle: e.target.value as any })}
            className="w-full bg-gray-800 text-white rounded-lg px-4 py-2"
          >
            <option value="bar">Progress Bar</option>
            <option value="dots">Dots</option>
            <option value="none">None</option>
          </select>
        </div>

        {/* Close */}
        <button
          onClick={() => {/* Close settings */}}
          className="w-full py-3 bg-primary-600 hover:bg-primary-700 text-white font-medium rounded-lg transition-all"
        >
          Done
        </button>
      </div>
    </div>
  )
}
```

### Phase 3: Integration

**Step 3.1: Add Slideshow Button to PhotoGallery**
```typescript
// features/gallery/PhotoGallery.tsx

const [slideshowOpen, setSlideshowOpen] = useState(false)
const [slideshowIndex, setSlideshowIndex] = useState(0)

// In header, add slideshow button
<button
  onClick={() => {
    setSlideshowIndex(0)
    setSlideshowOpen(true)
  }}
  className="p-2 rounded-full bg-primary-100 text-primary-600 hover:bg-primary-200 transition-all"
  title="Start Slideshow"
>
  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
  </svg>
</button>

// At bottom, add slideshow modal
{slideshowOpen && (
  <Slideshow
    photos={photos}
    initialIndex={slideshowIndex}
    source={filterType === 'favorites' ? 'favorites' : 'all'}
    onClose={() => setSlideshowOpen(false)}
  />
)}
```

**Step 3.2: Add to Album Detail Page**
```typescript
// features/albums/AlbumDetail.tsx

// Similar integration, passing source="album" and sourceId={albumId}
```

**Step 3.3: Add Keyboard Shortcuts Hint**
```typescript
// Show tooltip on hover
<KeyboardShortcutTooltip shortcuts={[
  { key: 'Space', action: 'Play/Pause' },
  { key: '← →', action: 'Previous/Next' },
  { key: 'Esc', action: 'Exit' },
  { key: 'F', action: 'Fullscreen' }
]} />
```

### Phase 4: Advanced Features

**NOTE: Background music feature SKIPPED per user decision**

**Step 4.1: Fullscreen API**
```typescript
// hooks/useFullscreen.ts
export function useFullscreen() {
  const [isFullscreen, setIsFullscreen] = useState(false)

  const enter = () => {
    document.documentElement.requestFullscreen()
  }

  const exit = () => {
    document.exitFullscreen()
  }

  const toggle = () => {
    isFullscreen ? exit() : enter()
  }

  useEffect(() => {
    const handleChange = () => {
      setIsFullscreen(!!document.fullscreenElement)
    }

    document.addEventListener('fullscreenchange', handleChange)
    return () => document.removeEventListener('fullscreenchange', handleChange)
  }, [])

  return { isFullscreen, enter, exit, toggle }
}
```

**Step 4.3: Photo Preloading**
```typescript
// Preload next 3 photos for smooth transitions
useEffect(() => {
  const preloadIndices = [
    currentIndex + 1,
    currentIndex + 2,
    currentIndex + 3
  ].filter(i => i < shuffledPhotos.length)

  preloadIndices.forEach(index => {
    const photo = shuffledPhotos[index]
    if (photo) {
      const img = new Image()
      img.src = getPhotoUrl(photo.id)
    }
  })
}, [currentIndex, shuffledPhotos])
```

## 5. Edge Cases & Considerations

### Empty Photo List
- **Problem**: User starts slideshow with no photos
- **Solution**: Disable slideshow button, show toast message

### Single Photo
- **Problem**: Slideshow with only one photo
- **Solution**: Hide prev/next buttons, auto-pause after first photo

### Network Failures
- **Problem**: Image fails to load during slideshow
- **Solution**: Show placeholder, skip to next photo after 2 seconds

### Large Photos
- **Problem**: High-res photos take time to load
- **Solution**: Preload next photo, show loading spinner

### Battery Drain (Mobile)
- **Problem**: Continuous playback drains battery
- **Solution**: Show battery warning after 15 minutes, suggest pausing

### Screen Wake Lock
- **Problem**: Device sleeps during slideshow
- **Solution**: Use Screen Wake Lock API
```typescript
const wakeLock = await navigator.wakeLock.request('screen')
```

### Memory Leaks
- **Problem**: Blob URLs not revoked
- **Solution**: Cleanup in useEffect return, revoke all URLs on unmount

### Rapid Navigation
- **Problem**: User clicks next/prev rapidly
- **Solution**: Debounce navigation, cancel pending image loads

### Window Resize
- **Problem**: Image aspect ratio changes on resize
- **Solution**: Use object-contain CSS, recalculate on resize

### Accessibility
- **Problem**: Keyboard navigation conflicts
- **Solution**: Check if user is typing in input before handling shortcuts

## 6. Testing Strategy

### Unit Tests

**SlideshowStore:**
- Initial state matches defaults
- setConfig updates specific config values
- play/pause toggles isPlaying
- next/previous increments/decrements currentIndex
- goToIndex sets specific index
- Loop behavior at end of photos

**KenBurnsEffect:**
- Renders image with correct scale
- Animates from scale 1 to 1.1 over duration
- Resets animation on re-mount
- Cleans up animation on unmount

**SlideshowProgress:**
- Shows correct progress percentage
- Renders correct number of dots
- Highlights current dot
- Clicking dot navigates to that photo

### Integration Tests

**Slideshow Component:**
- Starts at initialIndex
- Auto-advances when playing
- Stops auto-advance when paused
- Handles keyboard shortcuts
- Loads images correctly
- Preloads next image
- Handles shuffle mode

### E2E Tests

**User Flow:**
1. User opens slideshow from gallery
2. Slideshow starts automatically
3. User pauses with Space key
4. User navigates with arrow keys
5. User opens settings and changes speed
6. User exits with Esc key
7. Slideshow closes and returns to gallery

**Different Sources:**
- All Photos slideshow
- Favorites slideshow
- Album slideshow
- Search results slideshow
- Tag photos slideshow

## 7. Performance Considerations

### Frontend
- Use requestAnimationFrame for smooth animations
- Debounce resize handlers
- Preload images efficiently (max 3 ahead)
- Revoke blob URLs to prevent memory leaks
- Use CSS transforms instead of layout changes
- Virtualize for very large photo sets (500+)

### Mobile
- Optimize images for mobile display
- Reduce animation complexity on low-end devices
- Respect reduced motion preference
```typescript
const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches
```

## 8. Accessibility

**Keyboard Navigation:**
- Space: Play/Pause
- Arrow Left/Right: Previous/Next
- Escape: Exit
- F: Toggle fullscreen
- S: Open settings

**Screen Reader:**
- Announce current photo filename
- Announce playback state changes
- Label all controls
- Focus trap in slideshow modal

**Visual:**
- High contrast controls
- Large touch targets (44px min)
- Visible focus indicators
- Respect prefers-reduced-motion

## 9. User Experience Flow

```
1. User clicks "Slideshow" button in PhotoGallery
2. Slideshow opens in fullscreen
3. Photos auto-advance (if not paused)
4. Progress bar shows current position
5. User can:
   - Pause/play with Space bar or button
   - Navigate with arrow keys or buttons
   - Adjust settings (speed, shuffle, effects)
   - Exit with Escape or button
6. On exit, returns to previous view
```

## 10. Future Enhancements

### Transitions
- Fade transition
- Slide transition (left/right, up/down)
- Zoom transition
- Blur transition
- Custom transition pack

### Photo Information
- Show EXIF data overlay
- Show location on map
- Show tags and albums
- Show capture date/time

### Interactivity
- Click photo to pause
- Double click to like
- Swipe gestures on mobile
- Pinch to zoom

### Sharing
- Share slideshow link
- Export slideshow as video
- Cast to Chromecast/TV
- AirPlay support

### AI Features
- Auto-select best photos
- Face-only slideshow
- Scene-based slideshow
- Mood-based slideshow (happy, scenic, etc.)

### Collaborative
- Group slideshow mode
- Live sync across devices
- Vote on photos
- Comment during slideshow
