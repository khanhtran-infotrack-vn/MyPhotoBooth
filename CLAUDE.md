# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MyPhotoBooth is a full-stack photo memories application for storing, organizing, and viewing photos with friends and family. Built with React (TypeScript + Vite) frontend and ASP.NET Core 10 backend following Clean Architecture principles.

## Project Status

**Status**: ✅ Production Ready (v1.3.0)

All core features implemented and tested, including CQRS architecture with MediatR (v1.3.0). Comprehensive test coverage with 117 tests. Ready for deployment.

## Architecture

### Clean Architecture (4 Layers) + CQRS

```
src/
├── MyPhotoBooth.API/          # Presentation (Controllers, Middleware)
├── MyPhotoBooth.Application/  # Application (CQRS, MediatR, Validators)
│   ├── Features/              # Feature-based organization
│   │   ├── Auth/             # Commands, Queries, Handlers, Validators
│   │   ├── Photos/           # Commands, Queries, Handlers, Validators
│   │   ├── Albums/           # Commands, Queries, Handlers, Validators
│   │   ├── Tags/             # Commands, Queries, Handlers, Validators
│   │   └── ShareLinks/       # Commands, Queries, Handlers, Validators
│   └── Common/               # Shared components
│       ├── Behaviors/        # Validation, Logging, Transaction
│       ├── DTOs/             # Data transfer objects
│       ├── Requests/         # IRequest, ICommand, IQuery
│       ├── Validators/       # Shared validators
│       └── Pagination/       # PaginatedResult
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
- MediatR 14.0 (CQRS Pattern)
- FluentValidation 12.1 (Validation)
- CSharpFunctionalExtensions (Result Pattern)
- SixLabors.ImageSharp (Image Processing)
- Scalar API Documentation

**Testing:**
- xUnit 2.9 (Test Framework)
- Moq 4.20 (Mocking)
- FluentAssertions 8.8 (Assertions)
- Testcontainers 4.10 (Integration Testing)
- Coverlet 6.0 (Code Coverage)

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
# Start PostgreSQL and Mailpit
docker-compose up -d

# Production deployment
docker-compose -f docker-compose.prod.yml up -d
```

### Quick Start Script

```bash
# Start all services (PostgreSQL, Mailpit, Backend, Frontend)
./start.sh
```

## Key Features

1. **Authentication** - Email/password with JWT tokens (access + refresh)
2. **Password Reset** - Forgot password flow with email tokens
3. **Photo Upload** - Drag-and-drop with progress tracking
4. **Image Processing** - Auto-rotation, EXIF extraction, thumbnails
5. **Photo Gallery** - Grid view with lightbox
6. **Albums** - Organize photos into collections
7. **Tags** - Tag photos for easy searching
8. **Timeline** - Browse photos by date
9. **Public Sharing** - Share photos/albums via secure token-based links
10. **Modern UI** - Gradient design with smooth animations
11. **Smart Routing** - Auth redirects for logged-in users

## Project Structure

### Backend Endpoints

- `POST /api/auth/register|login|refresh|logout` - Authentication
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token
- `GET|POST|PUT|DELETE /api/photos` - Photo management
- `GET /api/photos/{id}/file|thumbnail` - Serve images
- `GET /api/photos/timeline` - Timeline view
- `CRUD /api/albums` - Album management
- `CRUD /api/tags` - Tag management
- `GET|POST|DELETE /api/sharelinks` - Share link management
- `GET /api/shared/{token}` - Public shared content access

### Frontend Routes

- `/login` - Login page (public route, redirects if authenticated)
- `/register` - Registration page (public route, redirects if authenticated)
- `/forgot-password` - Request password reset (public)
- `/reset-password` - Reset password with token (public)
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

- `appsettings.Development.json` - Development config (includes EmailSettings for Mailpit)
- `appsettings.Production.json` - Production config (create from template, configure real email service)

### Frontend

- `.env.development` - Development environment variables
- `.env.production` - Production environment variables

## Development Guidelines

### Backend (CQRS with MediatR)

1. **Creating a New Feature:**
   - Create feature folder: `Application/Features/{FeatureName}/`
   - Add Commands folder for write operations
   - Add Queries folder for read operations
   - Add Handlers folder for business logic
   - Add Validators folder for FluentValidation rules

2. **Command Pattern:**
   ```csharp
   public record CreateSomethingCommand(string Name) : ICommand<SomethingDto>;

   public class CreateSomethingCommandValidator : AbstractValidator<CreateSomethingCommand>
   {
       // Validation rules
   }

   public class CreateSomethingCommandHandler : ICommandHandler<CreateSomethingCommand, SomethingDto>
   {
       // Handler implementation
   }
   ```

3. **Query Pattern:**
   ```csharp
   public record GetSomethingQuery(Guid Id) : IQuery<SomethingDto>;

   public class GetSomethingQueryValidator : AbstractValidator<GetSomethingQuery>
   {
       // Validation rules
   }

   public class GetSomethingQueryHandler : IQueryHandler<GetSomethingQuery, SomethingDto>
   {
       // Handler implementation
   }
   ```

4. **Controller Pattern:**
   - Inject ISender from MediatR
   - Use _sender.Send(command/query) to dispatch
   - Return Result<T> from handlers
   - Minimal controller logic, all business logic in handlers

5. **Pipeline Behaviors:**
   - Validation runs automatically before handlers
   - Logging captures all requests/responses
   - Transactions wrap database operations

6. **Error Handling:**
   - Use Result.Success() or Result.Failure("error message")
   - Define error messages in Common/Errors.cs
   - Return 400 Bad Request for validation failures
   - Return 404 for not found resources

7. **Testing:**
   - Unit tests for validators, behaviors, handlers
   - Integration tests for API endpoints
   - Use Testcontainers for PostgreSQL in integration tests
   - Mock dependencies in unit tests

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
- **Mailpit (Email Testing)**: http://localhost:8025

## Documentation

- [README.md](README.md) - Project overview and setup
- [DEPLOYMENT.md](DEPLOYMENT.md) - Deployment guide
- API documentation available at `/scalar/v1` when running

## Known Issues / TODOs

None currently. All core features are complete and working.

## Recent Changes

### v1.3.0 - CQRS Architecture & Testing
- ✅ Implemented CQRS pattern using MediatR 14.0
- ✅ Separated Commands (writes) and Queries (reads)
- ✅ Migrated all services to command/query handlers
- ✅ Controllers now use ISender for dispatching requests
- ✅ Integrated FluentValidation 12.1 for declarative validation
- ✅ Created pipeline behaviors: Validation, Logging, Transaction
- ✅ Implemented Result<T> pattern for error handling
- ✅ Added 86 unit tests (validators, behaviors, handlers)
- ✅ Added 31 integration tests (API endpoints with Testcontainers)
- ✅ Feature-based folder structure in Application layer
- ✅ Test coverage: ~70% validators, ~10% handlers, 100% behaviors, 100% API endpoints

### v1.2.0 - Login Flow Completion
- ✅ Fixed user data consistency on page refresh (localStorage persistence)
- ✅ Added PublicRoute component for authenticated user redirects
- ✅ Implemented complete forgot password flow with email tokens
- ✅ Added EmailService with Mailpit integration for development
- ✅ Created password reset UI (ForgotPassword, ResetPassword components)
- ✅ Added quick start script (start.sh) for easy development setup
- ✅ New API endpoints: /api/auth/forgot-password, /api/auth/reset-password

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
