# Group Feature Implementation Plan

## Executive Summary

This document outlines the comprehensive implementation plan for adding a **Group Collaboration Feature** to MyPhotoBooth. Groups allow users to create collaborative spaces where members can share photos and albums with permission-based access control and automated lifecycle management.

**Status**: Planning Phase
**Target Release**: v1.4.0
**Estimated Effort**: 40-60 person-hours
**Testing Strategy**: TFD with Testcontainers (PostgreSQL)

---

## Table of Contents

1. [Feature Requirements](#feature-requirements)
2. [Architectural Decisions](#architectural-decisions)
3. [Domain Model](#domain-model)
4. [Database Schema](#database-schema)
5. [API Design](#api-design)
6. [Implementation Phases](#implementation-phases)
7. [File Creation Matrix](#file-creation-matrix)
8. [Testing Strategy](#testing-strategy)
9. [Migration Plan](#migration-plan)
10. [Risk Assessment](#risk-assessment)
11. [Open Questions](#open-questions)

---

## Feature Requirements

### Core Functionality

1. **Group Management**
   - Create groups (user becomes owner)
   - Update group name/description (owner only)
   - Delete groups (owner only, with member notification)
   - List groups (owned + member-of)

2. **Membership Management**
   - Add members via email invitation (owner only)
   - Remove members (owner only)
   - Leave group (members)
   - View group members

3. **Content Sharing**
   - Share photos to groups
   - Share albums to groups
   - View group-shared content
   - Remove own shared content from groups

4. **Permission Model**
   - Owner: Full control (manage group, members, content)
   - Members: View shared content + share own content + remove own content

### Lifecycle Management (Critical)

| Event | Trigger | Action | Timeline |
|-------|---------|--------|----------|
| Owner leaves without transfer | Owner exits group, no new owner | Schedule group deletion | 90 days (configurable) |
| Member leaves | Member exits group | Grace period for their content | 7 days (auto-remove after) |
| Group deletion scheduled | Owner leaves without transfer | Notify ALL members | Immediately + reminders |
| Deletion reminders | Scheduled deletion | Reminder emails | 60, 30, 7, 1 days before |

### Email Notifications

- Member invitation email
- Member added notification
- Member removed notification
- Member left notification
- Group deletion scheduled notification (to all members)
- Deletion reminder emails (60, 30, 7, 1 days)
- Ownership transfer notification

---

## Architectural Decisions

### Decision 1: Repository Pattern vs Direct DbContext

**CHOSEN**: Repository Pattern (following existing Albums/Photos/Tags pattern)

**Rationale**:
- Existing codebase uses repositories for Albums, Photos, Tags
- Consistency with established patterns
- Easier testing with mocks
- Encapsulates complex queries (especially for group membership checks)

**Files to Create**:
- `Application/Interfaces/IGroupRepository.cs`
- `Infrastructure/Persistence/Repositories/GroupRepository.cs`

### Decision 2: ICurrentUserService vs Controller-passed UserId

**CHOSEN**: Continue controller-passed UserId pattern

**Rationale**:
- Existing pattern throughout codebase
- Explicit parameter passing improves testability
- No hidden dependencies
- Consistent with current commands/queries

**Pattern**: All Commands/Queries include `string UserId` parameter

### Decision 3: Notification System Design

**CHOSEN**: Generic notification system (reusable for future features)

**Rationale**:
- Groups need notifications now
- Future features (comments, likes) will need notifications
- Single source of truth for notification preferences
- Easier to extend and maintain

**Components**:
- `NotificationService` background worker
- `INotificationService` interface
- `Notification` entity for audit trail
- Email templates for all notification types

### Decision 4: Background Job Infrastructure

**CHOSEN**: IHostedService for v1, RabbitMQ as future consideration

**Rationale**:
- v1 scope: Simple scheduled tasks (cron-like)
- IHostedService sufficient for single-instance deployment
- RabbitMQ adds operational complexity
- Can migrate later if horizontal scaling needed

**Implementation**:
- `GroupCleanupBackgroundService` - Runs daily to check deletion schedules
- Database query for groups pending deletion/reminders
- Sends emails and updates group status

### Decision 5: Frontend Scope

**CHOSEN**: Backend-only plan (frontend separate plan)

**Rationale**:
- Backend complexity warrants focused planning
- Frontend has different concerns (UI state, routing, components)
- Can be implemented in parallel after backend API is stable

**Future Frontend Work** (separate plan):
- Groups list page
- Group detail page
- Member management UI
- Share-to-group flow in photo/album views

---

## Domain Model

### Entities

```csharp
// Group.cs
public class Group
{
    public Guid Id { get; set; }
    public string Name { get; set; }           // Required, max 100 chars
    public string? Description { get; set; }    // Optional, max 500 chars
    public string OwnerId { get; set; }         // FK to ApplicationUser.Id
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }    // Soft delete
    public DateTime? DeletionScheduledAt { get; set; }  // When owner left without transfer
    public DateTime? DeletionProcessDate { get; set; }   // When deletion will occur

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
    public bool IsDeletionScheduled => DeletionScheduledAt.HasValue && !DeletedAt.HasValue;
    public int DaysUntilDeletion => DeletionProcessDate.HasValue
        ? (DeletionProcessDate.Value - DateTime.UtcNow).Days
        : 0;

    // Navigation
    public ApplicationUser Owner { get; set; }
    public ICollection<GroupMember> Members { get; set; }
    public ICollection<GroupSharedContent> SharedContent { get; set; }
}

// GroupMember.cs
public class GroupMember
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string UserId { get; set; }         // FK to ApplicationUser.Id
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }      // Soft leave
    public DateTime? ContentRemovalDate { get; set; }  // 7 days after leaving

    // Computed
    public bool IsActive => !LeftAt.HasValue;
    public bool IsInGracePeriod => LeftAt.HasValue &&
        ContentRemovalDate.HasValue &&
        ContentRemovalDate.Value > DateTime.UtcNow;

    // Navigation
    public Group Group { get; set; }
    public ApplicationUser User { get; set; }
}

// GroupSharedContent.cs
public class GroupSharedContent
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string SharedByUserId { get; set; }  // Who shared it
    public SharedContentType ContentType { get; set; }
    public Guid? PhotoId { get; set; }
    public Guid? AlbumId { get; set; }
    public DateTime SharedAt { get; set; }
    public DateTime? RemovedAt { get; set; }    // Soft remove

    // Computed
    public bool IsActive => !RemovedAt.HasValue;

    // Navigation
    public Group Group { get; set; }
    public Photo? Photo { get; set; }
    public Album? Album { get; set; }
}

// SharedContentType.cs
public enum SharedContentType
{
    Photo = 0,
    Album = 1
}
```

### Value Objects & DTOs

```csharp
// DTOs for API responses
public class GroupResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string OwnerId { get; set; }
    public string? OwnerEmail { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeletionScheduled { get; set; }
    public int? DaysUntilDeletion { get; set; }
}

public class GroupMemberResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string? UserName { get; set; }
    public bool IsOwner { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}

public class GroupContentResponse
{
    public Guid Id { get; set; }
    public SharedContentType ContentType { get; set; }
    public Guid? PhotoId { get; set; }
    public Guid? AlbumId { get; set; }
    public string SharedByUserEmail { get; set; }
    public DateTime SharedAt { get; set; }
    // Include Photo/Album details for display
}
```

---

## Database Schema

### Migration: `AddGroups`

```sql
-- Groups table
CREATE TABLE groups (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    owner_id VARCHAR(450) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    deleted_at TIMESTAMP NULL,
    deletion_scheduled_at TIMESTAMP NULL,
    deletion_process_date TIMESTAMP NULL,
    CONSTRAINT fk_groups_owner FOREIGN KEY (owner_id)
        REFERENCES aspnetusers(id) ON DELETE CASCADE
);

CREATE INDEX idx_groups_owner ON groups(owner_id);
CREATE INDEX idx_groups_deletion ON groups(deletion_process_date)
    WHERE deleted_at IS NULL AND deletion_process_date IS NOT NULL;

-- Group members
CREATE TABLE group_members (
    id UUID PRIMARY KEY,
    group_id UUID NOT NULL,
    user_id VARCHAR(450) NOT NULL,
    joined_at TIMESTAMP NOT NULL,
    left_at TIMESTAMP NULL,
    content_removal_date TIMESTAMP NULL,
    CONSTRAINT fk_members_group FOREIGN KEY (group_id)
        REFERENCES groups(id) ON DELETE CASCADE,
    CONSTRAINT fk_members_user FOREIGN KEY (user_id)
        REFERENCES aspnetusers(id) ON DELETE CASCADE,
    CONSTRAINT uq_group_user UNIQUE(group_id, user_id)
);

CREATE INDEX idx_members_group ON group_members(group_id);
CREATE INDEX idx_members_user ON group_members(user_id);
CREATE INDEX idx_members_removal ON group_members(content_removal_date)
    WHERE left_at IS NOT NULL AND content_removal_date IS NOT NULL;

-- Group shared content
CREATE TABLE group_shared_content (
    id UUID PRIMARY KEY,
    group_id UUID NOT NULL,
    shared_by_user_id VARCHAR(450) NOT NULL,
    content_type INT NOT NULL,
    photo_id UUID NULL,
    album_id UUID NULL,
    shared_at TIMESTAMP NOT NULL,
    removed_at TIMESTAMP NULL,
    CONSTRAINT fk_content_group FOREIGN KEY (group_id)
        REFERENCES groups(id) ON DELETE CASCADE,
    CONSTRAINT fk_content_photo FOREIGN KEY (photo_id)
        REFERENCES photos(id) ON DELETE CASCADE,
    CONSTRAINT fk_content_album FOREIGN KEY (album_id)
        REFERENCES albums(id) ON DELETE CASCADE,
    CONSTRAINT chk_content_type CHECK (
        (content_type = 0 AND photo_id IS NOT NULL AND album_id IS NULL) OR
        (content_type = 1 AND album_id IS NOT NULL AND photo_id IS NULL)
    )
);

CREATE INDEX idx_content_group ON group_shared_content(group_id);
CREATE INDEX idx_content_photo ON group_shared_content(photo_id);
CREATE INDEX idx_content_album ON group_shared_content(album_id);
```

---

## API Design

### Endpoints

```
POST   /api/groups                    Create group
GET    /api/groups                    List groups (owned + member-of)
GET    /api/groups/{id}               Get group details
PUT    /api/groups/{id}               Update group (owner)
DELETE /api/groups/{id}               Delete group (owner)

POST   /api/groups/{id}/members       Add member (owner)
GET    /api/groups/{id}/members       List members
DELETE /api/groups/{id}/members/{uid} Remove member (owner)
POST   /api/groups/{id}/leave         Leave group (member)
POST   /api/groups/{id}/transfer-ownership Transfer ownership (owner)

POST   /api/groups/{id}/photos        Share photo to group
DELETE /api/groups/{id}/photos/{pid}  Remove photo from group (owner or shared-by)
POST   /api/groups/{id}/albums        Share album to group
DELETE /api/groups/{id}/albums/{aid}  Remove album from group (owner or shared-by)

GET    /api/groups/{id}/content       Get group content (photos + albums)
GET    /api/groups/{id}/photos        Get group photos
GET    /api/groups/{id}/albums        Get group albums
```

### Request/Response Examples

```csharp
// CreateGroupCommand
public record CreateGroupCommand(
    string Name,
    string? Description,
    string UserId
) : ICommand<GroupResponse>;

// AddGroupMemberCommand
public record AddGroupMemberCommand(
    Guid GroupId,
    string MemberEmail,
    string UserId  // Owner making request
) : ICommand<GroupMemberResponse>;

// SharePhotoToGroupCommand
public record SharePhotoToGroupCommand(
    Guid GroupId,
    Guid PhotoId,
    string UserId  // User sharing
) : ICommand;

// LeaveGroupCommand
public record LeaveGroupCommand(
    Guid GroupId,
    string UserId
) : ICommand;
```

---

## Implementation Phases

### Phase 1: Foundation (Priority: HIGH)

**Goal**: Basic group CRUD + membership

**Tasks**:
1. Create domain entities (Group, GroupMember, GroupSharedContent)
2. Add EF Core configuration
3. Create migration
4. Implement IGroupRepository
5. Create CQRS commands/queries:
   - CreateGroupCommand
   - GetGroupsQuery (owned + member-of)
   - GetGroupQuery
   - UpdateGroupCommand
   - DeleteGroupCommand
6. Implement validators
7. Create GroupsController
8. Write unit tests (validators, handlers)
9. Write integration tests (API endpoints)

**Deliverables**:
- Groups can be created, viewed, updated, deleted
- Database schema complete
- API endpoints functional

**Estimated Time**: 16 hours

### Phase 2: Membership Management (Priority: HIGH)

**Goal**: Add/remove members, leave groups

**Tasks**:
1. Create CQRS commands/queries:
   - AddGroupMemberCommand
   - RemoveGroupMemberCommand
   - LeaveGroupCommand
   - GetGroupMembersQuery
   - TransferOwnershipCommand
2. Implement validators (email validation, ownership checks)
3. Create MembershipService for business logic
4. Add email templates:
   - Member invitation
   - Member added
   - Member removed
   - Member left
   - Ownership transferred
5. Extend IEmailService with group notification methods
6. Write unit tests
7. Write integration tests

**Deliverables**:
- Members can be added via email
- Members can be removed
- Members can leave groups
- Ownership can be transferred
- Email notifications sent

**Estimated Time**: 12 hours

### Phase 3: Content Sharing (Priority: HIGH)

**Goal**: Share photos/albums to groups

**Tasks**:
1. Create CQRS commands/queries:
   - SharePhotoToGroupCommand
   - ShareAlbumToGroupCommand
   - RemovePhotoFromGroupCommand
   - RemoveAlbumFromGroupCommand
   - GetGroupContentQuery
   - GetGroupPhotosQuery
   - GetGroupAlbumsQuery
2. Implement permission checks (owner can remove any, members only own)
3. Update Photo/Album responses to include group info
4. Write unit tests
5. Write integration tests

**Deliverables**:
- Photos can be shared to groups
- Albums can be shared to groups
- Content can be removed by owner or sharer
- Group content is queryable

**Estimated Time**: 10 hours

### Phase 4: Lifecycle Management (Priority: MEDIUM)

**Goal**: Deletion scheduling, grace periods, reminders

**Tasks**:
1. Create GroupCleanupBackgroundService (IHostedService)
2. Implement deletion scheduling logic:
   - Owner leaves without transfer → schedule deletion
   - Member leaves → set content_removal_date
3. Create reminder email templates (60, 30, 7, 1 days)
4. Implement daily cleanup job:
   - Check for pending deletions
   - Send reminder emails
   - Execute deletions
   - Remove expired member content
5. Add DeleteGroupCommand with notification logic
6. Add configuration:
   - GroupDeletionDays (default: 90)
   - MemberContentGraceDays (default: 7)
   - ReminderDays (default: 60, 30, 7, 1)
7. Write unit tests for background service logic
8. Write integration tests with time manipulation

**Deliverables**:
- Groups are scheduled for deletion when owner leaves
- Member content is removed after grace period
- Reminder emails are sent at configured intervals
- Groups are permanently deleted after scheduled date

**Estimated Time**: 12 hours

### Phase 5: Polish & Testing (Priority: LOW)

**Goal**: Full test coverage, documentation, edge cases

**Tasks**:
1. Add comprehensive error messages to Errors.cs
2. Add pagination to GetGroupContentQuery
3. Add filtering/sorting options
4. Performance testing (large groups, lots of content)
5. Security audit (SQL injection, authorization)
6. Update API documentation (Scalar)
7. Update CLAUDE.md with group feature
8. Update README.md
9. Create developer documentation

**Deliverables**:
- Production-ready feature
- Complete documentation
- 70%+ test coverage

**Estimated Time**: 10 hours

---

## File Creation Matrix

### Domain Layer
```
src/MyPhotoBooth.Domain/Entities/
├── Group.cs                                    [NEW]
├── GroupMember.cs                              [NEW]
├── GroupSharedContent.cs                       [NEW]
└── SharedContentType.cs                        [NEW] (enum)
```

### Application Layer - Interfaces
```
src/MyPhotoBooth.Application/Interfaces/
└── IGroupRepository.cs                         [NEW]
```

### Application Layer - Features/Groups
```
src/MyPhotoBooth.Application/Features/Groups/
├── Commands/
│   ├── CreateGroupCommand.cs                   [NEW]
│   ├── UpdateGroupCommand.cs                   [NEW]
│   ├── DeleteGroupCommand.cs                   [NEW]
│   ├── AddGroupMemberCommand.cs                [NEW]
│   ├── RemoveGroupMemberCommand.cs             [NEW]
│   ├── LeaveGroupCommand.cs                    [NEW]
│   ├── TransferOwnershipCommand.cs             [NEW]
│   ├── SharePhotoToGroupCommand.cs             [NEW]
│   ├── ShareAlbumToGroupCommand.cs             [NEW]
│   ├── RemovePhotoFromGroupCommand.cs          [NEW]
│   └── RemoveAlbumFromGroupCommand.cs          [NEW]
├── Queries/
│   ├── GetGroupsQuery.cs                       [NEW]
│   ├── GetGroupQuery.cs                        [NEW]
│   ├── GetGroupMembersQuery.cs                 [NEW]
│   ├── GetGroupContentQuery.cs                 [NEW]
│   ├── GetGroupPhotosQuery.cs                  [NEW]
│   └── GetGroupAlbumsQuery.cs                  [NEW]
├── Handlers/
│   ├── CreateGroupCommandHandler.cs            [NEW]
│   ├── UpdateGroupCommandHandler.cs            [NEW]
│   ├── DeleteGroupCommandHandler.cs            [NEW]
│   ├── AddGroupMemberCommandHandler.cs         [NEW]
│   ├── RemoveGroupMemberCommandHandler.cs      [NEW]
│   ├── LeaveGroupCommandHandler.cs             [NEW]
│   ├── TransferOwnershipCommandHandler.cs      [NEW]
│   ├── SharePhotoToGroupCommandHandler.cs      [NEW]
│   ├── ShareAlbumToGroupCommandHandler.cs      [NEW]
│   ├── RemovePhotoFromGroupCommandHandler.cs   [NEW]
│   ├── RemoveAlbumFromGroupCommandHandler.cs   [NEW]
│   ├── GetGroupsQueryHandler.cs                [NEW]
│   ├── GetGroupQueryHandler.cs                 [NEW]
│   ├── GetGroupMembersQueryHandler.cs          [NEW]
│   ├── GetGroupContentQueryHandler.cs          [NEW]
│   ├── GetGroupPhotosQueryHandler.cs           [NEW]
│   └── GetGroupAlbumsQueryHandler.cs           [NEW]
└── Validators/
    ├── CreateGroupCommandValidator.cs          [NEW]
    ├── UpdateGroupCommandValidator.cs          [NEW]
    ├── AddGroupMemberCommandValidator.cs       [NEW]
    ├── RemoveGroupMemberCommandValidator.cs    [NEW]
    ├── LeaveGroupCommandValidator.cs           [NEW]
    ├── SharePhotoToGroupCommandValidator.cs    [NEW]
    └── ShareAlbumToGroupCommandValidator.cs    [NEW]
```

### Application Layer - Common/DTOs
```
src/MyPhotoBooth.Application/Common/DTOs/
├── GroupDTOs.cs                                [NEW]
│   ├── GroupRequest
│   ├── GroupResponse
│   ├── GroupMemberResponse
│   ├── GroupContentResponse
│   └── CreateGroupRequest
```

### Application Layer - Common/Errors
```
src/MyPhotoBooth.Application/Common/Errors.cs  [MODIFY]
// Add:
// - Groups.NotFound
// - Groups.UnauthorizedAccess
// - Groups.NotAMember
// - Groups.AlreadyAMember
// - Groups.NotOwner
// - Groups.LastOwnerCannotLeave
// - Groups.ContentNotShared
// - Groups.MemberNotFound
// - Groups.InvalidEmail
```

### Application Layer - Interfaces (Services)
```
src/MyPhotoBooth.Application/Interfaces/IEmailService.cs  [MODIFY]
// Add:
// - Task SendGroupInvitationEmailAsync(...)
// - Task SendGroupMemberAddedEmailAsync(...)
// - Task SendGroupDeletionScheduledEmailAsync(...)
// - Task SendGroupDeletionReminderEmailAsync(...)
// - etc.
```

### Infrastructure Layer - Repositories
```
src/MyPhotoBooth.Infrastructure/Persistence/Repositories/
└── GroupRepository.cs                           [NEW]
```

### Infrastructure Layer - Background Services
```
src/MyPhotoBooth.Infrastructure/BackgroundServices/
├── GroupCleanupBackgroundService.cs            [NEW]
└── ScheduledTaskBase.cs                        [NEW] (optional helper)
```

### Infrastructure Layer - Email Templates
```
src/MyPhotoBooth.Infrastructure/Email/Templates/
├── GroupInvitation.html                        [NEW]
├── GroupInvitation.text                        [NEW]
├── GroupMemberAdded.html                       [NEW]
├── GroupMemberRemoved.html                     [NEW]
├── GroupMemberLeft.html                        [NEW]
├── GroupOwnershipTransferred.html              [NEW]
├── GroupDeletionScheduled.html                 [NEW]
├── GroupDeletionScheduled.text                 [NEW]
├── GroupDeletionReminder.html                  [NEW]
└── GroupDeletionReminder.text                  [NEW]
```

### API Layer - Controllers
```
src/MyPhotoBooth.API/Controllers/
└── GroupsController.cs                         [NEW]
```

### API Layer - Configuration
```
src/MyPhotoBooth.API/appsettings.json          [MODIFY]
// Add:
// "GroupSettings": {
//   "DeletionDays": 90,
//   "MemberContentGraceDays": 7,
//   "ReminderDays": [60, 30, 7, 1]
// }
```

### API Layer - Program.cs
```
src/MyPhotoBooth.API/Program.cs                [MODIFY]
// Add: builder.Services.AddHostedService<GroupCleanupBackgroundService>();
```

### Infrastructure Layer - DependencyInjection
```
src/MyPhotoBooth.Infrastructure/DependencyInjection.cs  [MODIFY]
// Add: services.AddScoped<IGroupRepository, GroupRepository>();
// Add: services.AddSingleton<GroupCleanupBackgroundService>();
```

### Infrastructure Layer - DbContext
```
src/MyPhotoBooth.Infrastructure/Persistence/AppDbContext.cs  [MODIFY]
// Add DbSets:
// - public DbSet<Group> Groups => Set<Group>();
// - public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
// - public DbSet<GroupSharedContent> GroupSharedContent => Set<GroupSharedContent>();

// Add OnModelCreating configuration for all three entities
```

### Tests - Unit
```
tests/MyPhotoBooth.UnitTests/Features/Groups/
├── Commands/
│   ├── CreateGroupCommandHandlerTests.cs      [NEW]
│   ├── AddGroupMemberCommandHandlerTests.cs   [NEW]
│   ├── LeaveGroupCommandHandlerTests.cs       [NEW]
│   └── ... (remaining command handlers)
├── Queries/
│   ├── GetGroupsQueryHandlerTests.cs          [NEW]
│   └── ... (remaining query handlers)
└── Validators/
    ├── CreateGroupCommandValidatorTests.cs    [NEW]
    ├── AddGroupMemberCommandValidatorTests.cs [NEW]
    └── ... (remaining validators)
```

### Tests - Integration
```
tests/MyPhotoBooth.IntegrationTests/Features/Groups/
└── GroupsEndpointTests.cs                     [NEW]
```

### Documentation
```
docs/
├── plans/
│   └── group-feature-implementation.md        [THIS FILE]
└── architecture/
    └── groups-domain-model.md                 [NEW]
```

---

## Testing Strategy

### Unit Tests (Target: 70% coverage)

**Validators** (100% coverage mandatory per TFD rules):
- All validation rules
- Edge cases (null, empty, invalid formats)
- Custom validators (GroupNameValidator, EmailValidator)

**Handlers** (70% coverage target):
- Happy path scenarios
- Error cases (not found, unauthorized, invalid state)
- Business logic (ownership checks, permission validation)

**Behaviors**:
- ValidationBehavior works for all group commands
- LoggingBehavior captures group operations
- TransactionBehavior wraps group mutations

**Background Service**:
- Deletion scheduling logic
- Reminder scheduling
- Grace period calculations
- Time-based edge cases

### Integration Tests (Target: 100% endpoint coverage)

**TestWebApplicationFactory**:
- MockEmailService for email verification
- PostgreSQL via Testcontainers
- Auth helper for user creation

**Test Scenarios**:
- Create group → verify DB + response
- Add member → verify member record + email sent
- Share photo → verify content record
- Leave group → verify grace period set
- Owner leaves without transfer → verify deletion scheduled
- Unauthorized access → verify 403

**Time Manipulation**:
- Use TimeSpan overrides for grace period testing
- Test reminder scheduling with configurable dates

### E2E Tests (Future)

- User creates group and invites members
- Members join and share content
- Owner leaves, verify deletion process
- Verify all emails sent correctly

---

## Migration Plan

### Pre-Migration Checklist

- [ ] Backup database
- [ ] Review migration SQL
- [ ] Test migration on staging
- [ ] Plan rollback strategy

### Migration Steps

```bash
# Create migration
dotnet ef migrations add AddGroups \
  --project src/MyPhotoBooth.Infrastructure \
  --startup-project src/MyPhotoBooth.API

# Review generated migration
# Add indexes for performance optimization

# Apply to development
dotnet ef database update \
  --project src/MyPhotoBooth.Infrastructure \
  --startup-project src/MyPhotoBooth.API

# Apply to production (via deployment pipeline)
```

### Rollback Strategy

```bash
# Rollback to previous migration
dotnet ef database update [PreviousMigration] \
  --project src/MyPhotoBooth.Infrastructure \
  --startup-project src/MyPhotoBooth.API
```

### Data Migration (N/A for v1)

- No existing data to migrate
- Groups is a net-new feature

---

## Risk Assessment

### Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Background service race conditions | Medium | High | Use database locks, idempotent operations |
| Email delivery failures | Low | Medium | Retry logic, failure logging, admin alerts |
| Large group performance issues | Low | Medium | Pagination, database indexes, query optimization |
| Soft-delete data bloat | Medium | Low | Periodic cleanup job, hard delete after 1 year |
| Member content orphaning | Low | Medium | Cascading deletes, foreign key constraints |

### Business Logic Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Owner leaves without notification | Low | High | Multiple reminder emails, admin override |
| Member content deleted prematurely | Low | High | Extendable grace period, content recovery |
| Spam group creation | Medium | Low | Rate limiting, user group limits |

### Operational Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Background service stops | Low | High | Health checks, auto-restart, monitoring |
| Database migration fails | Low | High | Pre-migration testing, rollback plan |
| Email service down | Medium | Medium | Queue emails for retry, status page |

---

## Open Questions

1. **Group Limits**: Should there be a maximum number of members per group? If so, what's the default?
   - **Recommendation**: Start with no limit, monitor performance, add limit in v1.1 if needed

2. **Member Invitation Flow**: Should invitations require acceptance or are members auto-added?
   - **Recommendation**: Auto-add for v1 (simpler), add invitation acceptance in v1.2

3. **Non-Registered Users**: Can owners invite users who haven't registered yet?
   - **Recommendation**: No for v1 (email must match existing user), add in v1.3

4. **Group Visibility**: Should groups be searchable/discoverable by email?
   - **Recommendation**: No for v1 (private only), consider public groups in v2.0

5. **Content Permissions**: Can members edit shared content (e.g., add tags)?
   - **Recommendation**: No for v1 (view-only), consider collaborative editing in v1.5

6. **Deletion Override**: Can admins intervene in deletion process?
   - **Recommendation**: No for v1, add admin override API in v1.4

7. **Email Configuration**: Where should GroupSettings be configured?
   - **Recommendation**: appsettings.json with environment overrides

8. **Frontend Framework**: React patterns for group management?
   - **Recommendation**: Create separate frontend plan after backend stable

9. **API Versioning**: Should groups be under /api/v1/groups or /api/groups?
   - **Recommendation**: /api/groups for v1 (no versioning yet), add versioning in v2.0

10. **Testing Database**: Use Testcontainers or mocks for unit tests?
    - **Recommendation**: Mocks for unit tests (faster), Testcontainers for integration tests

---

## Configuration

### appsettings.json additions

```json
{
  "GroupSettings": {
    "DeletionDays": 90,
    "MemberContentGraceDays": 7,
    "ReminderDays": [60, 30, 7, 1],
    "MaxMembersPerGroup": null,
    "MaxGroupsPerUser": null,
    "CleanupServiceInterval": "01:00:00",
    "EnableDeletionSchedule": true
  }
}
```

### Environment Variables (Production)

```bash
GroupSettings__DeletionDays=90
GroupSettings__MemberContentGraceDays=7
GroupSettings__EnableDeletionSchedule=true
```

---

## Next Steps

1. **Review & Approval** (1 hour)
   - Stakeholder review of requirements
   - Architecture decision sign-off
   - Timeline confirmation

2. **Phase 1 Implementation** (16 hours)
   - Set up branch: `feature/groups`
   - Create domain entities
   - Implement foundation layer
   - Write tests

3. **Phase 2-4** (34 hours)
   - Sequential implementation of remaining phases
   - Continuous testing and validation

4. **Release Preparation** (10 hours)
   - Documentation updates
   - Performance testing
   - Security review

5. **Deployment** (2 hours)
   - Database migration
   - Feature flag rollout
   - Monitoring setup

---

## Appendix

### A. Error Messages (Errors.cs additions)

```csharp
public static class Groups
{
    public const string NotFound = "Group not found";
    public const string UnauthorizedAccess = "You do not have access to this group";
    public const string NotAMember = "You are not a member of this group";
    public const string AlreadyAMember = "User is already a member of this group";
    public const string NotOwner = "Only the group owner can perform this action";
    public const string LastOwnerCannotLeave = "The last owner cannot leave without transferring ownership";
    public const string ContentNotShared = "Content is not shared to this group";
    public const string MemberNotFound = "Member not found in group";
    public const string InvalidEmail = "Invalid email address";
    public const string UserNotFound = "User not found";
    public const string CannotRemoveOwner = "Cannot remove the group owner";
    public const string DeletionAlreadyScheduled = "Group deletion is already scheduled";
    public const string GroupIsDeleted = "This group has been deleted";
}
```

### B. Email Template Variables

```csharp
// GroupInvitation
{ "appName", groupName, inviterEmail, inviteLink, year }

// GroupMemberAdded
{ "appName", groupName, addedByEmail, memberEmail, year }

// GroupDeletionScheduled
{ "appName", groupName, deletionDate, ownerEmail, daysRemaining, year }

// GroupDeletionReminder
{ "appName", groupName, deletionDate, daysRemaining, year }
```

### C. Background Service Pseudo-code

```csharp
public class GroupCleanupBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessGroupDeletions(stoppingToken);
            await ProcessMemberContentRemoval(stoppingToken);
            await SendDeletionReminders(stoppingToken);

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
```

---

**Document Version**: 1.0
**Last Updated**: 2026-02-10
**Author**: AI Planning Agent
**Status**: Ready for Review
