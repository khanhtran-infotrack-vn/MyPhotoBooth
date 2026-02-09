# Phase 01: Project Setup

## Context Links

- [Main Plan](./plan.md)
- [Tech Stack](../../docs/tech-stack.md)
- [ASP.NET Core API Report](../reports/260209-researcher-to-initializer-aspnet-core-api-report.md)
- [React SPA Architecture Report](../reports/260209-researcher-to-initializer-react-spa-architecture-report.md)

---

## Overview

| Field | Value |
|-------|-------|
| Date | 2026-02-09 |
| Priority | Critical - Foundation for all other phases |
| Status | pending |
| Progress | 0% |
| Estimated Effort | 2-3 hours |

---

## Key Insights

- Clean Architecture with four layers (Domain, Application, Infrastructure, API) ensures testable, maintainable code
- Vite provides 10x faster dev server startup (390ms vs 4.5s CRA) and instant HMR
- Solution structure must support both frontend and backend in a monorepo layout
- Git configuration with proper .gitignore files prevents committing build artifacts, secrets, and node_modules

---

## Requirements

1. Create the .NET solution with Clean Architecture project structure
2. Initialize the React frontend with Vite and TypeScript
3. Configure Git repository with appropriate .gitignore
4. Install foundational NuGet packages for all backend projects
5. Install foundational npm packages for the frontend
6. Establish development environment configuration files

---

## Architecture

### Solution Layout
```
MyPhotoBooth/
├── src/
│   ├── MyPhotoBooth.Domain/              # .NET Class Library
│   ├── MyPhotoBooth.Application/         # .NET Class Library
│   ├── MyPhotoBooth.Infrastructure/      # .NET Class Library
│   ├── MyPhotoBooth.API/                 # ASP.NET Core Web API
│   └── client/                           # React SPA (Vite)
│       ├── src/
│       │   ├── features/
│       │   ├── components/
│       │   ├── lib/
│       │   ├── stores/
│       │   ├── hooks/
│       │   └── utils/
│       ├── package.json
│       └── vite.config.ts
├── tests/
│   ├── MyPhotoBooth.Domain.Tests/
│   ├── MyPhotoBooth.Application.Tests/
│   └── MyPhotoBooth.API.Tests/
├── MyPhotoBooth.sln
├── .gitignore
└── CLAUDE.md
```

### Project References (Dependencies Flow)
```
API --> Application --> Domain
Infrastructure --> Application --> Domain
API --> Infrastructure
```

---

## Related Code Files

| File | Action | Purpose |
|------|--------|---------|
| `MyPhotoBooth.sln` | Create | Solution file |
| `src/MyPhotoBooth.Domain/MyPhotoBooth.Domain.csproj` | Create | Domain entities project |
| `src/MyPhotoBooth.Application/MyPhotoBooth.Application.csproj` | Create | Business logic project |
| `src/MyPhotoBooth.Infrastructure/MyPhotoBooth.Infrastructure.csproj` | Create | Data access project |
| `src/MyPhotoBooth.API/MyPhotoBooth.API.csproj` | Create | Web API project |
| `src/client/package.json` | Create | Frontend dependencies |
| `src/client/vite.config.ts` | Create | Vite configuration |
| `src/client/tsconfig.json` | Create | TypeScript configuration |
| `.gitignore` | Create | Git ignore rules |

---

## Implementation Steps

1. **Create .NET solution and projects**
   - Run `dotnet new sln -n MyPhotoBooth` in project root
   - Create Domain project: `dotnet new classlib -n MyPhotoBooth.Domain -o src/MyPhotoBooth.Domain`
   - Create Application project: `dotnet new classlib -n MyPhotoBooth.Application -o src/MyPhotoBooth.Application`
   - Create Infrastructure project: `dotnet new classlib -n MyPhotoBooth.Infrastructure -o src/MyPhotoBooth.Infrastructure`
   - Create API project: `dotnet new webapi -n MyPhotoBooth.API -o src/MyPhotoBooth.API`
   - Add all projects to solution

2. **Configure project references**
   - Application references Domain
   - Infrastructure references Application
   - API references Application and Infrastructure

3. **Install foundational NuGet packages**
   - Domain: No external dependencies (pure domain)
   - Application: `Microsoft.Extensions.DependencyInjection.Abstractions`
   - Infrastructure: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Tools`, `SixLabors.ImageSharp`
   - API: `Microsoft.EntityFrameworkCore.Design`

4. **Create initial folder structures in each project**
   - Domain: `Entities/`, `Enums/`, `Specifications/`
   - Application: `Common/`, `Interfaces/`, `Photos/`, `Albums/`, `Tags/`
   - Infrastructure: `Persistence/`, `Storage/`, `Identity/`
   - API: `Controllers/`, `Filters/`, `Middleware/`

5. **Initialize React frontend**
   - Run `npm create vite@latest client -- --template react-ts` in `src/` directory
   - Install core packages: `react-router-dom`, `@tanstack/react-query`, `zustand`, `axios`
   - Install dev packages: `eslint`, `prettier`, `@types/node`

6. **Create frontend folder structure**
   - `src/features/auth/`, `src/features/gallery/`, `src/features/albums/`, `src/features/upload/`, `src/features/lightbox/`
   - `src/components/`, `src/lib/`, `src/stores/`, `src/hooks/`, `src/utils/`

7. **Configure development environment**
   - Create `src/MyPhotoBooth.API/appsettings.Development.json` with placeholder connection strings
   - Create `src/client/.env.development` with API base URL
   - Configure Vite proxy for API calls during development

8. **Set up Git**
   - Initialize repository if not already done
   - Create comprehensive .gitignore covering .NET, Node.js, IDE files, and secrets
   - Create initial commit

---

## Todo List

- [ ] Create .NET solution file
- [ ] Create Domain class library project
- [ ] Create Application class library project
- [ ] Create Infrastructure class library project
- [ ] Create API web project
- [ ] Configure project references (dependency flow)
- [ ] Install NuGet packages for all projects
- [ ] Create folder structures in each .NET project
- [ ] Initialize Vite + React + TypeScript frontend
- [ ] Install frontend npm packages
- [ ] Create frontend folder structure
- [ ] Configure appsettings.Development.json
- [ ] Configure Vite proxy and .env files
- [ ] Create .gitignore
- [ ] Verify solution builds without errors
- [ ] Verify frontend dev server starts

---

## Success Criteria

- `dotnet build` succeeds for the entire solution with zero errors
- `npm run dev` starts the Vite dev server in the client directory
- All project references resolve correctly (API -> Application -> Domain, Infrastructure -> Application -> Domain)
- Folder structures exist for all architectural layers
- .gitignore properly excludes bin/, obj/, node_modules/, .env files
- No secrets are committed to the repository

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| .NET SDK version mismatch | Low | Medium | Specify target framework in all .csproj files |
| NuGet package version conflicts | Low | Medium | Use consistent package versions across projects |
| Node.js version incompatibility | Low | Low | Document minimum Node.js version (18+) |
| Vite proxy misconfiguration | Medium | Low | Test API proxy early in development |

---

## Security Considerations

- Never commit `appsettings.json` with real connection strings or secrets
- Ensure `.env` files are in .gitignore
- Use placeholder values in committed configuration files
- Store actual secrets in environment variables or a secret manager

---

## Next Steps

After completing this phase, proceed to:
- [Phase 02: Database & Infrastructure](./phase-02-database-infrastructure.md) - Set up PostgreSQL, EF Core DbContext, and initial migrations
- [Phase 06: Frontend Foundation](./phase-06-frontend-foundation.md) - Can begin in parallel once the client project is scaffolded
