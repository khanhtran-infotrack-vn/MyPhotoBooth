# Changelog

All notable changes to MyPhotoBooth will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-02-10

### Added - Public Sharing Feature
- 🔗 **Share photos and albums via public links**
  - Token-based secure sharing (cryptographically random 32-byte tokens)
  - Password protection for sensitive shares
  - Expiration date configuration
  - Download control (enable/disable per share)
  - Link revocation with instant effect
  - Share link management UI

- **Backend Implementation**:
  - New `ShareLink` entity with comprehensive validation
  - `ShareLinksController` for authenticated share management
  - `SharedController` for public unauthenticated access
  - Password hashing using ASP.NET Identity PasswordHasher
  - Token generation with RandomNumberGenerator
  - Database migration: `20260210021155_AddShareLinks`

- **Frontend Implementation**:
  - `ShareModal` component for creating share links
  - `ShareManagement` page for managing active shares
  - `SharedView` public page for viewing shared content
  - `SharedPhotoGrid` and `SharedLightbox` for shared albums
  - New hooks: `useShareLinks` and `useSharedContent`
  - Public API client (`publicApi`) for unauthenticated requests

- **API Endpoints**:
  - `POST /api/sharelinks` - Create share link (authenticated)
  - `GET /api/sharelinks` - List user's share links (authenticated)
  - `DELETE /api/sharelinks/{id}` - Revoke share link (authenticated)
  - `GET /api/shared/{token}` - Get share metadata (public)
  - `POST /api/shared/{token}/access` - Access shared content with password (public)
  - `GET /api/shared/{token}/photos/{id}/file` - Download shared photo (public)
  - `GET /api/shared/{token}/photos/{id}/thumbnail` - Get shared thumbnail (public)

- **Routes**:
  - `/shares` - Share link management (protected)
  - `/shared/{token}` - Public shared content view

### Changed
- Updated `AppDbContext` to include ShareLinks entity configuration with computed properties ignored
- Added `IShareLinkRepository` registration in DependencyInjection
- Enhanced PhotoGallery and AlbumDetail with share functionality
- Added "Shares" navigation item to Sidebar
- Updated Lightbox with share action button

### Security
- Cryptographically secure token generation (base64url encoded)
- Password hashing with industry-standard algorithm
- Token validation on every request
- Expiration and revocation enforcement
- Download control per share link
- No authentication required for public endpoints (by design)

## [1.0.0] - 2026-02-09

### Added
- 🎉 Initial release of MyPhotoBooth
- 🔐 User authentication with email/password
- 🔑 JWT-based authentication with access and refresh tokens
- 📸 Photo upload with drag-and-drop support
- 🖼️ Automatic image processing (EXIF extraction, auto-rotation, thumbnail generation)
- 📱 Responsive photo gallery with grid layout
- 🔍 Lightbox view for full-size photos
- 📁 Album creation and management
- 🏷️ Photo tagging system
- 📅 Timeline view for browsing photos by date
- 🎨 Modern UI with gradient theme and smooth animations
- 📖 Interactive API documentation with Scalar
- 🐳 Docker Compose setup for PostgreSQL
- 📝 Comprehensive documentation (README, DEPLOYMENT, CLAUDE.md)
- 🔒 CORS configuration for frontend-backend communication
- ✅ Clean Architecture implementation (Domain, Application, Infrastructure, API)

### Security
- Password requirements (min 8 chars, uppercase, digit)
- JWT token rotation with refresh tokens
- File upload validation (type and size)
- Authorization on all protected endpoints
- Secure password hashing with ASP.NET Identity

### Technical
- ASP.NET Core 10.0 Web API
- React 18 with TypeScript
- Entity Framework Core 10.0
- PostgreSQL 16
- Vite build tool
- TanStack Query for server state
- Zustand for client state
- Axios with automatic token refresh
- SixLabors.ImageSharp for image processing

## [Unreleased]

### Planned Features
- Cloud storage integration (AWS S3, Azure Blob)
- Face detection and recognition
- Advanced search and filtering
- Bulk photo operations
- Photo editing capabilities
- Mobile app (React Native)
- Social features (comments, likes)
- Export albums to PDF/ZIP
- Automated photo organization
- Multi-user collaboration on albums

---

[1.1.0]: https://github.com/yourusername/myphotobooth/releases/tag/v1.1.0
[1.0.0]: https://github.com/yourusername/myphotobooth/releases/tag/v1.0.0
