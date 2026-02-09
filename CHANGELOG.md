# Changelog

All notable changes to MyPhotoBooth will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
- Photo sharing with other users
- Face detection and recognition
- Advanced search and filtering
- Bulk photo operations
- Photo editing capabilities
- Mobile app (React Native)
- Social features (comments, likes)
- Export albums to PDF/ZIP
- Automated photo organization

---

[1.0.0]: https://github.com/yourusername/myphotobooth/releases/tag/v1.0.0
