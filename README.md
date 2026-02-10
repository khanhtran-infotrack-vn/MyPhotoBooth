# 📸 MyPhotoBooth

A modern photo memories application for storing and viewing photos with friends and family. Built with React and ASP.NET Core.

![MyPhotoBooth](https://img.shields.io/badge/Version-1.1.0-blue)
![React](https://img.shields.io/badge/React-18-61dafb)
![.NET](https://img.shields.io/badge/.NET-10.0-512bd4)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)

## ✨ Features

- 🔐 **Secure Authentication** - Email/password authentication with JWT tokens
- 📸 **Photo Management** - Upload, view, organize, and delete photos
- 🖼️ **Smart Image Processing** - Auto-rotation, EXIF extraction, thumbnail generation
- 📁 **Albums & Tags** - Organize photos into albums and tag them
- 📅 **Timeline View** - Browse photos by date
- 🔗 **Public Sharing** - Share photos and albums via secure public links
- 🎨 **Modern UI** - Beautiful, responsive design with smooth animations
- 🚀 **High Performance** - Optimized image loading with blob URLs
- 📱 **Responsive** - Works on desktop, tablet, and mobile

## 🏗️ Architecture

### Clean Architecture Pattern

```
MyPhotoBooth/
├── src/
│   ├── MyPhotoBooth.API/          # Presentation Layer (ASP.NET Core Web API)
│   ├── MyPhotoBooth.Application/  # Application Layer (DTOs, Interfaces)
│   ├── MyPhotoBooth.Infrastructure/# Infrastructure Layer (EF Core, Services)
│   ├── MyPhotoBooth.Domain/       # Domain Layer (Entities, Business Logic)
│   └── client/                     # React Frontend (TypeScript + Vite)
```

### Technology Stack

**Backend:**
- ASP.NET Core 10.0 Web API
- Entity Framework Core 10.0
- PostgreSQL 16
- ASP.NET Identity (Authentication)
- JWT Bearer Authentication
- SixLabors.ImageSharp (Image Processing)
- Scalar API Documentation

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

### API Endpoints

**Authentication:**
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout

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
# Backend tests
dotnet test

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

- **Controllers** - API endpoints
- **Services** - Business logic
- **Repositories** - Data access
- **DTOs** - Data transfer objects
- **Entities** - Domain models

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

Made with ❤️ by [Your Name]
