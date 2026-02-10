# Public Sharing Feature (v1.1.0)

Comprehensive documentation for the public sharing feature that allows users to share photos and albums via secure, token-based public links.

## Overview

The public sharing feature enables users to create shareable links for their photos and albums. These links can be accessed by anyone without requiring authentication, with optional security features like password protection and expiration dates.

### Key Features

- Token-based secure sharing (32-byte cryptographically random tokens)
- Password protection for sensitive content
- Configurable expiration dates
- Download control (enable/disable per share)
- Instant link revocation
- Share link management interface
- Public viewing without authentication

## Architecture

### Entity Model

**ShareLink Entity** (`MyPhotoBooth.Domain/Entities/ShareLink.cs`):

```csharp
public class ShareLink
{
    public Guid Id { get; set; }
    public string Token { get; set; }              // Base64url-encoded 32-byte token
    public string UserId { get; set; }             // FK to AspNetUsers
    public ShareLinkType Type { get; set; }        // Photo or Album
    public Guid? PhotoId { get; set; }             // FK to Photos (nullable)
    public Guid? AlbumId { get; set; }             // FK to Albums (nullable)
    public string? PasswordHash { get; set; }      // Hashed password (nullable)
    public DateTime? ExpiresAt { get; set; }       // Expiration date (nullable)
    public bool AllowDownload { get; set; }        // Download control
    public DateTime CreatedAt { get; set; }        // Creation timestamp
    public DateTime? RevokedAt { get; set; }       // Revocation timestamp (nullable)

    // Computed properties (ignored by EF Core)
    public bool IsExpired { get; }                 // Check if expired
    public bool IsRevoked { get; }                 // Check if revoked
    public bool IsActive { get; }                  // Check if active

    // Navigation properties
    public Photo? Photo { get; set; }
    public Album? Album { get; set; }
}
```

**ShareLinkType Enum**:
```csharp
public enum ShareLinkType
{
    Photo = 0,
    Album = 1
}
```

### Database Schema

**ShareLinks Table** (Migration: `20260210021155_AddShareLinks`):

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | UUID | No | Primary key |
| Token | varchar | No | Unique share token (indexed) |
| UserId | varchar | No | User who created share (FK, indexed) |
| Type | int | No | 0=Photo, 1=Album |
| PhotoId | UUID | Yes | Foreign key to Photos |
| AlbumId | UUID | Yes | Foreign key to Albums |
| PasswordHash | text | Yes | Hashed password |
| ExpiresAt | timestamp | Yes | Expiration date/time |
| AllowDownload | bool | No | Download permission |
| CreatedAt | timestamp | No | Creation timestamp |
| RevokedAt | timestamp | Yes | Revocation timestamp |

**Indexes**:
- Unique index on `Token`
- Index on `UserId` for user queries
- Foreign key indexes on `PhotoId` and `AlbumId`

**Constraints**:
- One of `PhotoId` or `AlbumId` must be set (enforced at application level)
- `PhotoId` is set when `Type = Photo`
- `AlbumId` is set when `Type = Album`

### Backend Implementation

#### Controllers

**ShareLinksController** (Authenticated):

Handles share link management for authenticated users.

```
POST   /api/sharelinks           Create new share link
GET    /api/sharelinks           List user's share links
DELETE /api/sharelinks/{id}      Revoke share link
```

**SharedController** (Public):

Handles public access to shared content (no authentication required).

```
GET    /api/shared/{token}                        Get share metadata
POST   /api/shared/{token}/access                 Access shared content with password
GET    /api/shared/{token}/photos/{id}/file       Download shared photo
GET    /api/shared/{token}/photos/{id}/thumbnail  Get shared thumbnail
```

#### Token Generation

Tokens are generated using `RandomNumberGenerator` for cryptographic security:

```csharp
private static string GenerateToken()
{
    var bytes = RandomNumberGenerator.GetBytes(32);
    return Convert.ToBase64String(bytes)
        .Replace("+", "-")
        .Replace("/", "_")
        .TrimEnd('=');
}
```

Result: 43-character URL-safe base64url string (e.g., `dGhpc19pc19hX3Rlc3RfdG9rZW5fZm9yX2V4YW1wbGU`)

#### Password Protection

