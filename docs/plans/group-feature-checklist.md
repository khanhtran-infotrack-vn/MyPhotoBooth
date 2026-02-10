# Group Feature - Implementation Checklist

**Version**: 1.4.0
**Status**: Ready to Start
**Estimated Time**: 50 hours
**Last Updated**: 2026-02-10

---

## Quick Reference - Configuration Defaults

```json
{
  "GroupSettings": {
    "MaxMembersPerGroup": 50,
    "DeletionDays": 90,
    "MemberContentGraceDays": 7,
    "ReminderDays": [60, 30, 7, 1],
    "CleanupServiceInterval": "01:00:00"
  }
}
```

**Dev Override**: `DeletionDays: 1`, `MemberContentGraceDays: 0`

---

## Phase 1: Foundation (16h)

### Domain Entities (2h)
- [ ] `src/MyPhotoBooth.Domain/Entities/Group.cs`
- [ ] `src/MyPhotoBooth.Domain/Entities/GroupMember.cs`
- [ ] `src/MyPhotoBooth.Domain/Entities/GroupSharedContent.cs`
- [ ] `src/MyPhotoBooth.Domain/Entities/SharedContentType.cs` (enum)

### DbContext & Migration (3h)
- [ ] Update `AppDbContext.cs`:
  - [ ] Add DbSets: `Groups`, `GroupMembers`, `GroupSharedContent`
  - [ ] Add OnModelCreating configurations (indexes, FKs, constraints)
- [ ] Create migration: `AddGroups`
- [ ] Apply migration to development database
- [ ] Verify tables created correctly

### Repository (2h)
- [ ] `src/MyPhotoBooth.Application/Interfaces/IGroupRepository.cs`
- [ ] `src/MyPhotoBooth.Infrastructure/Persistence/Repositories/GroupRepository.cs`
- [ ] Register in `DependencyInjection.cs`

### DTOs (1h)
- [ ] `src/MyPhotoBooth.Application/Common/DTOs/GroupDTOs.cs`
  - [ ] `CreateGroupRequest`
  - [ ] `UpdateGroupRequest`
  - [ ] `GroupResponse`
  - [ ] `GroupDetailsResponse`
  - [ ] `GroupMemberResponse`
  - [ ] `GroupContentResponse`

### Error Constants (0.5h)
- [ ] Update `src/MyPhotoBooth.Application/Common/Errors.cs`
  - [ ] Add `Groups` error class (15+ constants)

### CQRS - Commands (3h)
- [ ] `CreateGroupCommand.cs` + Handler + Validator
- [ ] `UpdateGroupCommand.cs` + Handler + Validator
- [ ] `DeleteGroupCommand.cs` + Handler + Validator

### CQRS - Queries (2h)
- [ ] `GetGroupsQuery.cs` (owned + member-of) + Handler
- [ ] `GetGroupQuery.cs` (details) + Handler + Validator

### Configuration (1h)
- [ ] `src/MyPhotoBooth.Application/Common/Configuration/GroupSettings.cs`
- [ ] Update `appsettings.Development.json`
- [ ] Update `appsettings.json` (production defaults)
- [ ] Register IOptions<GroupSettings> in DI

### Controller (1.5h)
- [ ] `src/MyPhotoBooth.API/Controllers/GroupsController.cs`
  - [ ] POST /api/groups
  - [ ] GET /api/groups
  - [ ] GET /api/groups/{id}
  - [ ] PUT /api/groups/{id}
  - [ ] DELETE /api/groups/{id}

### Tests - Unit (2h)
- [ ] `CreateGroupCommandHandlerTests.cs`
- [ ] `CreateGroupCommandValidatorTests.cs`
- [ ] `GetGroupsQueryHandlerTests.cs`

### Tests - Integration (2h)
- [ ] `GroupsEndpointTests.cs` (CRUD operations)

---

## Phase 2: Membership Management (12h)

### Email Templates (3h)
- [ ] `GroupMemberAdded.metadata.json` + `.html`
- [ ] `GroupMemberRemoved.metadata.json` + `.html`
- [ ] `GroupMemberLeft.metadata.json` + `.html`
- [ ] `GroupOwnershipTransferred.metadata.json` + `.html`

### IEmailService Extensions (1h)
- [ ] Add methods to `IEmailService.cs`:
  - [ ] `SendGroupMemberAddedEmailAsync`
  - [ ] `SendGroupMemberRemovedEmailAsync`
  - [ ] `SendGroupMemberLeftEmailAsync`
  - [ ] `SendGroupOwnershipTransferredEmailAsync`
- [ ] Implement in `EmailService.cs`

### CQRS - Commands (4h)
- [ ] `AddGroupMemberCommand.cs` + Handler + Validator
  - [ ] Check: MaxMembersPerGroup limit (50)
  - [ ] Check: Email exists in AspNetUsers
  - [ ] Send: Member added email
- [ ] `RemoveGroupMemberCommand.cs` + Handler + Validator
  - [ ] Check: Cannot remove owner
  - [ ] Send: Member removed email
