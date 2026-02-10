# 📸 MyPhotoBooth

A modern photo memories application for storing and viewing photos with friends and family. Built with React and ASP.NET Core using Clean Architecture and CQRS patterns.

![MyPhotoBooth](https://img.shields.io/badge/Version-1.3.0-blue)
![React](https://img.shields.io/badge/React-18-61dafb)
![.NET](https://img.shields.io/badge/.NET-10.0-512bd4)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)
![Test Coverage](https://img.shields.io/badge/Tests-117-success)

## ✨ Features

- 🔐 **Secure Authentication** - Email/password authentication with JWT tokens
- 🔑 **Password Reset** - Forgot password flow with email tokens
- 📸 **Photo Management** - Upload, view, organize, and delete photos
- 🖼️ **Smart Image Processing** - Auto-rotation, EXIF extraction, thumbnail generation
- 📁 **Albums & Tags** - Organize photos into albums and tag them
- 📅 **Timeline View** - Browse photos by date
- 🔗 **Public Sharing** - Share photos and albums via secure public links
- 🎨 **Modern UI** - Beautiful, responsive design with smooth animations
- 🌙 **Dark Mode** - Three-state theme toggle (Light | Dark | System)
- 🚀 **High Performance** - Optimized image loading with blob URLs
- 📱 **Responsive** - Works on desktop, tablet, and mobile
- ✅ **Smart Routing** - Auth redirects for logged-in users
- 🧪 **Comprehensive Testing** - 117 tests with ~70% coverage (unit + integration)

## 🏗️ Architecture

### Clean Architecture with CQRS Pattern

```
MyPhotoBooth/
├── src/
│   ├── MyPhotoBooth.API/          # Presentation Layer (Controllers, Middleware)
│   ├── MyPhotoBooth.Application/  # Application Layer (CQRS, MediatR, Validators)
│   ├── MyPhotoBooth.Infrastructure/# Infrastructure Layer (EF Core, Services)
│   ├── MyPhotoBooth.Domain/       # Domain Layer (Entities, Business Logic)
│   └── client/                     # React Frontend (TypeScript + Vite)
├── tests/
│   ├── MyPhotoBooth.UnitTests/    # Unit Tests (86 tests)
│   └── MyPhotoBooth.IntegrationTests/ # Integration Tests (31 tests)
```

### Technology Stack

**Backend:**
- ASP.NET Core 10.0 Web API
- Entity Framework Core 10.0
- PostgreSQL 16
- ASP.NET Identity (Authentication)
- JWT Bearer Authentication
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
- React 18
- TypeScript
- Vite
- React Router v6
- TanStack Query (React Query)
- Zustand (State Management)
- Axios (HTTP Client)

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker](https://www.docker.com/) (for PostgreSQL)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd MyPhotoBooth
   ```

2. **Start PostgreSQL with Docker**
   ```bash
   docker-compose up -d
   ```

3. **Configure Backend**

   Update `src/MyPhotoBooth.API/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=myphotobooth_dev;Username=postgres;Password=postgres_dev_password"
     },
     "JwtSettings": {
       "SecretKey": "your-super-secret-key-min-32-chars",
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

4. **Run Database Migrations**
   ```bash
   cd src/MyPhotoBooth.API
   dotnet ef database update
   ```

5. **Start Backend**
   ```bash
   dotnet run --project src/MyPhotoBooth.API
   ```
   Backend will run on: http://localhost:5149

6. **Configure Frontend**

   Update `src/client/.env.development`:
   ```env
   VITE_API_BASE_URL=http://localhost:5149/api
   ```

7. **Install Frontend Dependencies**
   ```bash
   cd src/client
   npm install
   ```

8. **Start Frontend**
   ```bash
   npm run dev
   ```
   Frontend will run on: http://localhost:3000

## 📖 Usage

### Access the Application

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5149
- **API Documentation**: http://localhost:5149/scalar/v1
- **OpenAPI Spec**: http://localhost:5149/openapi/v1.json

### Default User Roles

The application seeds two roles on startup:
- **User** - Regular users who can manage their photos
- **Admin** - Administrators (future use)

### Quick Start

Use the provided quick start script to launch all services:

```bash
./start.sh
```

This script will:
1. Start PostgreSQL and Mailpit services using Docker Compose
2. Start the backend API on http://localhost:5149
3. Start the frontend on http://localhost:3000
4. Run health checks to ensure all services are running

For individual service startup, see the Installation section below.

### API Endpoints

**Authentication:**
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout
- `POST /api/auth/forgot-password` - Request password reset email
- `POST /api/auth/reset-password` - Reset password with token

**Photos:**
- `POST /api/photos` - Upload photo
- `GET /api/photos` - List photos (paginated)
- `GET /api/photos/{id}` - Get photo details
- `GET /api/photos/{id}/file` - Download photo
- `GET /api/photos/{id}/thumbnail` - Get thumbnail
- `PUT /api/photos/{id}` - Update photo
- `DELETE /api/photos/{id}` - Delete photo
- `GET /api/photos/timeline` - Get timeline view

**Albums:**
- `POST /api/albums` - Create album
- `GET /api/albums` - List albums
- `GET /api/albums/{id}` - Get album details
- `PUT /api/albums/{id}` - Update album
- `DELETE /api/albums/{id}` - Delete album
- `POST /api/albums/{id}/photos/{photoId}` - Add photo to album
- `DELETE /api/albums/{id}/photos/{photoId}` - Remove photo from album

**Tags:**
- `POST /api/tags` - Create tag
- `GET /api/tags` - List tags
- `GET /api/tags/{id}` - Get tag details
- `GET /api/tags/search?name={query}` - Search tags
- `PUT /api/tags/{id}` - Update tag
- `DELETE /api/tags/{id}` - Delete tag

**Share Links:**
- `POST /api/sharelinks` - Create share link (photo or album)
- `GET /api/sharelinks` - List user's share links
- `DELETE /api/sharelinks/{id}` - Revoke share link

**Public Shared Content:**
- `GET /api/shared/{token}` - Get shared content (public, no auth)
- `POST /api/shared/{token}/verify` - Verify password for protected shares
- `GET /api/shared/{token}/photos/{id}/file` - Download shared photo
- `GET /api/shared/{token}/photos/{id}/thumbnail` - Get shared thumbnail

## 🔒 Security

- **JWT Authentication** - Secure token-based authentication
- **Password Requirements**:
  - Minimum 8 characters
  - At least 1 uppercase letter
  - At least 1 lowercase letter
  - At least 1 digit
- **CORS** - Configured for frontend origin
- **Authorization** - Protected endpoints require authentication
- **Refresh Tokens** - Secure token rotation
- **File Validation** - Image type and size validation

## 🎨 Features in Detail

### Image Processing

- **Auto-rotation** based on EXIF orientation
- **Thumbnail generation** (300x300px)
- **EXIF data extraction** (camera info, date taken, location)
- **File size optimization**
- Supported formats: JPEG, PNG, GIF, BMP

### Storage

- Local file system storage
- Organized structure: `photos/{userId}/{year}/{month}/`
- Configurable storage path and file size limits

### Public Sharing

Share photos and albums with anyone via secure public links:
- **Token-based access** - Cryptographically secure random tokens
- **Password protection** - Optional password requirement for sensitive content
- **Expiration dates** - Set links to auto-expire
- **Download control** - Enable/disable downloads for shared content
- **Revocation** - Instantly revoke access to shared links
- **Management** - View and manage all active share links

### Authentication Flow

1. User registers with email, password, and display name
2. User logs in and receives access token (15 min) + refresh token (7 days)
3. Access token is included in all API requests
4. When access token expires, refresh token is used to get new tokens
5. Refresh tokens are stored in database with rotation
6. User can reset password via email link (development uses Mailpit)

### Email Service (Development)

For development, the application uses Mailpit to capture emails without sending them:
- Mailpit Web UI: http://localhost:8025
- SMTP Server: localhost:1025

For production, configure a real email service (SendGrid, AWS SES, etc.) in `appsettings.Production.json`.

### Frontend Routes

- `/login` - Login page (redirects if already logged in)
- `/register` - Registration page (redirects if already logged in)
- `/forgot-password` - Request password reset
- `/reset-password` - Reset password with token
- `/` or `/photos` - Photo gallery (protected)
- `/albums` - Album list (protected)
- `/albums/{id}` - Album detail (protected)
- `/tags` - Tags list (protected)
- `/shares` - Share links management (protected)
- `/shared/{token}` - Public shared content view (public)

## 🐳 Docker Deployment

### Build Docker Image (Backend)

```bash
cd src/MyPhotoBooth.API
docker build -t myphotobooth-api .
```

### Build Docker Image (Frontend)

```bash
cd src/client
docker build -t myphotobooth-client .
```

### Docker Compose (Full Stack)

Create `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: myphotobooth
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  api:
    image: myphotobooth-api
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=myphotobooth;Username=postgres;Password=${DB_PASSWORD}"
      JwtSettings__SecretKey: ${JWT_SECRET}
    volumes:
      - api_storage:/app/storage
    ports:
      - "5149:5149"
    depends_on:
      - postgres

  client:
    image: myphotobooth-client
    environment:
      VITE_API_BASE_URL: http://localhost:5149/api
    ports:
      - "3000:3000"
    depends_on:
      - api

volumes:
  postgres_data:
  api_storage:
```

Run with:
```bash
docker-compose -f docker-compose.prod.yml up -d
```

## 🧪 Development

### Run Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/MyPhotoBooth.UnitTests

# Run integration tests only
dotnet test tests/MyPhotoBooth.IntegrationTests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test
dotnet test --filter "FullyQualifiedName~LoginCommandHandlerTests"

# Frontend tests
cd src/client
npm run test
```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/MyPhotoBooth.Infrastructure --startup-project src/MyPhotoBooth.API

# Update database
dotnet ef database update --project src/MyPhotoBooth.API

# Remove last migration
dotnet ef migrations remove --project src/MyPhotoBooth.Infrastructure --startup-project src/MyPhotoBooth.API
```

### Code Structure

**Backend Architecture (CQRS with MediatR):**
- **Controllers** - Thin wrappers that use ISender to dispatch commands/queries
- **Features** - Feature-based organization (Auth, Photos, Albums, Tags, ShareLinks)
  - **Commands** - Write operations (Create, Update, Delete)
  - **Queries** - Read operations (Get, List, Search)
  - **Handlers** - Business logic implementation
  - **Validators** - FluentValidation rules
- **Common/Behaviors** - Cross-cutting concerns (Validation, Logging, Transaction)
- **Common/DTOs** - Data transfer objects
- **Entities** - Domain models

**Pipeline Behaviors (Execution Order):**
1. **ValidationBehavior** - Validates incoming requests using FluentValidation
2. **LoggingBehavior** - Logs request/response with timing
3. **TransactionBehavior** - Wraps handlers in database transactions

## 📝 Environment Variables

### Backend (.NET)

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | See appsettings.json |
| `JwtSettings__SecretKey` | JWT signing key (min 32 chars) | Required |
| `JwtSettings__Issuer` | JWT issuer | MyPhotoBooth.API |
| `JwtSettings__Audience` | JWT audience | MyPhotoBooth.Client |
| `JwtSettings__AccessTokenExpirationMinutes` | Access token lifetime | 15 |
| `JwtSettings__RefreshTokenExpirationDays` | Refresh token lifetime | 7 |
| `StorageSettings__PhotosBasePath` | Photo storage path | ./storage/photos |
| `StorageSettings__MaxFileSizeMB` | Max upload size | 50 |
| `EmailSettings__SmtpHost` | SMTP server host | localhost (dev) |
| `EmailSettings__SmtpPort` | SMTP server port | 1025 (dev) |
| `EmailSettings__FromEmail` | From email address | noreply@myphotobooth.com |
| `EmailSettings__FromName` | From display name | MyPhotoBooth |

### Frontend (React)

| Variable | Description | Default |
|----------|-------------|---------|
| `VITE_API_BASE_URL` | Backend API URL | http://localhost:5000/api |

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [React](https://react.dev/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
- [TanStack Query](https://tanstack.com/query)
- [Scalar API Documentation](https://scalar.com/)

## 📞 Support

For issues, questions, or contributions, please open an issue on GitHub.

---

Made with ❤️

## 📜 Changelog

### v1.3.0 - CQRS Architecture & Testing (Current)

**Architecture Refactoring:**
- Implemented CQRS pattern using MediatR 14.0
- Separated Commands (writes) and Queries (reads)
- Migrated all services to command/query handlers
- Controllers now use ISender for dispatching requests
- Feature-based folder structure (Features/Auth, Features/Photos, etc.)

**Validation:**
- Integrated FluentValidation 12.1 for declarative validation
- Automatic validation via MediatR pipeline
- Custom validators: Email, Password, StrongPassword, ImageFile, UserName, AlbumName, TagName
- Validation errors return structured error messages

**Pipeline Behaviors:**
- ValidationBehavior - Validates requests before handlers
- LoggingBehavior - Logs all requests with timing information
- TransactionBehavior - Wraps handlers in database transactions
- Behaviors execute in order: Validation → Logging → Transaction

**Error Handling:**
- Implemented Result<T> pattern using CSharpFunctionalExtensions
- Centralized error constants in Errors.cs
- Consistent error responses across all endpoints

**Testing:**
- Added 86 unit tests covering validators, behaviors, and handlers
- Added 31 integration tests covering all API endpoints
- Testcontainers for PostgreSQL in integration tests
- Test coverage: ~70% validators, ~10% handlers, 100% behaviors, 100% API endpoints
- Test frameworks: xUnit, Moq, FluentAssertions

**Infrastructure:**
- Registered MediatR and FluentValidation in DI container
- Pipeline behaviors registered with correct order
- TestWebApplicationFactory for integration testing
- MockEmailService for email testing

### v1.2.0 - Login Flow Completion

**Bug Fixes:**
- Fixed user data consistency on page refresh (localStorage persistence)

**Features:**
- Added forgot password functionality with email tokens
- Added password reset UI and flow
- Implemented PublicRoute component for authenticated user redirects
- Added quick start script (start.sh) for easy development setup
- Added Mailpit integration for email testing in development

**Infrastructure:**
- Created IEmailService interface and EmailService implementation
- Added email DTOs (EmailRequest, EmailResponse)
- Added password reset DTOs (ForgotPasswordRequest, ResetPasswordRequest)
- Registered EmailService in DI container
- Added EmailSettings configuration

**Security:**
- Password reset tokens use ASP.NET Identity cryptographic generation
- User enumeration protection (returns success even for non-existent emails)
- Token expiration for password reset links

### v1.1.0 - Public Sharing Feature

**Features:**
- Token-based public link sharing for photos and albums
- Password protection for sensitive shares
- Expiration date configuration
- Download control (enable/disable)
- Share link revocation
- ShareManagement UI for managing active shares
- Public SharedView for viewing shared content without authentication

**Infrastructure:**
- New ShareLink entity with migration (20260210021155_AddShareLinks)
- Two controllers: ShareLinksController (auth) + SharedController (public)

### v1.0.0 - Initial Release

**Features:**
- JWT authentication with refresh tokens
- Photo upload with drag-and-drop
- Image processing with EXIF extraction
- Albums and tags management
- Timeline view
- Modern gradient UI theme
- Scalar API documentation
