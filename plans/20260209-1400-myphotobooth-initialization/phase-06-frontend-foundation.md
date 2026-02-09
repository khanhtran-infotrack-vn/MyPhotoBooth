# Phase 06: Frontend Foundation

## Context Links

- [Main Plan](./plan.md)
- [Tech Stack - Frontend Stack](../../docs/tech-stack.md)
- [React SPA Architecture Report](../reports/260209-researcher-to-initializer-react-spa-architecture-report.md)

---

## Overview

| Field | Value |
|-------|-------|
| Date | 2026-02-09 |
| Priority | High - Foundation for all frontend features |
| Status | pending |
| Progress | 0% |
| Estimated Effort | 5-6 hours |
| Depends On | Phase 01 (Project Setup), Phase 03 (Backend Auth for testing) |

---

## Key Insights

- React Router v6 nested routes simplify protected route patterns with a single auth wrapper
- TanStack Query handles 80% of state (server data) while Zustand handles 20% (UI state)
- Route-based code splitting with React.lazy() keeps initial bundle small
- Suspense boundaries should be nested so parent content stays visible while children load
- Axios interceptors handle JWT token refresh transparently for all API calls
- Custom useAuth hook centralizes authentication logic across the application

---

## Requirements

1. Configure React Router v6 with public and protected route layouts
2. Set up TanStack Query client with default configuration
3. Create Zustand stores for UI state
4. Implement authentication flow (login, register, token management)
5. Build shared layout components (header, navigation, sidebar)
6. Configure Axios with JWT interceptor and base URL
7. Set up route-based code splitting with Suspense
8. Create error boundary components

---

## Architecture

### Route Structure
```
/login              -> LoginPage (public)
/register           -> RegisterPage (public)
/                   -> ProtectedLayout (requires auth)
  /gallery          -> GalleryPage (lazy loaded)
  /albums           -> AlbumsListPage (lazy loaded)
  /albums/:id       -> AlbumDetailPage (lazy loaded)
  /timeline         -> TimelinePage (lazy loaded)
  /upload           -> UploadPage (lazy loaded)
```

### State Architecture
```
TanStack Query (Server State):
  - ["photos", filters]      -> Photo list cache
  - ["photos", id]           -> Single photo details
  - ["albums"]               -> Album list cache
  - ["albums", id]           -> Album detail with photos
  - ["tags"]                 -> Tag list cache
  - ["timeline", year, month] -> Timeline data

Zustand Stores (Client State):
  - useAuthStore             -> token, user, isAuthenticated
  - useUIStore               -> viewMode, gridSize, sortOrder, sidebarOpen
  - useSelectionStore        -> selectedPhotoIds, isSelecting
  - useUploadStore           -> uploadQueue, progress, status
```

### Component Hierarchy
```
<App>
  <QueryClientProvider>
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route path="/gallery" element={<Suspense><GalleryPage /></Suspense>} />
            <Route path="/albums" element={<Suspense><AlbumsPage /></Suspense>} />
            ...
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  </QueryClientProvider>
</App>
```

---

## Related Code Files

| File | Action | Purpose |
|------|--------|---------|
| `src/client/src/App.tsx` | Modify | Root component with providers |
| `src/client/src/main.tsx` | Modify | Entry point |
| `src/client/src/lib/api.ts` | Create | Axios instance with interceptor |
| `src/client/src/lib/queryClient.ts` | Create | TanStack Query client config |
| `src/client/src/stores/authStore.ts` | Create | Authentication state |
| `src/client/src/stores/uiStore.ts` | Create | UI preferences state |
| `src/client/src/stores/selectionStore.ts` | Create | Photo selection state |
| `src/client/src/features/auth/LoginPage.tsx` | Create | Login form |
| `src/client/src/features/auth/RegisterPage.tsx` | Create | Registration form |
| `src/client/src/features/auth/useAuth.ts` | Create | Auth hook |
| `src/client/src/components/ProtectedRoute.tsx` | Create | Route guard |
| `src/client/src/components/AppLayout.tsx` | Create | Main layout shell |
| `src/client/src/components/Header.tsx` | Create | Top navigation |
| `src/client/src/components/Sidebar.tsx` | Create | Side navigation |
| `src/client/src/components/ErrorBoundary.tsx` | Create | Error handling |
| `src/client/src/components/LoadingFallback.tsx` | Create | Suspense fallback |

---

## Implementation Steps

1. **Configure Axios API client**
   - Create Axios instance with baseURL from environment variable
   - Add request interceptor: attach `Authorization: Bearer {token}` from auth store
   - Add response interceptor: on 401, attempt token refresh; if refresh fails, redirect to login
   - Handle network errors gracefully
   - Export typed API methods for each endpoint group

2. **Configure TanStack Query client**
   - Set default staleTime (5 minutes for most data)
   - Set default gcTime (10 minutes)
   - Configure retry logic (3 retries with exponential backoff)
   - Set up query key factory for consistent cache keys
   - Configure default error handler

3. **Create Zustand stores**
   - `useAuthStore`: accessToken, refreshToken, user, isAuthenticated, login(), logout(), setTokens()
   - `useUIStore`: viewMode (grid/list), gridSize (small/medium/large), sortOrder, sidebarOpen, toggleSidebar()
   - `useSelectionStore`: selectedIds Set, isSelecting, toggleSelection(), selectAll(), clearSelection()
   - Persist auth store to localStorage for session persistence
   - Persist UI store to localStorage for preference persistence