- [ ] `LeaveGroupCommand.cs` + Handler + Validator
  - [ ] Check: Owner must transfer or schedule deletion
  - [ ] Set: ContentRemovalDate (7 days)
  - [ ] Send: Member left email
- [ ] `TransferOwnershipCommand.cs` + Handler + Validator
  - [ ] Check: Cannot transfer to self
  - [ ] Check: New member must exist
  - [ ] Cancel deletion if scheduled
  - [ ] Send: Ownership transferred email

### CQRS - Queries (1h)
- [ ] `GetGroupMembersQuery.cs` + Handler

### Controller Updates (1h)
- [ ] POST /api/groups/{id}/members
- [ ] GET /api/groups/{id}/members
- [ ] DELETE /api/groups/{id}/members/{uid}
- [ ] POST /api/groups/{id}/leave
- [ ] POST /api/groups/{id}/transfer-ownership

### Tests (2h)
- [ ] Unit tests for all membership handlers
- [ ] Integration tests for membership endpoints
- [ ] Test: Max members limit enforcement
- [ ] Test: Owner cannot be removed
- [ ] Test: Email notifications sent

---

## Phase 3: Content Sharing (10h)

### CQRS - Commands (4h)
- [ ] `SharePhotoToGroupCommand.cs` + Handler + Validator
  - [ ] Check: User is group member
  - [ ] Check: Photo exists and belongs to user
- [ ] `ShareAlbumToGroupCommand.cs` + Handler + Validator
  - [ ] Check: User is group member
  - [ ] Check: Album exists and belongs to user
- [ ] `RemovePhotoFromGroupCommand.cs` + Handler + Validator
  - [ ] Check: User is owner OR shared-by user
- [ ] `RemoveAlbumFromGroupCommand.cs` + Handler + Validator
  - [ ] Check: User is owner OR shared-by user

### CQRS - Queries (2h)
- [ ] `GetGroupContentQuery.cs` (photos + albums) + Handler
- [ ] `GetGroupPhotosQuery.cs` + Handler
- [ ] `GetGroupAlbumsQuery.cs` + Handler

### Controller Updates (1h)
- [ ] POST /api/groups/{id}/photos
- [ ] DELETE /api/groups/{id}/photos/{pid}
- [ ] POST /api/groups/{id}/albums
- [ ] DELETE /api/groups/{id}/albums/{aid}
- [ ] GET /api/groups/{id}/content
- [ ] GET /api/groups/{id}/photos
- [ ] GET /api/groups/{id}/albums

### DTOs Updates (0.5h)
- [ ] Add `ShareContentRequest`
- [ ] Update `GroupContentResponse` with photo/album details

### Tests (2.5h)
- [ ] Unit tests for content sharing handlers
- [ ] Integration tests for content endpoints
- [ ] Test: Permission matrix (owner vs member vs non-member)
- [ ] Test: Members can only remove own content

---

## Phase 4: Lifecycle Management (12h)

### Email Templates (3h)
- [ ] `GroupDeletionScheduled.metadata.json` + `.html`
- [ ] `GroupDeletionReminder.metadata.json` + `.html`

### IEmailService Extensions (0.5h)
- [ ] Add `SendGroupDeletionScheduledEmailAsync`
- [ ] Add `SendGroupDeletionReminderEmailAsync`
- [ ] Implement in `EmailService.cs`

### Background Service (4h)
- [ ] `src/MyPhotoBooth.Infrastructure/BackgroundServices/GroupCleanupBackgroundService.cs`
  - [ ] Runs daily (configurable interval)
  - [ ] Process: Group deletions (scheduled date passed)
  - [ ] Process: Member content removal (grace period passed)
  - [ ] Process: Send reminder emails (60, 30, 7, 1 days)
  - [ ] Handle: Timezone issues (use UTC)
- [ ] Register in `Program.cs`: `AddHostedService<GroupCleanupBackgroundService>()`

### LeaveGroupCommand Update (1.5h)
- [ ] Modify handler to schedule deletion when owner leaves:
  - [ ] Check: If owner leaving without transfer
  - [ ] Set: `DeletionScheduledAt`, `DeletionProcessDate`
  - [ ] Send: Deletion scheduled email to ALL members
- [ ] Test: Owner leaves with members → deletion scheduled
- [ ] Test: Owner leaves alone → group deleted immediately

### TransferOwnershipCommand Update (0.5h)
- [ ] Cancel deletion if scheduled
- [ ] Send: Ownership transferred email
- [ ] Test: Transferring cancels deletion

### Tests (2.5h)
- [ ] Unit tests for background service logic
- [ ] Integration tests with time manipulation
- [ ] Test: 90-day deletion scheduling
- [ ] Test: 7-day grace period content removal
- [ ] Test: Reminder emails at 60, 30, 7, 1 days
- [ ] Test: Owner rejoining cancels deletion

---

## Phase 5: Polish & Testing (10h)

