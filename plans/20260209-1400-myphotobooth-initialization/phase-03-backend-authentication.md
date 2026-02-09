# Phase 03: Backend Authentication

## Context Links

- [Main Plan](./plan.md)
- [Tech Stack - Authentication Section](../../docs/tech-stack.md)
- [ASP.NET Core API Report - Section 2: Authentication](../reports/260209-researcher-to-initializer-aspnet-core-api-report.md)
- [React SPA Report - Section 7: Authentication Strategy](../reports/260209-researcher-to-initializer-react-spa-architecture-report.md)

---

## Overview

| Field | Value |
|-------|-------|
| Date | 2026-02-09 |
| Priority | Critical - Required before any protected endpoints |
| Status | pending |
| Progress | 0% |
| Estimated Effort | 4-5 hours |
| Depends On | Phase 02 (Database & Infrastructure) |

---

## Key Insights

- ASP.NET Core Identity provides battle-tested user management (registration, password hashing, lockout policies)
- JWT access tokens should be short-lived (15 minutes) to minimize compromise window
- Refresh tokens (7 days) stored in database enable revocation for logout and security incidents
- Token rotation on refresh prevents replay attacks
- Frontend route protection is UX-only; server must validate every request independently
- HMAC-SHA256 signing key must come from environment variables, never hardcoded

---

## Requirements

1. Configure ASP.NET Core Identity for user registration and login
2. Implement JWT access token generation with proper claims
3. Implement refresh token generation, storage, and rotation
4. Create authentication endpoints (register, login, refresh, logout)
5. Configure authorization policies (role-based, claims-based, resource-based)
6. Add middleware for JWT validation on protected routes
7. Implement proper error responses for authentication failures

---

## Architecture

### Authentication Flow
```
1. User registers -> POST /api/auth/register -> 201 Created
2. User logs in   -> POST /api/auth/login    -> { accessToken, refreshToken }
3. API calls      -> Authorization: Bearer {accessToken}
4. Token expires  -> POST /api/auth/refresh  -> { newAccessToken, newRefreshToken }
5. User logs out  -> POST /api/auth/logout   -> Revoke refresh token
```

### Token Structure
```
Access Token (JWT - 15 min):
  - sub: userId (GUID)
  - email: user@email.com
  - roles: ["User"]
  - iat, exp, iss, aud

Refresh Token (Opaque - 7 days):
  - Stored in database: Id, Token, UserId, ExpiresAt, CreatedAt, RevokedAt, ReplacedByToken
```

### Authorization Policies
```csharp
"PhotoOwner"  -> User can only access/modify their own photos
"AlbumOwner"  -> User can only access/modify their own albums
"AdminOnly"   -> Reserved for administrative operations
```

---

## Related Code Files

| File | Action | Purpose |
|------|--------|---------|
| `src/MyPhotoBooth.Domain/Entities/ApplicationUser.cs` | Create | Extended Identity user |
| `src/MyPhotoBooth.Domain/Entities/RefreshToken.cs` | Create | Refresh token entity |
| `src/MyPhotoBooth.Application/Interfaces/IAuthService.cs` | Create | Auth service interface |
| `src/MyPhotoBooth.Application/Interfaces/ITokenService.cs` | Create | Token generation interface |
| `src/MyPhotoBooth.Application/Common/DTOs/AuthDTOs.cs` | Create | Request/response DTOs |
| `src/MyPhotoBooth.Infrastructure/Identity/AuthService.cs` | Create | Auth service implementation |
| `src/MyPhotoBooth.Infrastructure/Identity/TokenService.cs` | Create | JWT/refresh token service |
| `src/MyPhotoBooth.API/Controllers/AuthController.cs` | Create | Authentication endpoints |
| `src/MyPhotoBooth.API/Program.cs` | Modify | Configure auth middleware |
| `src/MyPhotoBooth.Infrastructure/Persistence/AppDbContext.cs` | Modify | Add RefreshToken DbSet |

---

## Implementation Steps

1. **Extend ApplicationUser**
   - Create `ApplicationUser : IdentityUser` in Domain/Entities/
   - Add properties: DisplayName, ProfilePhotoPath, CreatedAt
   - Add navigation property for RefreshTokens collection

2. **Create RefreshToken entity**
   - Properties: Id (Guid), Token (string), UserId (FK), ExpiresAt, CreatedAt, RevokedAt, ReplacedByToken
   - Add IsExpired and IsActive computed properties
   - Configure in AppDbContext with index on Token for fast lookup

3. **Create authentication DTOs**
   - `RegisterRequest`: Email, Password, ConfirmPassword, DisplayName
   - `LoginRequest`: Email, Password
   - `AuthResponse`: AccessToken, RefreshToken, ExpiresAt
   - `RefreshTokenRequest`: RefreshToken
   - Validation attributes on request DTOs (Required, EmailAddress, MinLength)