Passwords are hashed using ASP.NET Identity's `PasswordHasher<object>`:

```csharp
// Hash password when creating share link
shareLink.PasswordHash = _passwordHasher.HashPassword(new object(), request.Password);

// Verify password when accessing shared content
var result = _passwordHasher.VerifyHashedPassword(new object(), shareLink.PasswordHash, request.Password);
if (result == PasswordVerificationResult.Failed)
    return Unauthorized();
```

#### Validation Logic

**Share Link Validation** (checked on every access):
1. Token exists in database
2. Not expired (`ExpiresAt` is null or future date)
3. Not revoked (`RevokedAt` is null)
4. Password correct (if `PasswordHash` is set)

**Ownership Validation** (when creating/revoking):
1. User owns the photo or album being shared
2. User owns the share link being revoked

### Frontend Implementation

#### Components

**ShareModal** (`src/client/src/features/sharing/ShareModal.tsx`):

Dialog for creating share links with options:
- Photo or album selection (passed as prop)
- Password protection toggle and input field
- Expiration date picker
- Download control toggle
- Generated shareable URL with copy-to-clipboard button
- Success/error feedback

**ShareManagement** (`src/client/src/features/sharing/ShareManagement.tsx`):

Page for managing all active share links:
- List view of all user's share links
- Display: type (photo/album), target name, creation date, expiration, status
- Status indicators: active (green), expired (orange), revoked (red)
- Revoke button with confirmation dialog
- Empty state when no shares exist
- Responsive grid layout

**SharedView** (`src/client/src/features/sharing/SharedView.tsx`):

Public page for viewing shared content:
- Fetches share metadata by token from URL
- Password prompt for protected shares
- Displays single photo or album grid
- Download button (if allowed)
- Error states: expired, revoked, not found
- No authentication required
- Public layout (no sidebar)

**SharedPhotoGrid** (`src/client/src/features/sharing/SharedPhotoGrid.tsx`):

Grid view for displaying photos in shared albums:
- Responsive grid layout
- Thumbnail display with aspect ratio
- Click to open SharedLightbox
- Loading states

**SharedLightbox** (`src/client/src/features/sharing/SharedLightbox.tsx`):

Lightbox for viewing shared photos:
- Full-screen photo display
- Navigation controls for albums
- Photo metadata display
- Download button (if allowed)
- Close button

#### Hooks

**useShareLinks** (`src/client/src/hooks/useShareLinks.ts`):

TanStack Query hooks for share link management:

```typescript
// Fetch user's share links
const { data: shareLinks } = useShareLinks();

// Create share link
const { mutate: createShare } = useCreateShareLink();

// Revoke share link
const { mutate: revokeShare } = useRevokeShareLink();
```

**useSharedContent** (`src/client/src/hooks/useSharedContent.ts`):

TanStack Query hooks for accessing shared content:

```typescript
// Fetch share metadata (public)
const { data: metadata } = useSharedMetadata(token);

// Access shared content with password (public)
const { mutate: accessContent } = useAccessSharedContent(token);
```

#### Routes

```typescript
// Public route (no authentication)
<Route path="/shared/:token" element={<SharedView />} />

// Protected route (authentication required)
<Route path="/shares" element={<ShareManagement />} />
```

#### API Integration

**Authenticated API** (`api` instance):
- Used for share link management (create, list, revoke)
- Includes JWT token in Authorization header
- Automatic token refresh on 401

**Public API** (`publicApi` instance):
- Used for accessing shared content
- No authentication headers
- Separate base URL configuration

```typescript
// In api.ts
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  // ... with auth interceptors
});

export const publicApi = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  // ... no auth interceptors
});
```

## API Reference

### Create Share Link

**Endpoint**: `POST /api/sharelinks`
**Authentication**: Required (JWT Bearer token)

**Request Body**:
```json
{
  "type": 0,                          // 0=Photo, 1=Album
  "photoId": "uuid",                  // Required if type=0
  "albumId": "uuid",                  // Required if type=1
  "password": "optional-password",    // Optional password protection
  "expiresAt": "2026-03-01T00:00:00Z", // Optional expiration date
  "allowDownload": true               // Optional, default true
}
```