### Additional Features (2h)
- [ ] Pagination for `GetGroupContentQuery`
- [ ] Sorting options (date, name)
- [ ] Filtering by content type

### Performance (2h)
- [ ] Database index review
- [ ] Query optimization for large groups
- [ ] Test with 50 members, 1000+ shared items

### Security (1h)
- [ ] SQL injection audit
- [ ] Authorization check review
- [ ] OWASP Top 10 validation

### Documentation (3h)
- [ ] Update `CLAUDE.md` with group feature
- [ ] Update `README.md` with new endpoints
- [ ] Create API documentation comments
- [ ] Update Scalar/OpenAPI specs

### Test Coverage (2h)
- [ ] Verify 70% handler coverage
- [ ] Verify 100% validator coverage
- [ ] Verify 100% API endpoint coverage
- [ ] Fix any gaps

---

## Pre-Implementation Checklist

### Environment Setup
- [ ] PostgreSQL running (localhost:5432)
- [ ] Mailpit running (localhost:8025) for email testing
- [ ] Dev database backed up

### Branch Setup
- [ ] Create branch: `feature/groups`
- [ ] Pull latest from `main`

### Dependencies
- [ ] Verify EF Core tools installed
- [ ] Verify all NuGet packages up to date

---

## File Creation Summary

### New Files (60+)
| Category | Count |
|----------|-------|
| Domain Entities | 4 |
| Repository | 2 |
| DTOs | 1 file (6+ classes) |
| Commands | 11 |
| Queries | 6 |
| Handlers | 17 |
| Validators | 7 |
| Controllers | 1 |
| Email Templates | 9 |
| Background Services | 1 |
| Unit Tests | 15+ |
| Integration Tests | 3+ |

### Modified Files (8)
| File | Changes |
|------|---------|
| `AppDbContext.cs` | Add DbSets, entity configs |
| `Errors.cs` | Add Groups error constants |
| `IEmailService.cs` | Add group email methods |
| `EmailService.cs` | Implement group emails |
| `DependencyInjection.cs` | Register repo, settings, background service |
| `Program.cs` | Add background service |
| `appsettings.json` | Add GroupSettings |
| `appsettings.Development.json` | Add GroupSettings (dev overrides) |

---

## Testing Command Reference

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter "FullyQualifiedName~UnitTests"

# Run integration tests only
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Verify build
dotnet build --no-restore
```

---

## Git Commit Strategy

```bash
# Phase 1 - Foundation
git commit -m "feat(groups): add domain entities and database schema

- Add Group, GroupMember, GroupSharedContent entities
- Create migration: AddGroups
- Implement IGroupRepository
- Add basic CRUD commands/queries
- Create GroupsController with CRUD endpoints
- Add GroupSettings configuration

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"

# Phase 2 - Membership
git commit -m "feat(groups): add membership management

- Add/remove members via email
- Leave group functionality
- Transfer ownership
- Email notifications for membership changes
- Member list endpoint

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"

# Phase 3 - Content Sharing
git commit -m "feat(groups): add content sharing

- Share photos/albums to groups
- Remove shared content
- Permission checks (owner vs member)
- Group content queries

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"

# Phase 4 - Lifecycle
git commit -m "feat(groups): add lifecycle management

- Group deletion scheduling (90 days)
- Member content grace period (7 days)
- Deletion reminder emails (60, 30, 7, 1 days)
- Background cleanup service
- Owner leave handling

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"

# Phase 5 - Polish
git commit -m "feat(groups): add polish and testing

- Pagination and filtering
- Performance optimizations
- Security audit
- Complete test coverage
- Documentation updates

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Verification Steps

After each phase, verify:

```bash
# 1. Build succeeds
dotnet build

# 2. Tests pass
dotnet test --no-build

# 3. API is accessible
curl http://localhost:5149/scalar/v1

# 4. Emails visible in Mailpit
open http://localhost:8025

# 5. Database schema correct
psql -h localhost -U postgres -d myphotobooth_dev -c "\dt groups*"
```

---

## Definition of Done

A phase is complete when:
- [ ] All tasks checked off
- [ ] Build succeeds with no new warnings
- [ ] Unit tests pass (70%+ coverage)
- [ ] Integration tests pass (100% endpoint coverage)
- [ ] API accessible via Scalar
- [ ] Emails visible in Mailpit (if applicable)
- [ ] Code reviewed (self or peer)
- [ ] Git commit created with descriptive message

---

## Progress Tracking

| Phase | Status | Completed | Date |
|-------|--------|-----------|------|
| 1. Foundation | ⬜ Not Started | 0/26 | |
| 2. Membership | ⬜ Not Started | 0/14 | |
| 3. Content Sharing | ⬜ Not Started | 0/10 | |
| 4. Lifecycle | ⬜ Not Started | 0/9 | |
| 5. Polish | ⬜ Not Started | 0/13 | |

**Overall**: 0/72 tasks completed (0%)

---

**Ready to start Phase 1?** Run: `git checkout -b feature/groups`
