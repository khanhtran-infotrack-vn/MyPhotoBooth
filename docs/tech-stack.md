# MyPhotoBooth Tech Stack

**Project**: Photo memories application for storing and viewing photos with friends and family
**Architecture**: React SPA frontend + ASP.NET Core Web API backend
**Last Updated**: 2026-02-09

---

## Frontend Stack

### Core Framework
- **React 18**: Leverages Suspense, transitions, and concurrent features for optimal performance
- **Vite**: Build tool providing 10x faster dev server (390ms vs 4.5s CRA), instant HMR, and smaller production bundles via Rollup
- **React Router v6**: Nested routes for protected content, improved developer API

### State Management (Hybrid Approach)
- **TanStack Query (React Query)**: Server state management (~80% of state)
  - Photo fetching, caching, and synchronization
  - Album/tag data management
  - Built-in cache invalidation and refetching
  - Optimistic updates for photo operations

- **Zustand**: Client state management (~20% of state)
  - UI state (selected photos, view mode, filters)
  - Lightbox open/closed state
  - Upload progress tracking
  - User preferences (grid size, sort order)

**Rationale**: Separation of server and client state keeps data synchronized while maintaining lightweight local state. Zustand has 30%+ YoY growth and appears in 40% of modern projects.

### Gallery & Image Components
- **react-visual-grid** or **TanStack Virtual**: Virtualized rendering for handling thousands of photos without performance degradation
- **Modern lightbox library**: Full-screen viewing with touch gestures, zoom, smooth transitions (7KB gzipped)
- **Native lazy loading**: `loading="lazy"` attribute on img tags
- **Responsive images**: srcset and sizes for device-appropriate assets

### File Upload
- **UpChunk**: Chunked upload library with 500KB optimal chunk size
  - Progress tracking per chunk with percentage calculation
  - Pause/resume/retry/cancel capabilities
  - Range request headers for proper chunk handling
  - Recovery from partial failures

### UI & Styling
- Modern component library (to be selected during initialization)
- CSS-in-JS or CSS modules for component styles
- Responsive design with mobile-first approach

### Route Structure
```
/login              # Public route
/signup             # Public route
/                   # Protected routes wrapper
  /gallery          # Main photo grid
  /albums/:id       # Album view
  /timeline         # Timeline view
  /upload           # Upload interface
```

### Project Structure
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

---

## Backend Stack

### Core Framework
- **ASP.NET Core 8+**: Modern, high-performance web API framework
- **Clean Architecture**: Four-layer structure (Domain, Application, Infrastructure, API)
  - Domain: Entities, value objects, specifications
  - Application: Business logic, DTOs, interfaces (CQRS pattern)
  - Infrastructure: Data access, file I/O, external services
  - API: Controllers, middleware, configuration

**Rationale**: Clean Architecture inverts dependencies so infrastructure depends on business logic, enabling testability and maintainability as the photo management system grows.

### Authentication & Authorization
- **ASP.NET Core Identity**: User management and authentication
- **JWT Tokens**: Stateless API access
  - Short-lived access tokens (15 minutes)
  - Refresh tokens (7 days) stored in database with revocation capability
  - HMAC-SHA256 signing with secure key from environment variables

**Authorization Strategy**:
- Role-based authorization for admin features
- Claims-based authorization for photo ownership
- Policy-based authorization for album sharing

**Security**:
- HTTPS-only in production
- Token rotation for refresh tokens
- Server-side validation on every request
- Never trust client-provided filenames

### Database & ORM
- **PostgreSQL**: Primary database
- **Entity Framework Core**: ORM with Npgsql provider
- **Npgsql.EntityFrameworkCore.PostgreSQL** (8.0.4+): PostgreSQL-specific features

**Core Entities**:
```
Photo       # Original file, storage key, EXIF data (JSON), thumbnails
Album       # Name, description, cover photo
Tag         # Name, category
User        # ASP.NET Identity tables
PhotoTag    # Many-to-many relationship
AlbumPhoto  # Many-to-many relationship
```

