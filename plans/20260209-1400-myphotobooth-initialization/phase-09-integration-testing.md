# Phase 09: Integration & Testing

## Context Links

- [Main Plan](./plan.md)
- [Tech Stack - Testing Frameworks & Code Quality](../../docs/tech-stack.md)
- [React SPA Report - Section 6: Performance Optimization](../reports/260209-researcher-to-initializer-react-spa-architecture-report.md)
- [Photo Management Features Report - Section 3: Performance Strategies](../reports/260209-researcher-to-initializer-photo-management-features-report.md)

---

## Overview

| Field | Value |
|-------|-------|
| Date | 2026-02-09 |
| Priority | High - Quality assurance before deployment |
| Status | pending |
| Progress | 0% |
| Estimated Effort | 8-10 hours |
| Depends On | Phase 04, 05, 06, 07, 08 (All feature phases) |

---

## Key Insights

- Backend testing with xUnit, Moq, and FluentAssertions covers unit and integration tests
- Frontend testing with Vitest and React Testing Library covers component and hook tests
- End-to-end tests validate complete user flows (register, upload, view, organize)
- Performance testing with real photo datasets reveals bottlenecks early
- CORS configuration must be validated for SPA-to-API communication
- Test data seeding enables reproducible integration tests
- Error handling must be tested at every boundary (client-server, server-database, server-filesystem)

---

## Requirements

1. Write backend unit tests for services, handlers, and validators
2. Write backend integration tests for API endpoints
3. Write frontend unit tests for hooks, stores, and utilities
4. Write frontend component tests for critical UI paths
5. Create end-to-end tests for core user flows
6. Perform performance testing with realistic data volumes
7. Validate CORS, authentication flow, and error handling end-to-end
8. Fix bugs and optimize based on testing results

---

## Architecture

### Testing Strategy
```
Backend Tests:
  Unit Tests (xUnit + Moq + FluentAssertions)
    ├── Domain entity validation
    ├── Application service/handler logic
    ├── Token generation and validation
    └── File validation utilities

  Integration Tests (xUnit + WebApplicationFactory)
    ├── API endpoint responses (status codes, payload)
    ├── Authentication flow (register, login, refresh, logout)
    ├── Photo upload and retrieval
    ├── Album and tag CRUD operations
    └── Database operations via EF Core

Frontend Tests:
  Unit Tests (Vitest)
    ├── Zustand store logic
    ├── Utility functions
    ├── File validation
    └── Upload chunking logic

  Component Tests (Vitest + React Testing Library)
    ├── Login/Register forms
    ├── DropZone interactions
    ├── PhotoCard rendering
    ├── FilterBar state changes
    └── Lightbox navigation

  End-to-End Tests (Playwright)
    ├── Full registration + login flow
    ├── Photo upload + gallery display
    ├── Album creation + photo organization
    ├── Timeline navigation
    └── Responsive layout verification
```

### Test Data
```
Seed Data:
  - 2 test users (test@example.com, other@example.com)
  - 50 photos per user with varied EXIF dates
  - 5 albums per user with distributed photos
  - 20 tags per user with varied assignments
  - Sample JPEG/PNG files of different sizes
```

---

## Related Code Files

| File | Action | Purpose |
|------|--------|---------|
| `tests/MyPhotoBooth.Domain.Tests/` | Create | Domain entity tests |
| `tests/MyPhotoBooth.Application.Tests/` | Create | Handler/service tests |
| `tests/MyPhotoBooth.API.Tests/` | Create | Integration tests |
| `tests/MyPhotoBooth.API.Tests/TestWebApplicationFactory.cs` | Create | Test server setup |
| `tests/MyPhotoBooth.API.Tests/TestData/` | Create | Seed data and test images |
| `src/client/src/features/auth/__tests__/` | Create | Auth component tests |
| `src/client/src/features/upload/__tests__/` | Create | Upload component tests |
| `src/client/src/features/gallery/__tests__/` | Create | Gallery component tests |
| `src/client/src/stores/__tests__/` | Create | Zustand store tests |
| `src/client/src/lib/__tests__/` | Create | Utility tests |
| `src/client/vitest.config.ts` | Create | Vitest configuration |
| `e2e/` | Create | Playwright E2E tests |
| `e2e/playwright.config.ts` | Create | Playwright config |

---

## Implementation Steps

1. **Set up backend test infrastructure**
   - Create test projects: MyPhotoBooth.Domain.Tests, MyPhotoBooth.Application.Tests, MyPhotoBooth.API.Tests
   - Install xUnit, Moq, FluentAssertions NuGet packages
   - Create `TestWebApplicationFactory` for integration tests using in-memory database or test PostgreSQL
   - Create test data builders for entities (Photo, Album, Tag)
   - Prepare sample image files for upload tests

2. **Write domain unit tests**
   - Photo entity: validate required fields, computed properties
   - Album entity: validate name length, description constraints
   - Tag entity: validate name normalization
   - ExifData: validate serialization/deserialization
   - RefreshToken: validate IsExpired and IsActive logic

3. **Write application layer unit tests**
   - AuthService: test register (success, duplicate email), login (success, wrong password), refresh (valid, expired, revoked), logout
   - TokenService: test JWT generation contains correct claims, test refresh token generation uniqueness
   - ImageProcessingService: test EXIF extraction, test thumbnail dimensions, test auto-rotation
   - FileValidationService: test valid types pass, invalid types rejected, size limits enforced

4. **Write API integration tests**
   - Auth endpoints: register returns 201, login returns tokens, refresh rotates tokens, logout revokes
   - Photo endpoints: upload returns 201 with metadata, list returns paginated results, get returns details, delete removes files
   - Album endpoints: create returns 201, add photos works, remove photos works, delete cascades correctly
   - Tag endpoints: add tags creates or reuses, search returns matches, delete removes associations
   - Authorization: verify 401 for unauthenticated requests, verify 403 for other user's resources

