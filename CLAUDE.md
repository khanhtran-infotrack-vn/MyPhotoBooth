# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MyPhotoBooth is a full-stack photo memories application for storing, organizing, and viewing photos with friends and family. Built with React (TypeScript + Vite) frontend and ASP.NET Core 10 backend following Clean Architecture principles.

## Project Status

**Status**: ✅ Production Ready (v1.1.0)

All core features implemented and tested, including public sharing (v1.1.0). Ready for deployment.

## Architecture

### Clean Architecture (4 Layers)

```
src/
├── MyPhotoBooth.API/          # Presentation (Controllers, Middleware)
├── MyPhotoBooth.Application/  # Application (DTOs, Interfaces)
├── MyPhotoBooth.Infrastructure/# Infrastructure (EF Core, Services)
├── MyPhotoBooth.Domain/       # Domain (Entities, Business Logic)
└── client/                    # React Frontend (TypeScript + Vite)
```

### Technology Stack

**Backend:**
- ASP.NET Core 10.0 Web API
- Entity Framework Core 10.0
- PostgreSQL 16
- ASP.NET Identity + JWT Authentication
- SixLabors.ImageSharp (Image Processing)
- Scalar API Documentation

**Frontend:**
- React 18 + TypeScript
- Vite (Build Tool)
- React Router v6
- TanStack Query (Server State)
- Zustand (Client State)
- Axios (HTTP Client)

## Build Commands

### Backend

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/MyPhotoBooth.API

# Database migrations
dotnet ef migrations add MigrationName --project src/MyPhotoBooth.Infrastructure --startup-project src/MyPhotoBooth.API
dotnet ef database update --project src/MyPhotoBooth.API
```

### Frontend

```bash
cd src/client

# Install dependencies
npm install

# Development
npm run dev

# Build
npm run build

# Preview production build
npm run preview
```

### Docker

```bash
# Start PostgreSQL
docker-compose up -d

# Production deployment
docker-compose -f docker-compose.prod.yml up -d
```

## Key Features

1. **Authentication** - Email/password with JWT tokens (access + refresh)
2. **Photo Upload** - Drag-and-drop with progress tracking
3. **Image Processing** - Auto-rotation, EXIF extraction, thumbnails
4. **Photo Gallery** - Grid view with lightbox
5. **Albums** - Organize photos into collections
6. **Tags** - Tag photos for easy searching
7. **Timeline** - Browse photos by date
8. **Public Sharing** - Share photos/albums via secure token-based links
9. **Modern UI** - Gradient design with smooth animations

## Project Structure

### Backend Endpoints

- `POST /api/auth/register|login|refresh|logout` - Authentication
- `GET|POST|PUT|DELETE /api/photos` - Photo management
- `GET /api/photos/{id}/file|thumbnail` - Serve images
- `GET /api/photos/timeline` - Timeline view
- `CRUD /api/albums` - Album management
- `CRUD /api/tags` - Tag management
- `GET|POST|DELETE /api/sharelinks` - Share link management
- `GET /api/shared/{token}` - Public shared content access

### Frontend Routes

- `/login` - Login page
- `/register` - Registration page
- `/` or `/photos` - Photo gallery (protected)
- `/albums` - Album list (protected)
- `/albums/{id}` - Album detail (protected)
- `/tags` - Tags list (protected)
- `/shares` - Share links management (protected)
- `/shared/{token}` - Public shared content view (public)

### Database Schema

**Tables:**
- AspNetUsers (Identity)
- AspNetRoles (Identity)
- AspNetUserRoles (Identity)
- RefreshTokens (JWT refresh tokens)
- Photos (photo metadata)
- Albums (photo collections)
- Tags (photo tags)
- PhotoTags (many-to-many)
- AlbumPhotos (many-to-many with sort order)
- ShareLinks (public sharing tokens)

## Configuration Files

### Backend

- `appsettings.Development.json` - Development config
- `appsettings.Production.json` - Production config (create from template)

### Frontend

- `.env.development` - Development environment variables
- `.env.production` - Production environment variables

## Development Guidelines

### Backend

1. Follow Clean Architecture principles
2. Use repository pattern for data access
3. DTOs for API contracts
4. Async/await for all I/O operations
5. Proper error handling and validation

### Frontend

1. Use TypeScript for type safety
2. Components in `features/` directory
3. Shared state with Zustand
4. Server state with TanStack Query
5. Axios interceptors for JWT refresh

## Security

- JWT tokens with refresh rotation
- Password requirements (8+ chars, uppercase, digit)
- CORS configured for frontend origin
- Authorization on all protected endpoints
- File upload validation (type + size)
- EXIF data sanitization

## Storage

- Local file system: `./storage/photos/`
- Structure: `{userId}/{year}/{month}/{filename}`
- Thumbnails: 300x300px, auto-generated
- Max file size: 50MB (configurable)

## Access Points

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5149
- **API Docs**: http://localhost:5149/scalar/v1
- **OpenAPI Spec**: http://localhost:5149/openapi/v1.json

## Documentation

- [README.md](README.md) - Project overview and setup
- [DEPLOYMENT.md](DEPLOYMENT.md) - Deployment guide
- API documentation available at `/scalar/v1` when running

## Known Issues / TODOs

None currently. All core features are complete and working.

## Recent Changes

### v1.1.0 - Public Sharing Feature
- ✅ Token-based public link sharing for photos and albums
- ✅ Password protection for sensitive shares
- ✅ Expiration date configuration
- ✅ Download control (enable/disable)
- ✅ Share link revocation
- ✅ ShareManagement UI for managing active shares
- ✅ Public SharedView for viewing shared content without authentication
- ✅ New ShareLink entity with migration (20260210021155_AddShareLinks)

### v1.0.0 - Initial Release
- ✅ Fixed 401 error on thumbnails (use authenticated blob URLs)
- ✅ Fixed CORS issues for registration/login
- ✅ Redesigned UI with modern gradient theme
- ✅ Added drag-and-drop file upload
- ✅ Improved lightbox with photo info
- ✅ Added Scalar API documentation
- ✅ Implemented JWT authentication with refresh tokens
- ✅ Added image processing with EXIF extraction
- ✅ Implemented albums and tags features

## Contributing

When making changes:
1. Follow existing code structure and patterns
2. Update documentation if adding new features
3. Test thoroughly before committing
4. Use conventional commits (feat:, fix:, docs:, etc.)

## Support

For questions or issues, refer to:
- README.md for setup instructions
- DEPLOYMENT.md for deployment help
- API documentation at /scalar/v1