**Response** (200 OK):
```json
{
  "id": "share-link-uuid",
  "token": "dGhpc19pc19hX3Rlc3RfdG9rZW4",
  "type": 0,
  "photoId": "photo-uuid",
  "albumId": null,
  "hasPassword": true,
  "expiresAt": "2026-03-01T00:00:00Z",
  "allowDownload": true,
  "shareUrl": "https://yourdomain.com/shared/dGhpc19pc19hX3Rlc3RfdG9rZW4",
  "isActive": true,
  "createdAt": "2026-02-10T10:00:00Z"
}
```

### List Share Links

**Endpoint**: `GET /api/sharelinks`
**Authentication**: Required

**Response** (200 OK):
```json
[
  {
    "id": "share-link-uuid",
    "token": "dGhpc19pc19hX3Rlc3RfdG9rZW4",
    "type": 0,
    "photoId": "photo-uuid",
    "albumId": null,
    "targetName": "vacation.jpg",
    "hasPassword": true,
    "expiresAt": "2026-03-01T00:00:00Z",
    "allowDownload": true,
    "shareUrl": "https://yourdomain.com/shared/dGhpc19pc19hX3Rlc3RfdG9rZW4",
    "isActive": true,
    "createdAt": "2026-02-10T10:00:00Z"
  }
]
```

### Revoke Share Link

**Endpoint**: `DELETE /api/sharelinks/{id}`
**Authentication**: Required

**Response**: 204 No Content

### Get Share Metadata

**Endpoint**: `GET /api/shared/{token}`
**Authentication**: None (public)

**Response** (200 OK):
```json
{
  "type": 0,
  "hasPassword": true,
  "isExpired": false,
  "isActive": true
}
```

### Access Shared Content

**Endpoint**: `POST /api/shared/{token}/access`
**Authentication**: None (public)

**Request Body**:
```json
{
  "password": "optional-password"  // Required if hasPassword=true
}
```

**Response** (200 OK) - Single Photo:
```json
{
  "type": 0,
  "photo": {
    "id": "photo-uuid",
    "fileName": "vacation.jpg",
    "width": 1920,
    "height": 1080,
    "capturedAt": "2026-02-01T10:00:00Z",
    "uploadedAt": "2026-02-10T10:00:00Z",
    "description": "Beach vacation",
    "allowDownload": true
  }
}
```

**Response** (200 OK) - Album:
```json
{
  "type": 1,
  "album": {
    "name": "Vacation 2026",
    "description": "Summer vacation photos",
    "allowDownload": true,
    "photos": [
      {
        "id": "photo-uuid-1",
        "fileName": "beach.jpg",
        "width": 1920,
        "height": 1080,
        "capturedAt": "2026-02-01T10:00:00Z",
        "uploadedAt": "2026-02-10T10:00:00Z",
        "description": null,
        "allowDownload": true
      }
    ]
  }
}
```

**Error Responses**:
- `401 Unauthorized`: Invalid password
- `404 Not Found`: Share link not found
- `410 Gone`: Share link expired or revoked

### Download Shared Photo

**Endpoint**: `GET /api/shared/{token}/photos/{photoId}/file`
**Authentication**: None (public)

**Response**: Image file (Content-Type: image/jpeg, image/png, etc.)

**Error Responses**:
- `403 Forbidden`: Downloads not allowed for this share
- `404 Not Found`: Photo not found or not part of share

### Get Shared Thumbnail

**Endpoint**: `GET /api/shared/{token}/photos/{photoId}/thumbnail`
**Authentication**: None (public)

**Response**: JPEG thumbnail image (300x300px max)

## Security Considerations

### Token Security

1. **Generation**: Uses `RandomNumberGenerator` for cryptographic randomness
2. **Length**: 32 bytes (256 bits) of entropy, encoded to 43 characters
3. **Format**: Base64url encoding (URL-safe, no special characters)
4. **Uniqueness**: Database enforces unique constraint on Token column

### Password Protection

1. **Hashing**: Uses ASP.NET Identity `PasswordHasher<object>` with:
   - PBKDF2 algorithm
   - Per-password salt
   - Multiple iterations
2. **Verification**: Constant-time comparison to prevent timing attacks
3. **Storage**: Only hash stored, never plaintext

### Access Control