5. **Set up frontend test infrastructure**
   - Configure Vitest in vitest.config.ts with jsdom environment
   - Set up React Testing Library with custom render wrapper (providers for QueryClient, Router)
   - Create mock API responses for TanStack Query tests
   - Set up MSW (Mock Service Worker) for API mocking in tests

6. **Write frontend unit tests**
   - Auth store: test login sets tokens, logout clears state, persistence works
   - UI store: test view mode changes, grid size changes, persistence
   - Selection store: test toggle, select all, clear
   - Upload store: test add files, remove file, update progress, status transitions
   - File validators: test MIME type checks, size checks, extension checks

7. **Write frontend component tests**
   - LoginPage: test form submission, validation errors, loading state
   - RegisterPage: test form submission, password match validation
   - DropZone: test file drop, invalid file rejection, click-to-browse
   - PhotoCard: test click opens lightbox, selection mode toggle
   - FilterBar: test sort change, view mode change
   - UploadQueue: test progress display, cancel button, retry button

8. **Set up and write E2E tests with Playwright**
   - Configure Playwright with test project (chromium, firefox, webkit)
   - Full registration flow: fill form, submit, verify redirect to gallery
   - Full login flow: fill form, submit, verify gallery access
   - Photo upload flow: navigate to upload, drop file, verify progress, verify in gallery
   - Album flow: create album, add photos, verify album view, remove photos, delete album
   - Timeline flow: navigate, verify grouping by date, scroll through months
   - Responsive: verify layout at mobile (375px), tablet (768px), desktop (1440px) widths

9. **Performance testing**
   - Load test: upload 100 photos, verify gallery performance
   - Measure Time to First Meaningful Paint with photo grid
   - Measure scroll performance with 500+ thumbnails (virtualization verification)
   - Database query performance: measure photo list, timeline, and filtered queries
   - API response times: verify <200ms for list queries, <500ms for uploads
   - Bundle analysis: run `npx vite-bundle-visualizer` to identify large chunks

10. **Bug fixing and optimization**
    - Fix issues discovered during testing
    - Optimize slow queries identified by performance tests
    - Reduce bundle size if any chunks exceed 200KB
    - Address accessibility issues (keyboard navigation, screen reader support)
    - Verify all error states display correctly

---

## Todo List

- [ ] Create backend test projects and install test packages
- [ ] Create TestWebApplicationFactory for integration tests
- [ ] Create test data builders and seed data
- [ ] Write domain entity unit tests
- [ ] Write AuthService unit tests
- [ ] Write TokenService unit tests
- [ ] Write ImageProcessingService unit tests
- [ ] Write FileValidation unit tests
- [ ] Write API integration tests for auth endpoints
- [ ] Write API integration tests for photo endpoints
- [ ] Write API integration tests for album endpoints
- [ ] Write API integration tests for tag endpoints
- [ ] Write authorization tests (401/403 scenarios)
- [ ] Configure Vitest and React Testing Library
- [ ] Set up MSW for API mocking
- [ ] Write Zustand store unit tests (auth, UI, selection, upload)
- [ ] Write file validator unit tests
- [ ] Write LoginPage component tests
- [ ] Write RegisterPage component tests
- [ ] Write DropZone component tests
- [ ] Write PhotoCard component tests
- [ ] Write FilterBar component tests
- [ ] Configure Playwright for E2E tests
- [ ] Write E2E: registration flow
- [ ] Write E2E: login flow
- [ ] Write E2E: photo upload flow
- [ ] Write E2E: album management flow
- [ ] Write E2E: timeline navigation
- [ ] Write E2E: responsive layout verification
- [ ] Run performance tests with 100+ photos
- [ ] Analyze and optimize bundle size
- [ ] Fix all discovered bugs
- [ ] Verify all tests pass in CI-like environment

---

## Success Criteria

- Backend unit tests: 80%+ code coverage on Application and Domain layers
- Backend integration tests: all API endpoints tested with success and error paths
- Frontend unit tests: all Zustand stores and utility functions tested
- Frontend component tests: all critical user interaction paths tested
- E2E tests: core flows (register, login, upload, gallery, albums) pass across browsers
- No critical or high-severity bugs remaining
- API response times: list queries <200ms, upload <500ms for 5MB file
- Gallery scrolling: 60fps with 500+ photos loaded (virtualization verified)
- Bundle size: initial load <500KB gzipped
- All tests pass consistently (no flaky tests)

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Flaky E2E tests | High | Medium | Use proper wait conditions, avoid timing-based assertions |
| Test database state leakage | Medium | Medium | Reset database between tests, use transactions |
| Integration tests too slow | Medium | Low | Use in-memory database for unit tests, real PostgreSQL for integration |
| Mock drift from actual API | Medium | Medium | Generate types from API spec, validate mocks match |
| Low coverage in complex areas | Medium | Medium | Prioritize testing business logic over UI rendering |
| CI environment differences | Low | High | Use Docker for consistent test environment |

---

## Security Considerations

- Test that authentication is enforced on all protected endpoints
- Test that users cannot access or modify other users' resources (authorization tests)
- Test that invalid tokens (expired, malformed, revoked) are properly rejected
- Test file upload validation prevents non-image files from being stored
- Test that SQL injection attempts are handled safely (EF Core parameterization)
- Test that XSS payloads in user input (album names, descriptions, tags) are sanitized
- Verify no secrets or tokens are logged or exposed in error responses

---

## Next Steps

After completing this phase, proceed to:
- [Phase 10: Documentation & Deployment](./phase-10-documentation-deployment.md) - Prepare for production deployment