4. **Implement authentication flow**
   - `useAuth` hook: wraps authStore with API calls
   - `login(email, password)`: POST /api/auth/login, store tokens, redirect to /gallery
   - `register(email, password, displayName)`: POST /api/auth/register, store tokens, redirect to /gallery
   - `logout()`: POST /api/auth/logout, clear tokens, redirect to /login
   - `refreshToken()`: POST /api/auth/refresh, update tokens (called by Axios interceptor)
   - Check token expiration on app load, refresh if needed

5. **Create LoginPage component**
   - Email and password form fields with validation
   - Submit button with loading state
   - Error message display for invalid credentials
   - Link to registration page
   - Redirect to /gallery if already authenticated

6. **Create RegisterPage component**
   - Email, password, confirm password, display name fields
   - Client-side validation (email format, password strength, match)
   - Submit button with loading state
   - Error message display (duplicate email, validation errors)
   - Link to login page

7. **Create ProtectedRoute component**
   - Check `isAuthenticated` from auth store
   - If not authenticated, redirect to /login with return URL
   - If authenticated, render `<Outlet />` for nested routes
   - Show loading spinner while checking auth state on initial load

8. **Create AppLayout component**
   - Header with logo, navigation links, user menu (profile, logout)
   - Optional sidebar with album list, tag cloud
   - Main content area rendering `<Outlet />`
   - Responsive layout: sidebar collapses on mobile
   - Active route highlighting in navigation

9. **Set up route-based code splitting**
   - Lazy load each page component: `const GalleryPage = lazy(() => import('./features/gallery/GalleryPage'))`
   - Wrap lazy components in `<Suspense fallback={<LoadingFallback />}>`
   - Create skeleton-style loading fallback for perceived performance
   - Add ErrorBoundary around each route for graceful error handling

10. **Create ErrorBoundary component**
    - Catch rendering errors and display friendly error message
    - Include "Try Again" button to reset error state
    - Log errors for debugging
    - Different error displays for network errors vs application errors

---

## Todo List

- [ ] Create Axios API client with base URL configuration
- [ ] Implement JWT request interceptor
- [ ] Implement 401 response interceptor with token refresh
- [ ] Configure TanStack Query client with defaults
- [ ] Create query key factory
- [ ] Create useAuthStore with localStorage persistence
- [ ] Create useUIStore with localStorage persistence
- [ ] Create useSelectionStore
- [ ] Implement useAuth hook with login/register/logout
- [ ] Create LoginPage with form validation
- [ ] Create RegisterPage with form validation
- [ ] Create ProtectedRoute component
- [ ] Create AppLayout with header and sidebar
- [ ] Create Header component with navigation and user menu
- [ ] Create Sidebar component with albums and tags
- [ ] Set up route definitions with lazy loading
- [ ] Create LoadingFallback (skeleton) component
- [ ] Create ErrorBoundary component
- [ ] Wire up all providers in App.tsx (QueryClient, Router)
- [ ] Test login flow end-to-end with backend
- [ ] Test registration flow end-to-end
- [ ] Test token refresh on 401
- [ ] Test protected route redirect
- [ ] Test route-based code splitting loads chunks correctly

---

## Success Criteria

- User can register a new account and be redirected to the gallery
- User can log in with existing credentials and access protected routes
- Unauthenticated users are redirected to /login
- JWT tokens are stored in localStorage and sent with every API request
- Expired tokens trigger automatic refresh without user intervention
- Failed refresh redirects to login
- Each page route loads its own JavaScript chunk (verify in network tab)
- Suspense fallback displays while lazy-loaded components are loading
- Error boundaries catch and display rendering errors gracefully
- Navigation highlights the active route
- Layout is responsive (sidebar collapses on mobile)

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Token stored in localStorage vulnerable to XSS | Medium | High | Sanitize all inputs, use Content Security Policy |
| CORS issues between Vite dev server and API | High | Medium | Configure Vite proxy for development |
| Race condition on token refresh | Medium | Medium | Queue requests during refresh, retry after |
| Bundle size too large | Low | Medium | Code splitting and tree shaking via Vite |
| Zustand store hydration flash | Medium | Low | Check auth state before rendering protected content |

---

## Security Considerations

- Sanitize all user inputs to prevent XSS (React does this by default for JSX)
- Never store sensitive data in Zustand stores beyond tokens and user profile
- Set `Content-Security-Policy` header to restrict script sources
- Use `rel="noopener noreferrer"` on external links
- Frontend route protection is UX-only; all security enforced server-side
- Consider httpOnly cookies as a more secure token storage alternative
- Clear all stored data on logout (localStorage, query cache, stores)
- Validate redirect URLs to prevent open redirect attacks

---

## Next Steps

After completing this phase, proceed to:
- [Phase 07: Photo Upload UI](./phase-07-photo-upload-ui.md) - Build the upload interface with drag-and-drop
- [Phase 08: Gallery Views](./phase-08-gallery-views.md) - Build gallery grid, lightbox, albums, timeline views
