# MyPhotoBooth Documentation

Welcome to the MyPhotoBooth documentation. This directory contains comprehensive technical documentation for developers working on the project.

## Table of Contents

### Getting Started
- [Main README](../README.md) - Project overview, setup instructions, and quick start guide
- [CLAUDE.md](../CLAUDE.md) - Project guidance for Claude Code AI assistant
- [CHANGELOG](../CHANGELOG.md) - Version history and release notes

### Setup & Deployment
- [DEPLOYMENT.md](../DEPLOYMENT.md) - Production deployment guide with Docker, AWS, Azure, and GCP instructions

### Technical Documentation
- [Tech Stack](./tech-stack.md) - Comprehensive technology stack overview and architecture decisions
- [Public Sharing Feature](./sharing-feature.md) - Detailed documentation for the public sharing feature (v1.1.0)

## Project Structure

```
MyPhotoBooth/
├── src/
│   ├── MyPhotoBooth.API/          # ASP.NET Core Web API (Presentation Layer)
│   ├── MyPhotoBooth.Application/  # Application logic, DTOs, interfaces
│   ├── MyPhotoBooth.Infrastructure/# EF Core, repositories, file storage
│   ├── MyPhotoBooth.Domain/       # Domain entities and business logic
│   └── client/                     # React TypeScript frontend
├── docs/                           # Documentation (you are here)
├── docker-compose.yml              # Development PostgreSQL setup
├── README.md                       # Main project documentation
├── DEPLOYMENT.md                   # Deployment guide
├── CLAUDE.md                       # AI assistant guidance
└── CHANGELOG.md                    # Version history
```

## Quick Links

### API Documentation
- **Local Development**: http://localhost:5149/scalar/v1 (when backend is running)
- **OpenAPI Spec**: http://localhost:5149/openapi/v1.json

### Core Features Documentation

#### Authentication & Authorization
- Email/password authentication
- JWT access tokens (15 minutes)
- Refresh tokens (7 days)
- Role-based authorization
- Token rotation on refresh

#### Photo Management
- Upload with drag-and-drop
- EXIF data extraction
- Automatic thumbnail generation (300x300px)
- Auto-rotation based on EXIF orientation
- File validation (type and size)
- Timeline view by date

#### Albums & Tags
- Create, edit, delete albums
- Add/remove photos from albums
- Custom sort order within albums
- Tag photos with keywords
- Search photos by tags

#### Public Sharing (v1.1.0)
- Token-based public links
- Password protection
- Expiration dates
- Download control
- Link revocation
- Share management UI

See [Public Sharing Feature](./sharing-feature.md) for detailed documentation.

## Architecture Overview

MyPhotoBooth follows **Clean Architecture** principles with four distinct layers:

### 1. Domain Layer (`MyPhotoBooth.Domain`)
- Core business entities (Photo, Album, Tag, ShareLink, User)
- Domain enums (ShareLinkType)
- No dependencies on other layers

### 2. Application Layer (`MyPhotoBooth.Application`)
- Business logic and use cases
- Data Transfer Objects (DTOs)
- Repository interfaces
- Service interfaces

### 3. Infrastructure Layer (`MyPhotoBooth.Infrastructure`)
- Entity Framework Core DbContext
- Repository implementations
- File storage service
- External service integrations
- Database migrations

### 4. Presentation Layer (`MyPhotoBooth.API`)
- REST API controllers
- Middleware configuration
- Authentication setup
- API documentation (Scalar)
- Program.cs and startup configuration

### Frontend Architecture (`client`)
- React 18 with TypeScript
- Vite for build and development
- React Router v6 for navigation
- TanStack Query for server state
- Zustand for client state
- Axios for HTTP requests
- Tailwind CSS for styling

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 10.0
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core 10.0
- **Authentication**: ASP.NET Identity + JWT
- **Image Processing**: SixLabors.ImageSharp
- **API Docs**: Scalar (OpenAPI)

### Frontend
- **Framework**: React 18
- **Language**: TypeScript
- **Build Tool**: Vite
- **Routing**: React Router v6
- **Server State**: TanStack Query (React Query)
- **Client State**: Zustand
- **HTTP Client**: Axios
- **Styling**: Tailwind CSS

### Infrastructure
- **Database**: PostgreSQL 16
- **Container**: Docker & Docker Compose
- **Storage**: Local file system (cloud-ready)

## Development Workflow

### Backend Development
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run API (with hot reload)
dotnet watch run --project src/MyPhotoBooth.API

# Create migration
dotnet ef migrations add MigrationName --project src/MyPhotoBooth.Infrastructure --startup-project src/MyPhotoBooth.API

# Update database
dotnet ef database update --project src/MyPhotoBooth.API
```

### Frontend Development
```bash
cd src/client

# Install dependencies
npm install

# Run dev server (with HMR)
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

### Database
```bash
# Start PostgreSQL
docker-compose up -d

# Stop PostgreSQL
docker-compose down

# View logs
docker-compose logs -f postgres
```

## API Endpoints Reference

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get tokens
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout and invalidate refresh token

### Photos
- `POST /api/photos` - Upload photo(s)
- `GET /api/photos` - List photos (paginated)
- `GET /api/photos/{id}` - Get photo details
- `GET /api/photos/{id}/file` - Download original photo
- `GET /api/photos/{id}/thumbnail` - Get thumbnail
- `PUT /api/photos/{id}` - Update photo metadata
- `DELETE /api/photos/{id}` - Delete photo
- `GET /api/photos/timeline` - Get timeline view

