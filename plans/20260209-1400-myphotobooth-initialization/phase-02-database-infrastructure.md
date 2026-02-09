# Phase 02: Database & Infrastructure

## Context Links

- [Main Plan](./plan.md)
- [Tech Stack - Database Section](../../docs/tech-stack.md)
- [ASP.NET Core API Report - Section 3: Database Design](../reports/260209-researcher-to-initializer-aspnet-core-api-report.md)

---

## Overview

| Field | Value |
|-------|-------|
| Date | 2026-02-09 |
| Priority | Critical - Required by all API phases |
| Status | pending |
| Progress | 0% |
| Estimated Effort | 3-4 hours |
| Depends On | Phase 01 (Project Setup) |

---

## Key Insights

- PostgreSQL JSONB columns allow flexible EXIF metadata storage without schema changes
- GUID primary keys provide security (non-sequential) and distributed-system compatibility
- EF Core migrations manage schema evolution in a version-controlled manner
- Indexes on UserId, CapturedAt, and TagId are critical for timeline and gallery query performance
- Many-to-many relationships (PhotoTag, AlbumPhoto) require explicit junction tables in EF Core
- Connection strings must never be committed; use environment variables or user secrets

---

## Requirements

1. Set up PostgreSQL database connection via Npgsql provider
2. Create domain entities: Photo, Album, Tag, PhotoTag, AlbumPhoto
3. Configure EF Core DbContext with PostgreSQL-specific features (JSONB, indexes)
4. Create initial database migration
5. Implement repository interfaces in the Application layer
6. Implement concrete repositories in the Infrastructure layer
7. Configure ASP.NET Core Identity tables in the same database

---

## Architecture

### Entity Relationships
```
User (1) ----< (many) Photo
User (1) ----< (many) Album
Photo (many) >---< (many) Tag      [via PhotoTag]
Album (many) >---< (many) Photo    [via AlbumPhoto]
Album (1) ----> (0..1) Photo       [CoverPhoto]
```

### Core Entities

```csharp
Photo: Id (Guid), OriginalFileName, StorageKey, FilePath, ThumbnailPath,
       FileSize, ContentType, CapturedAt, UploadedAt, Description,
       UserId (FK), ExifData (JSONB)

Album: Id (Guid), Name, Description, CoverPhotoId (FK nullable),
       UserId (FK), CreatedAt, UpdatedAt

Tag: Id (Guid), Name, UserId (FK), CreatedAt

PhotoTag: PhotoId (FK), TagId (FK)
AlbumPhoto: AlbumId (FK), PhotoId (FK), AddedAt, SortOrder
```

### Database Indexes
- `Photo`: Composite index on (UserId, CapturedAt DESC) for timeline queries
- `Photo`: Index on UserId for user photo listing
- `Tag`: Unique index on (Name, UserId) to prevent duplicate tag names per user
- `AlbumPhoto`: Composite index on (AlbumId, SortOrder)

---

## Related Code Files

| File | Action | Purpose |
|------|--------|---------|
| `src/MyPhotoBooth.Domain/Entities/Photo.cs` | Create | Photo entity |
| `src/MyPhotoBooth.Domain/Entities/Album.cs` | Create | Album entity |
| `src/MyPhotoBooth.Domain/Entities/Tag.cs` | Create | Tag entity |
| `src/MyPhotoBooth.Domain/Entities/PhotoTag.cs` | Create | Junction entity |
| `src/MyPhotoBooth.Domain/Entities/AlbumPhoto.cs` | Create | Junction entity |
| `src/MyPhotoBooth.Domain/Entities/ExifData.cs` | Create | EXIF value object |
| `src/MyPhotoBooth.Application/Interfaces/IPhotoRepository.cs` | Create | Repository interface |
| `src/MyPhotoBooth.Application/Interfaces/IAlbumRepository.cs` | Create | Repository interface |
| `src/MyPhotoBooth.Application/Interfaces/ITagRepository.cs` | Create | Repository interface |
| `src/MyPhotoBooth.Infrastructure/Persistence/AppDbContext.cs` | Create | EF Core DbContext |
| `src/MyPhotoBooth.Infrastructure/Persistence/Configurations/` | Create | Entity configurations |
| `src/MyPhotoBooth.Infrastructure/Persistence/Repositories/` | Create | Repository implementations |
| `src/MyPhotoBooth.Infrastructure/Persistence/Migrations/` | Create | EF Core migrations |
| `src/MyPhotoBooth.API/appsettings.json` | Modify | Connection string placeholder |

---

## Implementation Steps

