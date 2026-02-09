# React SPA Architecture Report - MyPhotoBooth

**Date**: February 9, 2026
**Project**: MyPhotoBooth - Photo Memories Application
**Focus**: React 18+ SPA architecture for photo gallery with authentication

## Executive Summary

Recommended stack: **Vite + React 18 + React Router v6 + TanStack Query + Zustand** with virtualized galleries and chunked uploads for optimal performance with large photo collections.

## 1. Build Tool Recommendation

**Use Vite** over Create React App for superior performance:
- Cold start: 390ms vs 4.5s (CRA)
- HMR updates: instant vs 5s delays
- Smaller production bundles via Rollup
- Better developer experience for larger applications

**Sources**: [Vite vs CRA Comparison](https://www.hyperlinkinfosystem.com/blog/vite-vs-create-react-app-a-complete-comparison), [Why Vite in 2025](https://dev.to/simplr_sh/why-you-should-stop-using-create-react-app-and-start-using-vite-react-in-2025-4d21)

## 2. State Management Architecture

**Hybrid Approach - TanStack Query + Zustand**:

### TanStack Query (React Query) for Server State (~80% of state)
- Photo fetching, caching, and synchronization
- Album/tag data management
- Built-in cache invalidation and refetching
- Optimistic updates for photo operations

### Zustand for Client State (~20% of state)
- UI state (selected photos, view mode, filters)
- Lightbox open/closed state
- Upload progress tracking
- User preferences (grid size, sort order)

**Rationale**: This separation keeps server data synchronized while maintaining lightweight client state. Zustand has 30%+ YoY growth and appears in 40% of modern projects.

**Sources**: [Zustand + React Query](https://medium.com/@freeyeon96/zustand-react-query-new-state-management-7aad6090af56), [State Management Showdown](https://dev.to/maurya-sachin/state-management-showdown-redux-toolkit-vs-zustand-vs-react-query-p44), [React State in 2025](https://dev.to/cristiansifuentes/react-state-management-in-2025-context-api-vs-zustand-385m)

## 3. Component Architecture

### Route Structure (React Router v6)
```
/login - Public route
/signup - Public route
/ - Protected routes wrapper
  /gallery - Main photo grid
  /albums/:id - Album view
  /timeline - Timeline view
  /upload - Upload interface
```

### Key Patterns
- **Nested routes** for protected content with single auth wrapper
- **Route-based code splitting** with React.lazy() for each major view
- **Custom useAuth hook** centralizing authentication logic
- **Protected route component** for UX-level security (server validates API calls)

**Sources**: [React Router Auth Guide](https://blog.logrocket.com/authentication-react-router-v6/), [Protected Routes Pattern](https://ui.dev/react-router-protected-routes-authentication), [React Router 7 Auth](https://www.robinwieruch.de/react-router-authentication/)

## 4. Photo Gallery Components

### Virtualization for Performance
- Use **react-visual-grid** or **TanStack Virtual** for rendering only visible images
- Handles thousands of photos without performance degradation
- In-built virtualization reduces browser strain

### Layout Options
- Masonry layout for Pinterest-style browsing
- Row/column layouts for organized albums
- Algorithm based on Knuth-Plass line breaking for optimal photo arrangement

### Infinite Scroll
- Block new page loads until previous images fully loaded (isLoadingPageImages flag)
- Load complete pages vs individual images for smoother UX
- Intersection Observer API for scroll detection

### Lightbox Integration
- Modern libraries ship with touch gestures, zoom, smooth transitions
- 7KB gzipped footprint for SSR support
- Mobile and web consistency

**Sources**: [React Visual Grid](https://dev.to/prabhuignoto/how-to-build-image-grids-with-react-visual-grid-10fh), [React Gallery Components 2026](https://reactscript.com/best-image-gallery/), [Infinite Scroll Guide](https://builtin.com/articles/react-infinite-scroll), [React Lightbox Libraries](https://blog.logrocket.com/comparing-the-top-3-react-lightbox-libraries/)

## 5. File Upload Strategy

### Chunked Upload Implementation
- **Chunk size**: 500KB (optimal balance between API efficiency and timeout risk)
- Chunks < 100KB waste bandwidth; > 5MB risk timeouts
- Use **UpChunk** library or custom axios implementation

### Features
- Progress tracking per chunk with percentage calculation
- Pause/resume/retry/cancel capabilities
- Range request headers for proper chunk handling
- Recovery from partial failures

### Image Optimization Pipeline
```
Client Side:
1. Image compression before upload (TinyPNG, Sharp)
2. EXIF data extraction for metadata (date, location)
3. Chunked upload with progress tracking

Server Side:
4. Generate multiple sizes (thumbnail, medium, full)
5. Convert to WebP/AVIF formats
6. Store on CDN with automatic optimization
```

**Sources**: [File Uploads in React](https://agilitycms.com/blog/how-to-handle-file-uploads-in-react-buffering-progress-preview), [UpChunk Library](https://github.com/muxinc/upchunk), [Chunked Uploader Hook](https://github.com/raffidil/react-chunked-uploader)

## 6. Performance Optimization

### Code Splitting with Suspense
- Route-level splitting: lazy load /gallery, /albums, /timeline, /upload
- Component-level: split components > 30-50KB not in first viewport
- Wrap in error boundaries for graceful failure handling
- Provide lightweight fallbacks (spinners for <300ms, skeletons for longer)
- Nest Suspense boundaries so parent content stays visible while child loads

### Image Loading Optimization
- **Native lazy loading**: `loading="lazy"` attribute on <img> tags
- **Modern formats**: WebP/AVIF with JPEG fallback
- **Responsive images**: srcset and sizes for device-appropriate assets
- **CDN integration**: Cloudflare or similar for automatic optimization
- **Progressive loading**: blur-up technique with low-res placeholder

### Critical Best Practices
- Don't create Promises inside render (causes infinite loops)
- Don't declare lazy components inside other components
- Always provide Suspense fallback UI
- Use bundle analyzer to identify split candidates

**Sources**: [React Suspense Tutorial](https://www.codewithseb.com/blog/react-suspense-tutorial-lazy-loading-async-rendering-data-fetching-react-18-19), [React Lazy Loading Guide](https://refine.dev/blog/react-lazy-loading/), [React Image Optimization](https://uploadcare.com/blog/react-image-optimization-techniques/), [Lazy Loading Images](https://transloadit.com/devtips/cdn-fotos/)

## 7. Authentication Strategy

### Implementation
- Store JWT in httpOnly cookies (CSRF protection needed, XSS resistant)
- Alternative: localStorage (requires XSS protection, more convenient)
- Custom useAuth hook with Context provider
- Server-side validation for all API calls (client routes are UX only)

### Security Notes
- Frontend route protection is for UX purposes only
- Implement proper server-side authorization checks
- Use authentication providers with built-in middleware (Auth0, WorkOS)

**Sources**: [Top Auth Solutions 2026](https://workos.com/blog/top-authentication-solutions-react-2026), [Complete React Auth Guide](https://auth0.com/blog/complete-guide-to-react-user-authentication/)

## 8. Recommended Project Structure

```
src/
├── features/
│   ├── auth/           # Login, signup, useAuth hook
│   ├── gallery/        # Photo grid, virtualized list
│   ├── albums/         # Album views and management
│   ├── upload/         # Chunked upload with progress
│   └── lightbox/       # Photo viewer component
├── components/         # Shared UI components
├── lib/                # TanStack Query setup, axios config
├── stores/             # Zustand stores
├── hooks/              # Custom React hooks
└── utils/              # Helper functions
```

## 9. Technology Stack Summary

| Category | Technology | Rationale |
|----------|-----------|-----------|
| Build Tool | Vite | 10x faster dev server, instant HMR |
| Framework | React 18 | Suspense, transitions, concurrent features |
| Routing | React Router v6 | Nested routes, better API |
| Server State | TanStack Query | Caching, sync, optimistic updates |
| Client State | Zustand | Lightweight, hook-based |
| Gallery | react-visual-grid | Built-in virtualization |
| Uploads | UpChunk | Chunked uploads with retry |
| Images | CDN + WebP/AVIF | Automatic optimization |

## Conclusion

This architecture balances performance, developer experience, and scalability for a photo-heavy SPA. The virtualized gallery handles thousands of photos, chunked uploads support large files with resilience, and the hybrid state management keeps the codebase maintainable as features grow.
