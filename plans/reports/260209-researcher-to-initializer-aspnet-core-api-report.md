# ASP.NET Core Web API Architecture Report - Photo Management Backend

**Date**: 2026-02-09
**Target**: MyPhotoBooth Backend API
**Stack**: ASP.NET Core 8+, PostgreSQL, Entity Framework Core

## 1. Recommended Project Structure

Adopt Clean Architecture with four core layers:

```
MyPhotoBooth.API/
├── src/
│   ├── MyPhotoBooth.Domain/          # Entities, value objects, enums
│   │   ├── Entities/                 # Photo, Album, Tag, User
│   │   └── Specifications/           # Query specifications
│   ├── MyPhotoBooth.Application/     # Business logic, DTOs, interfaces
│   │   ├── Common/                   # DTOs, mappings, behaviors
│   │   ├── Photos/                   # Photo commands/queries (CQRS)
│   │   ├── Albums/
│   │   ├── Tags/
│   │   └── Interfaces/               # IPhotoRepository, IFileStorage
│   ├── MyPhotoBooth.Infrastructure/  # Data access, file I/O, external services
│   │   ├── Persistence/              # DbContext, repositories, migrations
│   │   ├── Storage/                  # Local file system implementation
│   │   └── Identity/                 # ASP.NET Identity configuration
│   └── MyPhotoBooth.API/             # Controllers, middleware, configuration
│       ├── Controllers/
│       ├── Filters/
│       └── Program.cs
```

**Rationale**: Clean Architecture inverts dependencies so infrastructure depends on business logic, not vice versa. This enables testability and maintainability for a photo management system that will grow.

**References**: [Clean Architecture in .NET 8](https://medium.com/@madu.sharadika/clean-architecture-in-net-8-web-api-483979161c80), [Microsoft Common Web Application Architectures](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)

## 2. Authentication & Authorization Approach

**Recommendation**: ASP.NET Core Identity + JWT tokens for SPA authentication

**Implementation Strategy**:
- Use cookie-based authentication for browser clients when possible (default ASP.NET Core Identity behavior)
- Implement JWT tokens for stateless API access with short-lived access tokens (15 minutes) and refresh tokens (7 days)
- Store refresh tokens in database with revocation capability
- Use strong HMAC-SHA256 signing with a secure key from environment variables

**Token Structure**:
```csharp
// Claims to include
- UserId (sub)
- Email
- Roles (admin, user)
- Scopes (photo:read, photo:write, album:manage)
```

**Authorization Strategy**:
- Role-based authorization for admin features (user management)
- Claims-based authorization for photo ownership (users can only modify their own photos)
- Policy-based authorization for album sharing (check membership/permissions)

**Security Best Practices**:
- Never trust client-provided filenames
- Validate JWT signatures on every request
- Implement token rotation for refresh tokens
- Use HTTPS only in production

**References**: [ASP.NET Core Identity API Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-9.0), [ASP.NET Core JWT Best Practices](https://boldsign.com/blogs/aspnet-core-jwt-authentication-guide/)

## 3. Database Design Considerations

**PostgreSQL Integration**:
```bash
# Required NuGet packages
Npgsql.EntityFrameworkCore.PostgreSQL (8.0.4+)
Microsoft.EntityFrameworkCore.Tools
```

**Connection Configuration** (Program.cs):
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Core Entities**:

```csharp
// Photo entity
public class Photo
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; }
    public string StorageKey { get; set; }  // GUID-based key
    public string FilePath { get; set; }
    public string ThumbnailPath { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public DateTime CapturedAt { get; set; }  // From EXIF
    public DateTime UploadedAt { get; set; }
    public string UserId { get; set; }  // FK to AspNetUsers
    public ExifData ExifData { get; set; }  // JSON column
    public ICollection<PhotoTag> PhotoTags { get; set; }
    public ICollection<AlbumPhoto> AlbumPhotos { get; set; }
}

// Album entity
public class Album
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<AlbumPhoto> AlbumPhotos { get; set; }
}

// Tag entity
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<PhotoTag> PhotoTags { get; set; }
}
```

**PostgreSQL-Specific Features to Leverage**:
- JSONB columns for EXIF metadata (flexible schema)
- Full-text search for photo descriptions
- Array types for storing multiple tags efficiently
- Index on UserId, CapturedAt for timeline queries

**References**: [Npgsql EF Core Provider](https://www.npgsql.org/efcore/), [How to Configure PostgreSQL in EF Core](https://code-maze.com/configure-postgresql-ef-core/)

## 4. File Handling Strategy

**Streaming vs. Buffering**:
- Use streaming for files >5MB to avoid memory exhaustion
- Use buffered uploads (IFormFile) for smaller files (<5MB) for simplicity

**Security Validations**:
1. Whitelist allowed MIME types: image/jpeg, image/png, image/heic, image/webp
2. Validate file signatures (magic numbers), not just extensions
3. Limit file size (e.g., 20MB max per photo)
4. Generate storage keys server-side (GUID), never trust client filenames
5. Store files outside wwwroot to prevent direct serving
6. Run antivirus scanning for production environments

**Storage Structure**:
```
/photos/
  /{userId}/
    /{year}/
      /{month}/
        /{guid}.jpg
        /{guid}_thumb.jpg
```

**Implementation Pattern**:
```csharp
// Streaming for large files
[HttpPost("upload")]
[RequestSizeLimit(20_971_520)] // 20MB
public async Task<IActionResult> Upload()
{
    var reader = new MultipartReader(Request.ContentType, Request.Body);
    var section = await reader.ReadNextSectionAsync();

    // Process file without buffering entire content
    var storageKey = Guid.NewGuid().ToString();
    await _fileStorage.SaveStreamAsync(section.Body, storageKey);
}
```

**References**: [Upload Files in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-10.0), [Uploading Large Files](https://code-maze.com/aspnetcore-upload-large-files/)

## 5. Image Processing

**Recommended Library**: SixLabors.ImageSharp (cross-platform, modern, performant)

**Processing Pipeline**:
1. Validate uploaded image
2. Extract EXIF metadata (capture date, camera model, GPS coordinates)
3. Auto-rotate based on EXIF orientation
4. Generate thumbnail (300x300 max, maintain aspect ratio)
5. Compress original if >2MB (JPEG quality 85)
6. Strip sensitive EXIF data (GPS if user preference)
7. Save original and thumbnail to storage

**Sample Implementation**:
```csharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

public async Task<ProcessedPhoto> ProcessImageAsync(Stream imageStream)
{
    using var image = await Image.LoadAsync(imageStream);

    // Extract EXIF
    var exifData = image.Metadata.ExifProfile;
    var capturedAt = exifData?.GetValue(ExifTag.DateTimeOriginal)?.Value;

    // Generate thumbnail
    var thumbnail = image.Clone(x => x.Resize(new ResizeOptions
    {
        Mode = ResizeMode.Max,
        Size = new Size(300, 300)
    }));

    // Save both
    await image.SaveAsJpegAsync(originalPath, new JpegEncoder { Quality = 85 });
    await thumbnail.SaveAsJpegAsync(thumbnailPath, new JpegEncoder { Quality = 80 });
}
```

**Alternative Libraries**:
- Magick.NET (ImageMagick wrapper, more formats)
- MetadataExtractor (EXIF-only, lightweight)

**References**: [.NET Core Image Processing](https://devblogs.microsoft.com/dotnet/net-core-image-processing/), [Best NuGet EXIF Packages](https://nugetmusthaves.com/tag/EXIF)

## 6. API Endpoint Design

**RESTful Principles**:
- Use plural nouns for resources (/photos, /albums, /tags)
- HTTP verbs: GET (read), POST (create), PUT (update), DELETE (remove)
- Limit nesting depth to 2 levels maximum
- Accept and return JSON

**Recommended Endpoints**:

```
Photos:
POST   /api/photos                   # Upload photo(s)
GET    /api/photos                   # List photos (paginated, filtered)
GET    /api/photos/{id}              # Get photo details
PUT    /api/photos/{id}              # Update metadata
DELETE /api/photos/{id}              # Delete photo
GET    /api/photos/{id}/download     # Download original

Albums:
POST   /api/albums                   # Create album
GET    /api/albums                   # List user's albums
GET    /api/albums/{id}              # Get album details
PUT    /api/albums/{id}              # Update album
DELETE /api/albums/{id}              # Delete album
POST   /api/albums/{id}/photos       # Add photos to album
DELETE /api/albums/{id}/photos/{photoId}  # Remove photo from album

Tags:
GET    /api/tags                     # List all tags
POST   /api/photos/{id}/tags         # Add tag to photo
DELETE /api/photos/{id}/tags/{tagId} # Remove tag from photo

Timeline:
GET    /api/timeline                 # Get photos chronologically
  Query params: ?year=2026&month=2&userId={id}
```

**Pagination Pattern**:
```csharp
GET /api/photos?page=1&pageSize=50&sortBy=capturedAt&order=desc
```

**Filtering Pattern**:
```csharp
GET /api/photos?albumId={id}&tags=vacation,family&startDate=2026-01-01
```

**Response Format**:
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalCount": 250,
    "totalPages": 5
  }
}
```

**References**: [Best Practices for REST API Design](https://stackoverflow.blog/2020/03/02/best-practices-for-rest-api-design/), [Web API Design Best Practices](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)

## 7. Action Items

1. Initialize solution with Clean Architecture template
2. Install NuGet packages: Npgsql.EntityFrameworkCore.PostgreSQL, SixLabors.ImageSharp, Microsoft.AspNetCore.Identity
3. Configure PostgreSQL connection string in appsettings.json
4. Create domain entities (Photo, Album, Tag) and DbContext
5. Implement ASP.NET Identity with JWT authentication
6. Create file storage service interface and local implementation
7. Build photo upload endpoint with streaming and validation
8. Implement image processing pipeline with thumbnail generation
9. Create RESTful endpoints for albums, tags, and timeline
10. Add pagination, filtering, and sorting to photo list endpoint