1. **Create domain entities**
   - Define `Photo`, `Album`, `Tag`, `PhotoTag`, `AlbumPhoto` entities in Domain/Entities/
   - Create `ExifData` class for JSONB storage (CameraModel, FocalLength, ISO, Aperture, GPS, etc.)
   - Use GUID primary keys for all entities
   - Add navigation properties for EF Core relationships

2. **Create repository interfaces in Application layer**
   - `IPhotoRepository` with methods: GetByIdAsync, GetByUserIdAsync (paginated), AddAsync, UpdateAsync, DeleteAsync
   - `IAlbumRepository` with methods: GetByIdAsync, GetByUserIdAsync, AddAsync, UpdateAsync, DeleteAsync
   - `ITagRepository` with methods: GetByIdAsync, GetByUserIdAsync, GetOrCreateAsync, SearchAsync
   - `IUnitOfWork` interface for transaction management

3. **Configure AppDbContext**
   - Inherit from `IdentityDbContext` to integrate ASP.NET Identity tables
   - Register DbSets for Photo, Album, Tag, PhotoTag, AlbumPhoto
   - Configure JSONB column for ExifData using `.HasColumnType("jsonb")`
   - Configure many-to-many relationships with composite keys
   - Apply entity configurations using `IEntityTypeConfiguration<T>`

4. **Create entity configurations**
   - `PhotoConfiguration`: Indexes on (UserId, CapturedAt), required fields, JSONB mapping
   - `AlbumConfiguration`: Relationship to CoverPhoto, index on UserId
   - `TagConfiguration`: Unique index on (Name, UserId)
   - `PhotoTagConfiguration`: Composite primary key (PhotoId, TagId)
   - `AlbumPhotoConfiguration`: Composite primary key (AlbumId, PhotoId)

5. **Implement concrete repositories**
   - Implement each repository interface in Infrastructure/Persistence/Repositories/
   - Use `AppDbContext` for data access
   - Implement pagination using `Skip()` and `Take()` with total count
   - Include navigation properties with `.Include()` where needed

6. **Configure dependency injection**
   - Create `DependencyInjection.cs` in Infrastructure project
   - Register DbContext with Npgsql provider
   - Register all repository implementations
   - Configure connection string from configuration

7. **Create initial migration**
   - Run `dotnet ef migrations add InitialCreate` targeting Infrastructure project
   - Review generated migration for correctness
   - Test migration with `dotnet ef database update`

8. **Seed initial data (optional)**
   - Create a data seeding class for development/testing
   - Seed default admin user, sample tags

---

## Todo List

- [ ] Create Photo entity with all properties
- [ ] Create Album entity with all properties
- [ ] Create Tag entity with all properties
- [ ] Create PhotoTag junction entity
- [ ] Create AlbumPhoto junction entity
- [ ] Create ExifData value object class
- [ ] Create IPhotoRepository interface
- [ ] Create IAlbumRepository interface
- [ ] Create ITagRepository interface
- [ ] Create IUnitOfWork interface
- [ ] Create AppDbContext inheriting IdentityDbContext
- [ ] Configure entity relationships and JSONB columns
- [ ] Create entity configuration classes
- [ ] Add database indexes for performance
- [ ] Implement PhotoRepository
- [ ] Implement AlbumRepository
- [ ] Implement TagRepository
- [ ] Implement UnitOfWork
- [ ] Configure dependency injection for Infrastructure
- [ ] Create initial EF Core migration
- [ ] Test migration against local PostgreSQL
- [ ] Verify all relationships and indexes in database

---

## Success Criteria

- `dotnet ef database update` runs successfully and creates all tables
- PostgreSQL database contains: Photos, Albums, Tags, PhotoTags, AlbumPhotos, and ASP.NET Identity tables
- JSONB column is created for ExifData on the Photos table
- All indexes are present (verify with `\di` in psql)
- Repository methods compile and resolve via dependency injection
- Unit tests for repository operations pass (if created)

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| PostgreSQL not running locally | Medium | High | Document Docker setup as alternative |
| EF Core migration conflicts | Low | Medium | Generate clean migrations, test early |
| JSONB serialization issues | Medium | Medium | Test ExifData round-trip serialization |
| Missing indexes causing slow queries | Low | High | Define indexes upfront in configurations |
| Identity table naming conflicts | Low | Medium | Use explicit table name configuration |

---

## Security Considerations

- Connection strings stored in environment variables or user secrets, never in source control
- Use parameterized queries (EF Core does this by default) to prevent SQL injection
- GUID primary keys prevent sequential ID enumeration attacks
- Database user should have minimum required permissions (not superuser)
- Consider encrypting sensitive EXIF data (GPS coordinates) at rest

---

## Next Steps

After completing this phase, proceed to:
- [Phase 03: Backend Authentication](./phase-03-backend-authentication.md) - Configure ASP.NET Identity and JWT token generation