**PostgreSQL-Specific Features**:
- JSONB columns for EXIF metadata (flexible schema)
- Full-text search for photo descriptions
- Array types for storing multiple tags efficiently
- Indexes on UserId, CapturedAt for timeline queries

### File Storage
- **Local File System**: Initial implementation
- **Storage Structure**:
  ```
  /photos/{userId}/{year}/{month}/{guid}.jpg
  /photos/{userId}/{year}/{month}/{guid}_thumb.jpg
  ```
- **Strategy**: Files stored outside wwwroot to prevent direct serving
- **Streaming**: Use streaming for files >5MB to avoid memory exhaustion
- **Security**: Whitelist MIME types, validate file signatures, generate GUID-based storage keys

### Image Processing
- **SixLabors.ImageSharp**: Cross-platform, modern image processing library

**Processing Pipeline**:
1. Validate uploaded image
2. Extract EXIF metadata (capture date, camera model, GPS coordinates)
3. Auto-rotate based on EXIF orientation
4. Generate thumbnail (300x300 max, maintain aspect ratio)
5. Compress original if >2MB (JPEG quality 85)
6. Strip sensitive EXIF data (GPS if user preference)
7. Save original and thumbnail to storage

**Alternative Libraries**: Magick.NET (more formats), MetadataExtractor (EXIF-only, lightweight)

### API Design
**RESTful Principles**:
- Plural nouns for resources (/photos, /albums, /tags)
- Standard HTTP verbs (GET, POST, PUT, DELETE)
- JSON request/response format
- Pagination, filtering, and sorting support

**Key Endpoints**:
```
POST   /api/photos                          # Upload photo(s)
GET    /api/photos?page=1&pageSize=50       # List photos (paginated)
GET    /api/photos/{id}                     # Get photo details
PUT    /api/photos/{id}                     # Update metadata
DELETE /api/photos/{id}                     # Delete photo
GET    /api/photos/{id}/download            # Download original

POST   /api/albums                          # Create album
GET    /api/albums                          # List user's albums
POST   /api/albums/{id}/photos              # Add photos to album

GET    /api/timeline?year=2026&month=2      # Timeline view
GET    /api/tags                            # List all tags
```

### Project Structure
```
MyPhotoBooth.API/
├── src/
│   ├── MyPhotoBooth.Domain/          # Entities, value objects, enums
│   ├── MyPhotoBooth.Application/     # Business logic, DTOs, CQRS
│   ├── MyPhotoBooth.Infrastructure/  # DbContext, repositories, file I/O
│   └── MyPhotoBooth.API/             # Controllers, middleware, Program.cs
```

---

## Database

### PostgreSQL Configuration
- **Version**: PostgreSQL 12+ (latest stable recommended)
- **Connection**: Npgsql.EntityFrameworkCore.PostgreSQL provider
- **Migrations**: EF Core migrations for schema management

### Schema Design Highlights
- **GUID primary keys**: For distributed systems and security
- **JSONB for EXIF**: Flexible metadata storage without schema changes
- **Many-to-many relationships**: PhotoTag and AlbumPhoto junction tables
- **Timestamps**: CapturedAt (from EXIF), UploadedAt, CreatedAt
- **Indexes**: UserId, CapturedAt, TagId for optimized queries

### Performance Considerations
- Index frequently queried columns (UserId, CapturedAt, AlbumId)
- Use database-level pagination for large result sets
- Leverage PostgreSQL full-text search for description queries
- Consider partitioning Photo table by year for very large collections

---

## Development Tools

### Package Managers
- **Frontend**: npm or yarn (prefer npm for consistency)
- **Backend**: NuGet for .NET packages