1. **Ownership**: Users can only create shares for content they own
2. **Revocation**: Users can only revoke their own shares
3. **Validation**: Every access validates token, expiration, revocation, and password
4. **Download Control**: Server enforces download permission on file endpoints

### Best Practices

1. Always use HTTPS in production to protect tokens in transit
2. Consider short expiration times for sensitive content
3. Use password protection for private shares
4. Revoke shares immediately when no longer needed
5. Monitor share access logs (future enhancement)

## Usage Examples

### Creating a Share Link (Frontend)

```typescript
const { mutate: createShare } = useCreateShareLink();

createShare({
  type: ShareLinkType.Photo,
  photoId: selectedPhoto.id,
  password: 'secret123',
  expiresAt: new Date('2026-03-01'),
  allowDownload: true
}, {
  onSuccess: (data) => {
    // Copy share URL to clipboard
    navigator.clipboard.writeText(data.shareUrl);
    toast.success('Share link created!');
  }
});
```

### Accessing Shared Content (Frontend)

```typescript
const { token } = useParams();
const { data: metadata } = useSharedMetadata(token);
const { mutate: accessContent } = useAccessSharedContent(token);

// If password protected
if (metadata.hasPassword) {
  accessContent({ password: userInputPassword }, {
    onSuccess: (content) => {
      // Display shared content
      setSharedContent(content);
    },
    onError: () => {
      toast.error('Incorrect password');
    }
  });
}
```

### Revoking a Share Link (Frontend)

```typescript
const { mutate: revokeShare } = useRevokeShareLink();

revokeShare(shareLinkId, {
  onSuccess: () => {
    toast.success('Share link revoked');
    queryClient.invalidateQueries(['shareLinks']);
  }
});
```

## Testing

### Manual Testing Checklist

- [ ] Create share link for photo without password
- [ ] Create share link for photo with password
- [ ] Create share link for album
- [ ] Set expiration date and verify expiration
- [ ] Access shared photo without authentication
- [ ] Access password-protected share with correct password
- [ ] Access password-protected share with incorrect password
- [ ] Download shared photo when allowed
- [ ] Verify download blocked when not allowed
- [ ] Revoke share link and verify access denied
- [ ] Test expired share link
- [ ] Copy share URL and test in incognito browser
- [ ] Test share URL on mobile device

### Security Testing

- [ ] Verify token uniqueness (create multiple shares)
- [ ] Test token guessing resistance (try random tokens)
- [ ] Verify password hash cannot be reversed
- [ ] Test unauthorized revocation attempt
- [ ] Test sharing content not owned by user
- [ ] Verify HTTPS enforcement in production
- [ ] Test XSS in share descriptions
- [ ] Test SQL injection in token parameter

## Performance Considerations

1. **Database Queries**:
   - Token lookups are indexed for fast retrieval
   - Include eager loading of Photo/Album/AlbumPhotos to avoid N+1
   - Consider caching share metadata for frequently accessed links

2. **File Serving**:
   - Thumbnails served directly via streaming
   - Full images served with proper content-type headers
   - Consider CDN for high-traffic shared content

3. **Cleanup**:
   - Implement background job to delete expired shares
   - Archive revoked shares instead of hard delete (audit trail)

## Future Enhancements

- [ ] Share usage analytics (view count, last accessed)
- [ ] Email notification when share is accessed
- [ ] Watermark images in shared content
- [ ] Batch share creation
- [ ] Custom share URL slugs
- [ ] Share templates with preset options
- [ ] QR code generation for shares
- [ ] Social media preview metadata
- [ ] Share expiration reminders
- [ ] Share access logs

## Troubleshooting

### Common Issues

**Issue**: Share link returns 404
- **Solution**: Check if link was revoked or expired. Verify token is correct.

**Issue**: Password prompt not showing
- **Solution**: Verify share link has `hasPassword: true` in metadata response.

**Issue**: Download button not working
- **Solution**: Check if `allowDownload: false` in share settings. Verify CORS configuration.

**Issue**: Thumbnail not loading
- **Solution**: Check file storage path. Verify public API client is used (not authenticated client).

## References

- Clean Architecture: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- ASP.NET Identity PasswordHasher: https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing
- TanStack Query: https://tanstack.com/query/latest
- RandomNumberGenerator: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator
