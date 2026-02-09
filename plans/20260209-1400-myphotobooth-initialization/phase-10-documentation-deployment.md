# Phase 10: Documentation & Deployment

## Context Links

- [Main Plan](./plan.md)
- [Tech Stack - Deployment Considerations](../../docs/tech-stack.md)

---

## Overview

| Field | Value |
|-------|-------|
| Date | 2026-02-09 |
| Priority | Medium - Final step before launch |
| Status | pending |
| Progress | 0% |
| Estimated Effort | 4-5 hours |
| Depends On | Phase 09 (Integration & Testing) |

---

## Key Insights

- Docker containerization ensures consistent environments across development, staging, and production
- Frontend static files can be served from a CDN or Nginx for optimal performance
- PostgreSQL backups must be automated from day one
- Environment-specific configuration (dev/staging/prod) prevents secret leakage
- Health check endpoints enable monitoring and automated restarts
- HTTPS is mandatory in production for token security

---

## Requirements

1. Create comprehensive README with setup instructions
2. Document API endpoints (consider auto-generated docs with Swagger)
3. Create Docker configuration for backend and database
4. Configure production environment settings
5. Set up database backup strategy
6. Create deployment scripts or CI/CD pipeline
7. Document environment variables and secrets management
8. Create development setup guide for new contributors

---

## Architecture

### Deployment Architecture
```
Production:
  Nginx (reverse proxy + static files)
    ├── / -> React SPA static files (dist/)
    └── /api -> ASP.NET Core API (Kestrel on port 5000)

  PostgreSQL (managed or self-hosted)
    └── Automated daily backups

  File Storage
    └── /var/data/myphotobooth/photos/ (persistent volume)
```

### Docker Compose Stack
```
services:
  api:          # ASP.NET Core Web API
  db:           # PostgreSQL
  nginx:        # Reverse proxy + static files
  # Optional:
  pgadmin:      # Database management UI (dev only)
```

### Environment Configuration
```
Development:    appsettings.Development.json + .env.development
Staging:        appsettings.Staging.json + environment variables
Production:     appsettings.Production.json + environment variables / secrets manager
```

---

## Related Code Files

| File | Action | Purpose |
|------|--------|---------|
| `Dockerfile` | Create | API container image |
| `docker-compose.yml` | Create | Full stack orchestration |
| `docker-compose.dev.yml` | Create | Development overrides |
| `nginx/nginx.conf` | Create | Reverse proxy config |
| `nginx/Dockerfile` | Create | Nginx with SPA files |
| `.dockerignore` | Create | Docker build exclusions |
| `src/MyPhotoBooth.API/appsettings.Production.json` | Create | Production config |
| `src/MyPhotoBooth.API/HealthChecks/` | Create | Health check endpoints |
| `scripts/backup-db.sh` | Create | Database backup script |
| `scripts/deploy.sh` | Create | Deployment automation |
| `.github/workflows/ci.yml` | Create | CI/CD pipeline (if GitHub) |
| `CLAUDE.md` | Modify | Update with build commands |

---

## Implementation Steps

1. **Configure Swagger/OpenAPI documentation**
   - Add `Swashbuckle.AspNetCore` NuGet package
   - Configure Swagger in Program.cs with API info, version
   - Add XML comments to controllers for auto-generated docs
   - Configure JWT bearer authentication in Swagger UI
   - Enable Swagger only in Development environment (or behind auth in production)

2. **Add health check endpoints**
   - `/health` -> basic health status (API is running)
   - `/health/ready` -> readiness check (database connected, storage accessible)
   - Use `Microsoft.Extensions.Diagnostics.HealthChecks`
   - Add PostgreSQL health check via `AspNetCore.HealthChecks.NpgsqlMigrations`
   - Add file system storage health check (write test file)

3. **Create Dockerfile for API**
   - Multi-stage build: SDK image for build, runtime image for deploy
   - Copy only necessary files (not tests or frontend)
   - Expose port 5000 (HTTP) and 5001 (HTTPS)
   - Set ASPNETCORE_ENVIRONMENT to Production
   - Configure health check in Dockerfile

4. **Create Dockerfile for frontend (Nginx)**
   - Build stage: Node image, `npm ci`, `npm run build`
   - Serve stage: Nginx alpine image with SPA routing config
   - Copy dist/ to Nginx html directory
   - Configure Nginx for SPA fallback (try_files $uri /index.html)

5. **Create docker-compose.yml**
   - API service: build from Dockerfile, depends_on db, environment variables
   - Database service: PostgreSQL image, persistent volume, health check
   - Nginx service: build from nginx/Dockerfile, ports 80/443, depends_on api
   - Shared network for inter-service communication
   - Named volumes for database data and photo storage

