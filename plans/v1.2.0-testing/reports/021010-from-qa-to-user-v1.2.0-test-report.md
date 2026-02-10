# MyPhotoBooth v1.2.0 - Login Flow Completion Test Report

**Date:** 2026-02-10
**Tester:** QA Engineer
**Version:** v1.2.0
**Test Environment:** Development (localhost)

---

## Executive Summary

**Overall Status:** ⚠️ PARTIAL PASS (3/4 Features Fully Working)

| Feature | Status | Notes |
|---------|--------|-------|
| User Data Consistency Fix | ✅ PASS | Working correctly |
| Authenticated User Redirect | ✅ PASS | Working correctly |
| Forgot Password Flow | ⚠️ PARTIAL | Backend endpoints exist, email sending has bug |
| Password Reset Token Security | ✅ PASS | Security measures working |

**Critical Issue Found:** EmailService.cs uses incorrect Mailpit API format (back-end bug)

---

## Test Results Detail

### 1. User Data Consistency Fix ✅ PASS

**Previous Issue:** User display data was lost on page refresh

**Test Steps:**
1. Created new account: v12test@example.com (display name: "Test User v1.2")
2. Logged in successfully
3. Verified user email "v12test@example.com" displayed in sidebar
4. Verified avatar displayed "V" (first letter of display name)
5. Refreshed page (F5)
6. **Result:** User email STILL displays correctly, avatar still shows "V"

**Status:** ✅ **PASS** - Bug fixed successfully

**Screenshot:** `/tmp/v12-test-results.png`

---

### 2. Authenticated User Redirect ✅ PASS

**Expected Behavior:** Logged-in users visiting /login or /redirect to /photos

**Test Steps:**
1. Logged in as v12test@example.com
2. Navigated to http://localhost:3000/login
3. **Result:** Automatically redirected to /photos
4. Navigated to http://localhost:3000/register
5. **Result:** Automatically redirected to /photos
6. Navigated to http://localhost:3000/forgot-password while logged in
7. **Result:** Automatically redirected to /photos

**Status:** ✅ **PASS** - Protected routes working correctly

---

### 3. Forgot Password Flow ⚠️ PARTIAL

**Test Steps:**
1. Logged out, navigated to http://localhost:3000/forgot-password
2. Entered email: v12test@example.com
3. Clicked "Send Reset Link"
4. **Frontend Result:** Success message displayed: "If an account exists for v12test@example.com, you will receive a password reset link shortly."

**Backend Verification:**
- ✅ Backend endpoint `/api/Auth/forgot-password` EXISTS and responds correctly
- ✅ User lookup working (database query executed successfully)
- ✅ Token generation working
- ❌ **Email sending FAILS** - Backend logs show: "Failed to send email via Mailpit. Status: BadRequest"

**Root Cause Analysis:**

The `EmailService.cs` (lines 112-156) uses incorrect Mailpit API format:

**Current Code (WRONG):**
```csharp
var payload = new
{
    From = $"{_fromName} <{_fromEmail}>",
    To = new[] { toEmail },
    Subject = subject,
    HTML = htmlContent,
    Text = plainTextContent ?? htmlContent
};
```

**Mailpit Expects:**
```json
{
  "from": {"name": "Name", "email": "email@example.com"},
  "to": [{"name": "", "email": "recipient@example.com"}],
  "subject": "Subject",
  "text": "Plain text",
  "html": "<html>HTML</html>"
}
```

**Test Verification:**
```bash
# Correct format works:
curl 'http://localhost:8025/api/v1/send' -X POST \
  --data '{"from":{"name":"Test","email":"test@example.com"},"to":[{"email":"recipient@example.com"}],"subject":"Test","text":"Test"}'
# Response: {"ID":"MGE5sSjYSP4kPNedXUFFnQ"} ✅

# Backend format fails:
curl 'http://localhost:8025/api/v1/send' -X POST \
  --data '{"From":"Name <email>","To":["email"],"Subject":"Test"}'
# Response: {"Error":"json: cannot unmarshal string..."} ❌
```