### Albums
- `POST /api/albums` - Create album
- `GET /api/albums` - List user's albums
- `GET /api/albums/{id}` - Get album details
- `PUT /api/albums/{id}` - Update album
- `DELETE /api/albums/{id}` - Delete album
- `POST /api/albums/{id}/photos/{photoId}` - Add photo to album
- `DELETE /api/albums/{id}/photos/{photoId}` - Remove photo from album

### Tags
- `POST /api/tags` - Create tag
- `GET /api/tags` - List all tags
- `GET /api/tags/{id}` - Get tag details
- `GET /api/tags/search?name={query}` - Search tags
- `PUT /api/tags/{id}` - Update tag
- `DELETE /api/tags/{id}` - Delete tag

### Share Links (v1.1.0)
- `POST /api/sharelinks` - Create share link
- `GET /api/sharelinks` - List user's share links
- `DELETE /api/sharelinks/{id}` - Revoke share link

### Public Shared Content (v1.1.0)
- `GET /api/shared/{token}` - Get share metadata (public)
- `POST /api/shared/{token}/access` - Access shared content (public)
- `GET /api/shared/{token}/photos/{id}/file` - Download shared photo (public)
- `GET /api/shared/{token}/photos/{id}/thumbnail` - Get shared thumbnail (public)

## Database Schema

### Core Tables
- **AspNetUsers** - User accounts (Identity)
- **AspNetRoles** - User roles (Identity)
- **AspNetUserRoles** - User-role mapping (Identity)
- **RefreshTokens** - JWT refresh tokens
- **Photos** - Photo metadata and EXIF data
- **Albums** - Photo collections
- **Tags** - Photo tags
- **PhotoTags** - Many-to-many: Photo ↔ Tag
- **AlbumPhotos** - Many-to-many: Album ↔ Photo (with sort order)
- **ShareLinks** - Public share links (v1.1.0)

### Relationships
```
User (1) ──→ (N) Photos
User (1) ──→ (N) Albums
User (1) ──→ (N) ShareLinks

Photo (N) ←→ (N) Tags (via PhotoTags)
Photo (N) ←→ (N) Albums (via AlbumPhotos)

ShareLink (N) ──→ (1) Photo (nullable)
ShareLink (N) ──→ (1) Album (nullable)
```

## Configuration

### Backend Configuration

**appsettings.Development.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=myphotobooth_dev;Username=postgres;Password=postgres_dev_password"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "MyPhotoBooth.API",
    "Audience": "MyPhotoBooth.Client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "StorageSettings": {
    "PhotosBasePath": "./storage/photos",
    "MaxFileSizeMB": 50
  }
}
```

### Frontend Configuration

**.env.development**:
```env
VITE_API_BASE_URL=http://localhost:5149/api
```

## Security

### Authentication
- JWT access tokens (15-minute expiration)
- Refresh tokens with rotation (7-day expiration)
- HMAC-SHA256 signing algorithm
- Secure token storage (httpOnly cookies recommended for production)

### Authorization
- Role-based access control
- Owner-based resource access
- Protected file serving (no direct file access)

### File Security
- MIME type validation
- File size limits (configurable)
- GUID-based filenames (prevents path traversal)
- Files stored outside wwwroot
- EXIF data sanitization

### Share Link Security (v1.1.0)
- Cryptographically random tokens (32 bytes)
- Password hashing (PBKDF2 via PasswordHasher)
- Expiration enforcement
- Revocation support
- Download control

## Testing

### Backend Tests
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

### Frontend Tests
```bash
cd src/client

# Run tests
npm run test

# Run tests with coverage
npm run test:coverage
```

## Contributing

### Coding Standards

**Backend**:
- Follow Clean Architecture principles
- Use async/await for all I/O operations
- Repository pattern for data access
- DTOs for API contracts
- Proper error handling and validation

**Frontend**:
- Use TypeScript for type safety
- Components in `features/` directory by feature
- Shared components in `components/` directory
- Custom hooks in `hooks/` directory
- TanStack Query for server state
- Zustand for client state

### Commit Message Format
Use conventional commits format:
```
feat: add photo sharing feature
fix: resolve thumbnail loading issue
docs: update API documentation
refactor: improve album query performance
test: add unit tests for photo service
```

### Pull Request Process
1. Create feature branch from `main`
2. Implement changes following coding standards
3. Update relevant documentation
4. Add/update tests
5. Ensure all tests pass
6. Create PR with clear description
7. Request code review

## Troubleshooting

### Common Issues

**Backend won't start**:
- Check PostgreSQL is running: `docker-compose ps`
- Verify connection string in appsettings.json
- Run database migrations: `dotnet ef database update`

**Frontend build errors**:
- Delete `node_modules` and run `npm install`
- Clear Vite cache: `rm -rf node_modules/.vite`
- Check Node.js version (18+ required)

**CORS errors**:
- Verify frontend URL in backend CORS configuration
- Check VITE_API_BASE_URL in frontend .env file

**File upload fails**:
- Check storage path exists and has write permissions
- Verify MaxFileSizeMB setting
- Check available disk space

**Share links not working**:
- Verify migration `20260210021155_AddShareLinks` was applied
- Check ShareLinkRepository is registered in DI
- Verify token format (base64url)

## Support

For questions, issues, or contributions:

1. Check existing documentation
2. Review API documentation at `/scalar/v1`
3. Search closed issues on GitHub
4. Open new issue with detailed description

## Version History

- **v1.1.0** (2026-02-10): Public Sharing Feature
- **v1.0.0** (2026-02-09): Initial Production Release

See [CHANGELOG.md](../CHANGELOG.md) for detailed version history.

## License

This project is licensed under the MIT License.

---

**Last Updated**: 2026-02-10
**Version**: 1.1.0
