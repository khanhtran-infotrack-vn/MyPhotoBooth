# User Data Consistency Bug Report

**Date**: 2025-02-10
**Tester**: QA Engineer
**Application**: MyPhotoBooth v1.1.0

---

## Executive Summary

**CRITICAL BUG IDENTIFIED**: User data (displayName, email) is lost on page refresh, causing inconsistent display of user information throughout the application.

**Severity**: High
**Priority**: P1

---

## Test Results Overview

| Metric | Value |
|--------|-------|
| Test Scenarios Executed | 8 |
| Passed | 4 |
| Failed | 4 |
| Blocked | 0 |

---

## Bug Details

### Title
User data not persisted after page refresh - displays "User" fallback instead of actual user information

### Steps to Reproduce

1. Navigate to http://localhost:3000/register
2. Create a new account (e.g., "Test User" / testuser@example.com / Test12345)
3. Login with the newly created credentials
4. **Observe**: User displays correctly ("testuser@example.com" in sidebar, "T" avatar in header)
5. Refresh the page (F5 or browser refresh button)
6. **Observe**: User data changes to "User" / "U"

### Expected Behavior

After page refresh, user information should remain consistent:
- Sidebar: Should display "testuser@example.com"
- Top Bar Avatar: Should display "T"
- User Menu: Should display "Test User" (displayName) and "testuser@example.com" (email)

### Actual Behavior

After page refresh:
- Sidebar: Displays "User" instead of email
- Top Bar Avatar: Displays "U" instead of first letter of email
- User Menu: Displays "User" with no email shown

---

## Root Cause Analysis

### Location
`/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/stores/authStore.ts`

### Issue Details

The `authStore` initializes `user` state to `null` on every page load:

```typescript
export const useAuthStore = create<AuthState>((set) => ({
  isAuthenticated: !!localStorage.getItem('accessToken'),  // ✅ Restored from localStorage
  user: null,  // ❌ Always null - never restored!
```

The `checkAuth()` function only updates `isAuthenticated` but never restores user data:

```typescript
checkAuth: () => {
  const token = localStorage.getItem('accessToken');
  set({ isAuthenticated: !!token });
  // BUG: user is never restored!
},
```

The `login` function correctly sets user data from the API response, but this data is never persisted to localStorage or restored on app initialization.

### Why It Works Initially but Fails After Refresh

1. **Initial Login**: `login()` sets `user: { email, displayName }` from API response ✅
2. **Before Refresh**: Components render with correct user data ✅
3. **After Refresh**: Zustand store re-initializes, `user` becomes `null` ❌
4. **Components**: Fall back to `user?.displayName || user?.email || 'User'` which evaluates to `'User'` ❌

### Impact on Components

**Affected Components:**
- `src/client/src/components/layout/TopBar.tsx` (lines 95, 102-104)
- `src/client/src/components/layout/Sidebar.tsx` (lines 122-132)

**Fallback Logic:**
```typescript
// TopBar.tsx line 95
{(user?.displayName || user?.email || 'U')[0].toUpperCase()}

// TopBar.tsx lines 102-104
{user?.displayName || 'User'}  // Shows "User" when user is null
```

---

## Network Analysis

✅ **No authentication issues detected**
- All API requests return 200 status
- JWT tokens are properly stored in localStorage
- Token refresh mechanism works correctly
- CORS configured correctly (localhost:3000)

---

## Test Execution Details

### Test Scenario 1: Initial Login ✅
- Result: User displays correctly
- Sidebar: "testuser@example.com"
- Top Bar: "T"
- User Menu: "User" (displayName not shown due to separate issue)

### Test Scenario 2: Navigate to Albums ✅
- Result: User data persists during navigation
- Same as Scenario 1

### Test Scenario 3: Navigate to Tags ✅
- Result: User data persists during navigation
- Same as Scenario 1

### Test Scenario 4: Page Refresh on Tags ❌
- Result: **User data lost**
- Sidebar: "User" (expected: "testuser@example.com")
- Top Bar: "U" (expected: "T")
- User Menu: "User" with no email

### Test Scenario 5: Navigate to Shares ❌
- Result: User data still lost after refresh
- Sidebar: "User"
- Top Bar: "U"

### Test Scenario 6: Logout and Re-login ✅
- Result: User data restored after fresh login
- Sidebar: "testuser@example.com"
- Top Bar: "T"

### Test Scenario 7: Navigate to Albums and Refresh ❌
- Result: **Bug reproduced again**
- Sidebar: "User"
- Top Bar: "U"

### Test Scenario 8: User Menu After Refresh ❌
- Result: Email completely missing from dropdown
- Shows only: "User" and "Sign out"
- Expected: "Test User", "testuser@example.com", "Sign out"

---

## Recommendations

### Fix 1: Persist User Data to localStorage (Recommended)

**Update `authStore.ts`:**

```typescript
// In login function
login: async (email: string, password: string) => {
  const { data } = await api.post('/auth/login', { email, password });
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);
  // ADD: Persist user data
  localStorage.setItem('user', JSON.stringify({ email, displayName: data.displayName }));
  set({ isAuthenticated: true, user: { email, displayName: data.displayName } });
},

// In logout function
logout: async () => {
  // ... existing code ...
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  // ADD: Remove user data
  localStorage.removeItem('user');
  set({ isAuthenticated: false, user: null });
},

// In checkAuth function
checkAuth: () => {
  const token = localStorage.getItem('accessToken');
  const userStr = localStorage.getItem('user');
  const user = userStr ? JSON.parse(userStr) : null;
  set({ isAuthenticated: !!token, user });
},
```

**AND initialize store with restored data:**

```typescript
export const useAuthStore = create<AuthState>((set) => ({
  isAuthenticated: !!localStorage.getItem('accessToken'),
  // FIX: Restore user from localStorage
  user: (() => {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  })(),
  // ... rest of store
}));
```

### Fix 2: Add User Info to JWT (Alternative)

The backend already includes `displayName` in the JWT payload. The frontend could decode the JWT to extract user info:

```typescript
// Helper function
const getUserFromToken = () => {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return {
      email: payload.email,
      displayName: payload.displayName
    };
  } catch {
    return null;
  }
};

// In store initialization
user: getUserFromToken(),
```

### Fix 3: Add API Endpoint for Current User (Most Robust)

Create `/api/auth/me` endpoint to return current user info based on JWT token. Call this on app initialization if token exists.

---

## Additional Observations

1. **Token Storage**: JWT tokens correctly stored and survive refresh ✅
2. **Auth State**: `isAuthenticated` correctly restored from localStorage ✅
3. **User State**: `user` object never persisted or restored ❌
4. **API Integration**: Login endpoint returns both tokens and user data ✅
5. **JWT Payload**: Contains `email`, `displayName`, `role`, etc. ✅

---

## Unresolved Questions

1. Should user data be stored in localStorage or fetched from `/api/auth/me` endpoint on initialization?
2. Should user data be synchronized when JWT is refreshed?
3. Are there any security concerns with storing user email/displayName in localStorage?
4. Should the user data update if the user's profile changes on the backend?

---

## Screenshots

### Before Refresh (Correct)
- Sidebar: Shows "testuser@example.com"
- Top Bar: Shows "T" avatar

### After Refresh (Bug)
- Sidebar: Shows "User"
- Top Bar: Shows "U" avatar
- User Menu: Shows "User" with no email

---

## Next Steps

1. Implement Fix 1 (localStorage persistence) as it's the quickest solution
2. Add test case for page refresh scenario
3. Consider implementing Fix 3 (API endpoint) for better data consistency
4. Add user profile update functionality to test data synchronization