6. **Create docker-compose.dev.yml**
   - Override for development: mount source code as volumes
   - Enable hot reload for API (dotnet watch)
   - Expose PostgreSQL port for direct access
   - Add pgAdmin service for database management
   - Use development environment variables

7. **Configure production settings**
   - `appsettings.Production.json`: logging levels, CORS origins, request limits
   - Environment variables for secrets: DB connection string, JWT key, storage path
   - Enable HTTPS redirection
   - Configure rate limiting middleware
   - Enable response compression (gzip/brotli)
   - Set appropriate CORS policy for production domain

8. **Create database backup script**
   - `pg_dump` with compression to backup directory
   - Timestamped filenames: `myphotobooth_20260209_140000.sql.gz`
   - Retention policy: keep last 30 daily backups
   - Can be run via cron job or scheduled task
   - Test backup restoration procedure

9. **Create CI/CD pipeline**
   - On push to main: run backend tests, run frontend tests, build Docker images
   - On PR: run linting, run tests, report coverage
   - Optional: deploy to staging on merge to main
   - Cache NuGet packages and node_modules between runs
   - Run E2E tests against staging environment

10. **Update project documentation**
    - Update CLAUDE.md with build commands, architecture overview, test commands
    - Document all environment variables with descriptions and example values
    - Document development setup (prerequisites, first-time setup, daily workflow)
    - Document deployment process (Docker, manual, cloud provider)
    - Document backup and restoration procedures
    - Document troubleshooting common issues

---

## Todo List

- [ ] Add Swagger/OpenAPI to the API project
- [ ] Add XML comments to all controller actions
- [ ] Configure JWT authentication in Swagger UI
- [ ] Implement health check endpoints (/health, /health/ready)
- [ ] Create API Dockerfile (multi-stage build)
- [ ] Create frontend Nginx Dockerfile
- [ ] Create nginx.conf with SPA routing and reverse proxy
- [ ] Create docker-compose.yml for production
- [ ] Create docker-compose.dev.yml for development
- [ ] Create .dockerignore file
- [ ] Configure appsettings.Production.json
- [ ] Enable HTTPS, CORS, rate limiting, compression for production
- [ ] Create database backup script
- [ ] Create deployment script
- [ ] Create CI/CD pipeline configuration
- [ ] Update CLAUDE.md with build and test commands
- [ ] Document environment variables
- [ ] Document development setup instructions
- [ ] Document deployment procedures
- [ ] Document backup and restore procedures
- [ ] Test Docker build and run locally
- [ ] Test docker-compose up brings entire stack online
- [ ] Verify health checks respond correctly
- [ ] Verify Swagger UI is accessible in development
- [ ] Verify production builds work correctly

---

## Success Criteria

- `docker-compose up` starts the entire application stack (API + DB + Nginx)
- Health check endpoints respond with correct status
- Swagger UI is accessible and documents all endpoints with authentication
- API Dockerfile builds successfully and produces a working container
- Frontend Dockerfile builds and serves the SPA with proper routing
- Backup script creates compressed database dumps
- CLAUDE.md contains accurate build, test, and run instructions
- All environment variables are documented with descriptions
- A new developer can set up the project using only the documentation
- Production configuration enforces HTTPS, CORS, rate limiting

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Docker build too slow | Medium | Low | Use multi-stage builds, cache layers |
| Database backup fails silently | Low | Critical | Add monitoring, test backups regularly |
| Environment variable misconfiguration | Medium | High | Document all variables, validate at startup |
| CORS blocking API in production | Medium | High | Test with actual production domain early |
| SSL certificate issues | Medium | Medium | Use Let's Encrypt with auto-renewal |
| Storage volume permissions in Docker | Medium | Medium | Set correct UID/GID in Dockerfile |

---

## Security Considerations

- HTTPS is mandatory in production; redirect all HTTP to HTTPS
- SSL/TLS certificate must be from a trusted CA (Let's Encrypt or similar)
- Database credentials must not be in docker-compose.yml; use .env file or secrets
- Swagger UI should be disabled or password-protected in production
- Docker images should not contain secrets; inject via environment variables
- Container images should use non-root users
- Enable security headers: X-Content-Type-Options, X-Frame-Options, Strict-Transport-Security
- Rate limit all public endpoints (login, register) to prevent abuse
- Log security events (failed logins, unauthorized access attempts)
- Regularly update base Docker images for security patches

---

## Next Steps

After completing this phase, the MyPhotoBooth application is ready for initial deployment and user testing. Future enhancements may include:

- Cloud storage migration (AWS S3 or Azure Blob Storage)
- Image CDN integration (Cloudflare, Imgix)
- Progressive Web App (PWA) support
- Social sharing features
- AI-powered auto-tagging
- Facial recognition for people grouping
- Advanced search with full-text search
- Album sharing with external users
- Mobile app (React Native)