4. **Implement ITokenService**
   - `GenerateAccessToken(ApplicationUser, IList<string> roles)` -> JWT string
   - `GenerateRefreshToken()` -> random 64-byte base64 string
   - Configure JWT settings from `appsettings.json`: Issuer, Audience, SecretKey, AccessTokenExpiration, RefreshTokenExpiration
   - Use `SecurityTokenDescriptor` with HMAC-SHA256 signing

5. **Implement IAuthService**
   - `RegisterAsync(RegisterRequest)`: Create user via UserManager, generate tokens
   - `LoginAsync(LoginRequest)`: Validate credentials, generate tokens, store refresh token
   - `RefreshTokenAsync(string refreshToken)`: Validate refresh token, rotate (revoke old, issue new), generate new access token
   - `LogoutAsync(string refreshToken)`: Revoke refresh token
   - Handle errors: duplicate email, invalid credentials, expired/revoked tokens

6. **Create AuthController**
   - `POST /api/auth/register` -> RegisterAsync
   - `POST /api/auth/login` -> LoginAsync
   - `POST /api/auth/refresh` -> RefreshTokenAsync
   - `POST /api/auth/logout` -> LogoutAsync (requires authentication)
   - Return appropriate HTTP status codes (201, 200, 401, 400)

7. **Configure JWT authentication in Program.cs**
   - Add `Authentication` with JwtBearer scheme
   - Configure `TokenValidationParameters`: validate issuer, audience, lifetime, signing key
   - Add `Authorization` with default policy requiring authenticated users
   - Register custom authorization policies (PhotoOwner, AlbumOwner)

8. **Create authorization handlers**
   - `PhotoOwnerAuthorizationHandler`: Verify UserId matches photo owner
   - `AlbumOwnerAuthorizationHandler`: Verify UserId matches album owner
   - Register handlers in dependency injection

9. **Add JWT configuration to appsettings**
   - Create `JwtSettings` section with placeholder values
   - Use environment variables for production secret key
   - Document required environment variables

---

## Todo List

- [ ] Create ApplicationUser entity extending IdentityUser
- [ ] Create RefreshToken entity
- [ ] Update AppDbContext with RefreshToken DbSet and configuration
- [ ] Create authentication request/response DTOs with validation
- [ ] Create ITokenService interface
- [ ] Create IAuthService interface
- [ ] Implement TokenService (JWT + refresh token generation)
- [ ] Implement AuthService (register, login, refresh, logout)
- [ ] Create AuthController with all endpoints
- [ ] Configure JWT authentication in Program.cs
- [ ] Configure authorization policies
- [ ] Create resource-based authorization handlers
- [ ] Add JwtSettings to appsettings.json (placeholder values)
- [ ] Create EF Core migration for RefreshToken table
- [ ] Test registration endpoint
- [ ] Test login endpoint returns valid JWT
- [ ] Test refresh token rotation
- [ ] Test logout revokes refresh token
- [ ] Test protected endpoints reject unauthenticated requests
- [ ] Test expired tokens are properly rejected

---

## Success Criteria

- User can register with email and password via POST /api/auth/register
- User can log in and receive access token + refresh token via POST /api/auth/login
- Access token contains correct claims (sub, email, roles)
- Protected endpoints return 401 for unauthenticated requests
- Refresh token rotation works correctly (old token revoked, new token issued)
- Logout revokes the refresh token in the database
- Expired access tokens are rejected with 401
- Invalid/revoked refresh tokens return 401
- Password hashing uses bcrypt (ASP.NET Identity default)

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| JWT secret key exposure | Low | Critical | Use environment variables, never commit secrets |
| Token replay attacks | Medium | High | Short access token lifetime, refresh token rotation |
| Brute force login attempts | Medium | Medium | ASP.NET Identity lockout policy (5 attempts, 15 min) |
| Refresh token theft | Low | High | Store securely, rotate on use, allow revocation |
| Clock skew causing token issues | Low | Low | Configure 5-minute clock skew tolerance |

---

## Security Considerations

- JWT signing key must be at least 256 bits (32 bytes) for HMAC-SHA256
- Never return detailed error messages about why authentication failed (prevents user enumeration)
- Enable account lockout after 5 failed attempts (15-minute lockout)
- Require strong passwords (minimum 8 characters, mixed case, numbers, special characters)
- Validate email format server-side regardless of client validation
- Use HTTPS in production to protect tokens in transit
- Set appropriate CORS policy for the SPA origin
- Consider rate limiting on authentication endpoints

---

## Next Steps

After completing this phase, proceed to:
- [Phase 04: Photo Management API](./phase-04-photo-management-api.md) - Build photo upload, storage, and retrieval endpoints
- [Phase 05: Album & Tag Management](./phase-05-album-tag-management.md) - Can begin in parallel with Phase 04