**Status:** ⚠️ **PARTIAL** - Feature implemented but non-functional due to email format bug

**Fix Required:**
File: `/src/MyPhotoBooth.Infrastructure/Email/EmailService.cs` lines 126-133
Change `SendViaMailpitAsync` method to use object format for from/to fields

---

### 4. Password Reset Token Security ✅ PASS

**Test A: User Enumeration Protection**
1. Submitted forgot password for non-existent email: `nonexistent@example.com`
2. **Result:** Same success message as valid email: "If an account exists for nonexistent@example.com, you will receive a password reset link shortly."
3. **Status:** ✅ PASS - User enumeration prevented

**Test B: Invalid Token Handling**
1. Navigated to: `http://localhost:3000/reset-password?token=invalid-token-12345&email=v12test@example.com`
2. Entered new password: `NewPassword123`
3. Clicked "Reset Password"
4. **Result:** Error message displayed: "Failed to reset password: Invalid token."
5. **Status:** ✅ PASS - Proper error handling

**Test C: Original Password Unchanged**
1. After invalid token attempt, logged in with original password
2. **Result:** Successfully logged in
3. **Status:** ✅ PASS - Security maintained

---

## Browser Console Issues

**Minor Warnings (Non-blocking):**
- 1 issue: Form field element missing id or name attribute (accessibility)

No JavaScript errors or network request failures affecting functionality.

---

## API Endpoints Verified

**Available in OpenAPI spec:**
- ✅ POST `/api/Auth/register`
- ✅ POST `/api/Auth/login`
- ✅ POST `/api/Auth/refresh`
- ✅ POST `/api/Auth/logout`
- ✅ POST `/api/Auth/forgot-password`
- ✅ POST `/api/Auth/reset-password`

All endpoints properly registered and documented.

---

## Recommendations

### Critical (Must Fix Before Release)
1. **Fix EmailService.cs Mailpit API format** - Use object format for from/to fields
2. **Test full forgot password flow** after email fix is deployed

### High Priority
1. Add automated unit tests for email service with correct API format
2. Add integration test for complete password reset flow

### Medium Priority
1. Fix form field accessibility (missing id/name attributes)
2. Add email template testing to CI/CD pipeline

### Low Priority
1. Consider adding email rate limiting
2. Add logging for successful password resets

---

## Unresolved Questions

1. **Why does the existing test suite not catch the email format bug?**
   - Recommendation: Add API contract tests for external service integrations

2. **Should we add a fallback when Mailpit is unavailable?**
   - Current: Logs warning but continues silently
   - Consider: Developer dashboard notification or fallback to SMTP

3. **Email token expiration - is 2 hours appropriate?**
   - Current implementation uses 2 hours
   - No security concern identified, but worth reviewing

---

## Test Environment

**Services Running:**
- ✅ PostgreSQL (Docker): localhost:5432
- ✅ Backend API: localhost:5149
- ✅ Frontend (Vite): localhost:3000
- ✅ Mailpit: localhost:8025
- ❌ Email sending to Mailpit: **FAILING** (format issue)

**Test Account Created:**
- Email: v12test@example.com
- Password: Password123
- Display Name: Test User v1.2

---

## Conclusion

**v1.2.0 Status:** NOT READY FOR PRODUCTION

**Blocking Issues:**
1. EmailService.cs Mailpit API format bug (email sending fails)

**Non-Blocking Issues:**
1. Minor accessibility warnings

**Once Critical Issue is Fixed:**
- All 4 features will be fully functional
- Ready for production deployment

**Estimated Time to Fix:** 15-30 minutes (simple object format change)

---

**Report Generated:** 2026-02-10
**Test Duration:** ~45 minutes
**Test Coverage:** 100% of v1.2.0 features tested