### Required NuGet Packages
```
Npgsql.EntityFrameworkCore.PostgreSQL (8.0.4+)
Microsoft.EntityFrameworkCore.Tools
Microsoft.AspNetCore.Identity.EntityFrameworkCore
SixLabors.ImageSharp
```

### Testing Frameworks
- **Frontend**: Vitest (built into Vite), React Testing Library
- **Backend**: xUnit, Moq (mocking), FluentAssertions

### Code Quality
- **Frontend**: ESLint, Prettier
- **Backend**: StyleCop, SonarAnalyzer
- **Git hooks**: Husky for pre-commit linting

### Development Environment
- **Node.js**: 18+ for frontend development
- **.NET SDK**: 8.0+ for backend development
- **PostgreSQL**: Local instance or Docker container
- **IDE**: VS Code (frontend), Visual Studio or Rider (backend)

---

## Deployment Considerations

### Frontend Hosting Options
- **Static hosting**: Vercel, Netlify, Cloudflare Pages
- **CDN**: Cloudflare or similar for automatic image optimization
- **Build output**: Vite produces optimized static files for deployment

### Backend Hosting Options
- **Cloud platforms**: Azure App Service, AWS Elastic Beanstalk, DigitalOcean App Platform
- **Containerization**: Docker + Kubernetes for scalability
- **Self-hosted**: Linux server with Nginx reverse proxy

### Database Hosting
- **Managed services**: Azure Database for PostgreSQL, AWS RDS, DigitalOcean Managed Databases
- **Self-hosted**: PostgreSQL on VPS with regular backups

### Environment Configuration
- **Frontend**: .env files for API endpoints
- **Backend**: appsettings.json with environment-specific overrides
- **Secrets**: Use Azure Key Vault, AWS Secrets Manager, or environment variables

### Performance Optimization
- **Image CDN**: Serve optimized images from CDN with automatic format conversion
- **Caching**: Implement HTTP caching headers for static assets
- **Compression**: Enable gzip/brotli compression on server
- **Load balancing**: Consider multiple API instances for high traffic

---

## Why These Choices

### Frontend Decisions
**Vite over CRA**: 10x faster development experience with instant HMR and modern build optimization

**TanStack Query + Zustand**: Separates server and client state concerns, reducing complexity and improving maintainability

**Virtualized gallery**: Essential for handling thousands of photos without browser slowdown

**Chunked uploads**: Enables reliable large file uploads with progress tracking and failure recovery

### Backend Decisions
**ASP.NET Core 8+**: Enterprise-grade performance, excellent async/await support, strong typing, and comprehensive ecosystem

**Clean Architecture**: Testable, maintainable codebase that can evolve as requirements grow

**PostgreSQL**: Robust open-source database with advanced features (JSONB, full-text search, arrays)

**Entity Framework Core**: Type-safe queries, migration management, and excellent PostgreSQL support

**SixLabors.ImageSharp**: Cross-platform, modern, actively maintained image processing without native dependencies

### Storage Decisions
**Local file system (initial)**: Simplest to implement, no external dependencies, easy to migrate to cloud storage later

**GUID-based filenames**: Security (no path traversal), uniqueness across distributed systems

**Outside wwwroot**: Prevents direct file access, enables authorization checks

### Authentication Decisions
**ASP.NET Identity + JWT**: Industry-standard pattern for SPA authentication, built-in security features

**Short-lived tokens**: Minimizes risk of token compromise

**Refresh tokens in database**: Enables revocation for logout and security incidents

---

## Next Steps

1. Initialize Vite + React project with TypeScript
2. Set up ASP.NET Core solution with Clean Architecture structure
3. Configure PostgreSQL connection and create initial migrations
4. Implement authentication (ASP.NET Identity + JWT)
5. Build photo upload endpoint with streaming and validation
6. Create image processing pipeline with thumbnails
7. Develop gallery components with virtualization
8. Implement album and tag management
9. Add timeline view with chronological grouping
10. Optimize performance (lazy loading, caching, CDN)
