# Group Feature - Architectural Decisions Record

**Date**: 2026-02-10
**Status**: Approved
**Related Plan**: `group-feature-implementation.md`

---

## Summary

This document captures all architectural decisions for the Group feature, resolving the 10 open questions from the implementation plan. These decisions are FINAL and will guide the implementation.

---

## Decision 1: Group Limits

**Question**: Should there be a maximum number of members per group? If so, what's the default?

**ANSWER**: **YES - 50 members max per group**

**Rationale**:
- Performance protection: Large groups cause N+1 queries and slow response times
- Email notification limits: Avoid rate limiting with large blasts
- Database index efficiency: Keep indexes manageable
- Industry standard: Similar to Google Photos shared albums (50-500)
- Configurable for future scaling if needed

**Implementation**:
```csharp
// GroupSettings
public int MaxMembersPerGroup { get; set; } = 50;
```

**Validation**:
- AddGroupMemberCommandValidator checks against current member count
- Return `Groups.GroupFull` error if limit reached

---

## Decision 2: Member Invitation Flow

**Question**: Should invitations require acceptance or are members auto-added?

**ANSWER**: **AUTO-ADD for v1 (simpler)**

**Rationale**:
- v1 scope: Focus on core functionality
- Private groups: Only known collaborators being added
- Faster time-to-value: No invitation acceptance UI needed yet
- Email notification still sent for transparency

**Implementation**:
- Member immediately added to GroupMembers table
- Email sent: "You've been added to [Group Name]"
- Leave group always available if unwanted

**Future (v1.3)**: Add invitation/acceptance flow for public groups

---

## Decision 3: Non-Registered Users

**Question**: Can owners invite users who haven't registered yet?

**ANSWER**: **NO for v1 - email must match existing registered user**

**Rationale**:
- Simpler authentication flow
- No pending invitation storage needed
- Email already verified during registration
- Prevents spam invitations to random emails

**Implementation**:
- AddGroupMemberCommandValidator checks if email exists in AspNetUsers
- Return `Groups.UserNotFound` if not registered
- Error message: "User is not registered. Please ask them to sign up first."

**Future (v1.4)**: Add pending invitations table for unregistered users

---

## Decision 4: Group Visibility

**Question**: Should groups be searchable/discoverable by email?

**ANSWER**: **NO for v1 - private groups only**

**Rationale**:
- Privacy-first design
- Focus on personal collaboration (friends/family)
- No search/discovery UI needed yet
- Simpler permission model

**Implementation**:
- Groups only visible to members and owners
- No public group listing endpoint
- No group search functionality

**Future (v2.0)**: Consider public/discoverable groups if demand exists

---

## Decision 5: Content Permissions

**Question**: Can members edit shared content (e.g., add tags)?

**ANSWER**: **NO for v1 - view-only for shared content**

**Rationale**:
- Ownership model: Sharer retains full control
- Simpler permission matrix
- Avoid conflicts (two members editing same photo tags)
- Consistent with existing ShareLink model

**Permissions Matrix**:

| Action | Owner | Member (who shared) | Member (didn't share) |
|--------|-------|---------------------|----------------------|
| View group content | ✅ | ✅ | ✅ |
| Share own content | ✅ | ✅ | ✅ |
| Remove own content | ✅ | ✅ | ❌ |
| Remove others' content | ✅ | ❌ | ❌ |
| Edit any content | ✅ | ❌ | ❌ |
| Add/remove members | ✅ | ❌ | ❌ |
| Transfer ownership | ✅ | ❌ | ❌ |
| Delete group | ✅ | ❌ | ❌ |

**Implementation**:
- Command handlers check `content.SharedByUserId == currentUserId`
- Owner can remove any content (override)

---

## Decision 6: Deletion Override

**Question**: Can admins intervene in deletion process?

**ANSWER**: **NO for v1 - no admin role exists yet**

**Rationale**:
- No admin role in current application
- Adds complexity to permission model
- 90-day window sufficient for owner intervention
- Owner can re-transfer ownership before deletion

**Current Override**:
- Owner can return within 90 days and transfer ownership
- This cancels deletion schedule

**Future (v1.5)**: Add admin roles with override capabilities

---

## Decision 7: Email Configuration

**Question**: Where should GroupSettings be configured?

**ANSWER**: **appsettings.json with IOptions pattern**

**Implementation**:
```json
// appsettings.Development.json
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

```csharp
// GroupSettings.cs
public class GroupSettings
{
    public const string SectionName = "GroupSettings";
    public int MaxMembersPerGroup { get; set; } = 50;
    public int DeletionDays { get; set; } = 90;
    public int MemberContentGraceDays { get; set; } = 7;
    public int[] ReminderDays { get; set; } = new[] { 60, 30, 7, 1 };
    public string CleanupServiceInterval { get; set; } = "01:00:00";
}
```

```csharp
// DependencyInjection.cs
services.Configure<GroupSettings>(
    configuration.GetSection(GroupSettings.SectionName));
```

**Environment Overrides**:
- Development: 1-day deletion for testing
- Production: 90-day deletion

---

## Decision 8: Frontend Framework

**Question**: React patterns for group management?

**ANSWER**: **OUT OF SCOPE for v1 - backend-only implementation**

**Rationale**:
- Backend complexity warrants focused planning
- Frontend has different concerns (UI state, routing, components)
- Can be implemented in parallel after backend API is stable
- Plan to be created after backend is tested and documented

**Future Frontend Plan** (separate document):
- Groups list page with status indicators
- Group detail page with tabs (members, content, settings)
- Member management UI (add/remove/transfer)
- Share-to-group flow in photo/album views
- Deletion warning UI with countdown

---

## Decision 9: API Versioning

**Question**: Should groups be under `/api/v1/groups` or `/api/groups`?

**ANSWER**: **`/api/groups` - no versioning for v1**

**Rationale**:
- Current API has no versioning (Photos, Albums, Tags, ShareLinks)
- Consistency with existing patterns
- Versioning adds complexity without benefit yet
- Can add versioning in v2.0 if breaking changes needed

**Endpoints**:
```
POST   /api/groups
GET    /api/groups
GET    /api/groups/{id}
PUT    /api/groups/{id}
DELETE /api/groups/{id}
```

**Future (v2.0)**: Add `/api/v2/` if breaking changes introduced

---

## Decision 10: Testing Database

**Question**: Use Testcontainers or mocks for unit tests?

**ANSWER**: **Mock repository for unit tests, Testcontainers for integration tests**

**Rationale**:
- **Unit tests**: Fast execution, focused on business logic
  - Mock `IGroupRepository`
  - Test handlers in isolation
  - Target: 70% coverage
- **Integration tests**: Real database, end-to-end validation
  - Testcontainers with PostgreSQL
  - Test EF Core configuration
  - Target: 100% endpoint coverage

**Implementation**:

```csharp
// Unit test example
public class CreateGroupCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _mockRepo;

    [Fact]
    public async Task Handle_ValidRequest_CreatesGroup()
    {
        // Arrange
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

```csharp
// Integration test example
public class GroupsEndpointTests : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task CreateGroup_ReturnsCreatedGroup()
    {
        // Uses Testcontainers PostgreSQL
        // Real HTTP requests to API
        // Full stack validation
    }
}
```

---

## Additional Decisions (Not in Original 10)

### Decision 11: Grace Period Recovery

**QUESTION**: Can members recover their content during grace period?

**ANSWER**: **YES - members can rejoin to extend grace period**

**Rationale**:
- User-friendly: Accidental leaves shouldn't lose data
- Simple implementation: Rejoining clears `LeftAt`
- 7-day window provides ample recovery time

### Decision 12: Deletion Cancellation

**QUESTION**: How can deletion be cancelled after owner leaves?

**ANSWER**: **Owner rejoins and transfers ownership to another member**

**Rationale**:
- Self-service: No admin intervention needed
- Clear intent: Owner returning + transferring = group saved
- Cancels deletion schedule automatically

### Decision 13: Notification Preferences

**QUESTION**: Should users be able to opt-out of group emails?

**ANSWER**: **NO for v1 - all group emails mandatory**

**Rationale**:
- Critical notifications (deletion) cannot be optional
- Simpler implementation: No preferences table
- Users can leave group if unwanted
- Email client filters available for users

### Decision 14: Concurrent Membership Changes

**QUESTION**: How to handle race conditions in membership changes?

**ANSWER**: **Database unique constraint + optimistic concurrency**

**Implementation**:
- Unique constraint on `(GroupId, UserId)` prevents duplicates
- EF Core handles concurrent updates
- Return `Groups.AlreadyAMember` on constraint violation

### Decision 15: Group Name Uniqueness

**QUESTION**: Should group names be unique per user?

**ANSWER**: **NO - users can have multiple groups with same name**

**Rationale**:
- Flexibility: "Family 2024", "Family 2025"
- User disambiguation: Can edit description
- No technical constraint preventing duplicates
- Display order/created_at helps distinguish

---

## Implementation Priority Based on Decisions

### Phase 1: Foundation (Critical Path)
- Group limits (Decision 1)
- Configuration (Decision 7)

### Phase 2: Membership
- Auto-add flow (Decision 2)
- Registered users only (Decision 3)
- Private visibility (Decision 4)

### Phase 3: Content Sharing
- View-only permissions (Decision 5)

### Phase 4: Lifecycle
- No admin override (Decision 6)
- Self-service cancellation (Decision 12)

### Phase 5: Polish
- Testing strategy (Decision 10)
- API versioning (Decision 9)

---

## Configuration Defaults (Final)

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

**Development Override**:
```json
{
  "GroupSettings": {
    "DeletionDays": 1,
    "MemberContentGraceDays": 0,
    "ReminderDays": [0]
  }
}
```

---

## Sign-Off

**Approved By**: Implementation Team
**Date**: 2026-02-10
**Status**: Ready for Implementation

These decisions are binding for v1.4.0 release. Any changes require formal review and documentation update.
