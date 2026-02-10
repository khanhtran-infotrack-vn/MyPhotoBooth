# Dark Mode & Email Templates Implementation Plan

## Overview

This document provides comprehensive implementation plans for two new features in MyPhotoBooth:
1. **Dark Mode Support for Web UI**
2. **Separate Email Template Files**

---

# Feature 1: Dark Mode Support for Web UI

## Architecture Recommendations

### Technology Stack Decision
**Use Tailwind CSS v4 native dark mode support** (no additional libraries needed)

**Rationale:**
- Tailwind v4 has built-in dark mode with `dark:` prefix
- Already using Tailwind CSS v4.1.18
- Zustand persist middleware already in use for UI state
- Zero additional dependencies
- Performance optimized (CSS-based, not JS runtime)

### Implementation Approach
**CSS Custom Properties + Tailwind Dark Mode + System Preference Detection**

1. **Three-state toggle:** Light | Dark | System (auto)
2. **Tailwind class-based strategy** (not media query)
3. **localStorage persistence** via Zustand persist middleware
4. **System preference listener** with `matchMedia`
5. **CSS variables** for theme values (colors, gradients)

### Color Palette Design

#### Light Mode (Current)
```css
--bg-primary: #ffffff (white)
--bg-secondary: #f8fafc (gray-50)
--bg-tertiary: #f1f5f9 (gray-100)
--text-primary: #0f172a (gray-900)
--text-secondary: #475569 (gray-600)
--border-color: #e2e8f0 (gray-200)
--gradient-primary: linear-gradient(135deg, #667eea 0%, #764ba2 100%)
--gradient-secondary: linear-gradient(135deg, #4285f4 0%, #1a73e8 100%)
```

#### Dark Mode (New)
```css
--bg-primary: #0f172a (gray-900)
--bg-secondary: #1e293b (gray-800)
--bg-tertiary: #334155 (gray-700)
--text-primary: #f8fafc (gray-50)
--text-secondary: #94a3b8 (gray-400)
--border-color: #334155 (gray-700)
--gradient-primary: linear-gradient(135deg, #8b5cf6 0%, #a855f7 100%) (lighter purple)
--gradient-secondary: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%) (lighter blue)
```

## Step-by-Step Implementation Plan

### Phase 1: Foundation (Core Infrastructure)

#### Step 1.1: Extend UIStore with Theme State
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/stores/uiStore.ts`

```typescript
import { create } from 'zustand'
import { persist } from 'zustand/middleware'

type ThemeMode = 'light' | 'dark' | 'system'

interface UIState {
  sidebarCollapsed: boolean
  sidebarOpen: boolean
  themeMode: ThemeMode
  toggleSidebarCollapsed: () => void
  setSidebarOpen: (open: boolean) => void
  setThemeMode: (mode: ThemeMode) => void
  getEffectiveTheme: () => 'light' | 'dark'
}

export const useUIStore = create<UIState>()(
  persist(
    (set, get) => ({
      sidebarCollapsed: false,
      sidebarOpen: false,
      themeMode: 'system',
      toggleSidebarCollapsed: () =>
        set((state) => ({ sidebarCollapsed: !state.sidebarCollapsed })),
      setSidebarOpen: (open) => set({ sidebarOpen: open }),
      setThemeMode: (mode) => set({ themeMode: mode }),
      getEffectiveTheme: () => {
        const mode = get().themeMode
        if (mode === 'system') {
          return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
        }
        return mode
      }
    }),
    {
      name: 'ui-storage',
      partialize: (state) => ({
        sidebarCollapsed: state.sidebarCollapsed,
        themeMode: state.themeMode
      }),
    }
  )
)
```

#### Step 1.2: Create Theme Provider Hook
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/hooks/useTheme.ts`

```typescript
import { useEffect } from 'react'
import { useUIStore } from '../stores/uiStore'

export function useTheme() {
  const { themeMode, getEffectiveTheme } = useUIStore()

  useEffect(() => {
    const root = document.documentElement
    const effectiveTheme = getEffectiveTheme()

    root.setAttribute('data-theme', effectiveTheme)
    root.classList.remove('light', 'dark')
    root.classList.add(effectiveTheme)

    // Listen for system preference changes
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
    const handleChange = () => {
      if (themeMode === 'system') {
        const newTheme = mediaQuery.matches ? 'dark' : 'light'
        root.setAttribute('data-theme', newTheme)
        root.classList.remove('light', 'dark')
        root.classList.add(newTheme)
      }
    }

    mediaQuery.addEventListener('change', handleChange)
    return () => mediaQuery.removeEventListener('change', handleChange)
  }, [themeMode, getEffectiveTheme])

  return { themeMode, effectiveTheme: getEffectiveTheme() }
}
```

#### Step 1.3: Update Tailwind Configuration
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/styles/globals.css`

Add dark mode color variants to `@theme`:

```css
@import "tailwindcss";

@theme {
  /* Light mode colors */
  --color-bg-primary: #ffffff;
  --color-bg-secondary: #f8fafc;
  --color-bg-tertiary: #f1f5f9;
  --color-text-primary: #0f172a;
  --color-text-secondary: #475569;
  --color-border-default: #e2e8f0;

  /* Dark mode colors */
  --color-dark-bg-primary: #0f172a;
  --color-dark-bg-secondary: #1e293b;
  --color-dark-bg-tertiary: #334155;
  --color-dark-text-primary: #f8fafc;
  --color-dark-text-secondary: #94a3b8;
  --color-dark-border-default: #334155;

  /* Primary colors (unchanged) */
  --color-primary-50: #e8f0fe;
  /* ... existing primary colors ... */

  /* Semantic colors (unchanged) */
  --color-success: #34a853;
  --color-warning: #fbbc04;
  --color-error: #ea4335;

  /* Sidebar width */
  --sidebar-width: 280px;
  --sidebar-collapsed-width: 72px;
}

/* Dark mode overrides */
@layer base {
  html.dark {
    color-scheme: dark;
  }

  html.dark body {
    @apply bg-dark-bg-primary text-dark-text-primary;
  }
}

/* Update component utilities for dark mode */
@layer components {
  .card {
    @apply bg-white dark:bg-dark-bg-secondary rounded-xl shadow-sm border border-gray-200 dark:border-dark-border-default;
  }

  .input {
    @apply w-full px-4 py-2.5 text-sm border border-gray-300 dark:border-dark-border-default rounded-lg
           bg-white dark:bg-dark-bg-secondary placeholder-gray-400 dark:placeholder-dark-text-secondary
           text-gray-900 dark:text-dark-text-primary
           focus:outline-none focus:border-primary-600 focus:ring-1 focus:ring-primary-600
           disabled:bg-gray-100 dark:disabled:bg-dark-bg-tertiary disabled:cursor-not-allowed
           transition-colors duration-200;
  }

  .btn-secondary {
    @apply inline-flex items-center justify-center gap-2 px-4 py-2
           text-sm font-medium rounded-lg transition-all duration-200
           focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2
           disabled:opacity-50 disabled:cursor-not-allowed
           bg-white dark:bg-dark-bg-secondary text-gray-700 dark:text-dark-text-primary border border-gray-300 dark:border-dark-border-default
           hover:bg-gray-50 dark:hover:bg-dark-bg-tertiary active:bg-gray-100 dark:active:bg-dark-bg-tertiary
           focus-visible:ring-gray-500;
  }

  /* Add dark mode for other button classes... */
}

/* Update scrollbar for dark mode */
@layer utilities {
  .scrollbar-thin {
    scrollbar-width: thin;
    scrollbar-color: var(--color-gray-300) transparent;
  }

  html.dark .scrollbar-thin {
    scrollbar-color: var(--color-dark-border-default) transparent;
  }

  .scrollbar-thin::-webkit-scrollbar-thumb {
    background-color: var(--color-gray-300);
    border-radius: 3px;
  }

  html.dark .scrollbar-thin::-webkit-scrollbar-thumb {
    background-color: var(--color-dark-border-default);
  }
}
```

### Phase 2: UI Components

#### Step 2.1: Create Theme Toggle Component
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/theme/ThemeToggle.tsx`

```typescript
import { useState } from 'react'
import { useUIStore } from '../../stores/uiStore'

export function ThemeToggle() {
  const { themeMode, setThemeMode } = useUIStore()
  const [isOpen, setIsOpen] = useState(false)

  const themes = [
    { value: 'light' as const, icon: '☀️', label: 'Light' },
    { value: 'dark' as const, icon: '🌙', label: 'Dark' },
    { value: 'system' as const, icon: '💻', label: 'System' },
  ]

  const currentTheme = themes.find(t => t.value === themeMode)

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="btn-icon"
        title={`Theme: ${currentTheme?.label}`}
      >
        <span className="text-xl">{currentTheme?.icon}</span>
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-40 bg-white dark:bg-dark-bg-secondary rounded-xl shadow-lg border border-gray-200 dark:border-dark-border-default py-2 animate-scale-in">
          {themes.map((theme) => (
            <button
              key={theme.value}
              onClick={() => {
                setThemeMode(theme.value)
                setIsOpen(false)
              }}
              className={`w-full px-4 py-2 text-left text-sm flex items-center gap-3
                ${themeMode === theme.value
                  ? 'bg-primary-50 dark:bg-dark-bg-tertiary text-primary-600 dark:text-primary-400'
                  : 'text-gray-700 dark:text-dark-text-primary hover:bg-gray-100 dark:hover:bg-dark-bg-tertiary'
                }`}
            >
              <span className="text-lg">{theme.icon}</span>
              {theme.label}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
```

#### Step 2.2: Integrate Theme Hook in App
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/App.tsx`

```typescript
import { useTheme } from './hooks/useTheme'
// ... other imports

function App() {
  useTheme() // Initialize theme on app mount
  // ... rest of app
}
```

#### Step 2.3: Add Theme Toggle to TopBar
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/layout/TopBar.tsx`

Add to the actions section, between search and upload:

```typescript
import { ThemeToggle } from '../theme/ThemeToggle'

// In the actions div:
<div className="flex items-center gap-2">
  <ThemeToggle />
  {/* Upload button */}
  <button onClick={onUploadClick} className="btn-primary">
    {/* ... */}
  </button>
  {/* ... rest */}
</div>
```

#### Step 2.4: Update Layout Components for Dark Mode
**Files to update:**
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/layout/TopBar.tsx`
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/layout/Sidebar.tsx`
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/layout/AppShell.tsx`

**Pattern: Add `dark:` variants to all color-related classes**

Example TopBar updates:
```tsx
<header className="sticky top-0 z-30 bg-white dark:bg-dark-bg-primary border-b border-gray-200 dark:border-dark-border-default">
```

Example Sidebar updates:
```tsx
<aside className="... bg-white dark:bg-dark-bg-primary border-r border-gray-200 dark:border-dark-border-default">
```

### Phase 3: Feature Components

#### Step 3.1: Update Photo Grid
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/photos/PhotoGrid.tsx`

Add dark mode to:
- Background colors
- Selection states
- Hover states
- Loading skeletons

#### Step 3.2: Update Lightbox
**Files:**
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/lightbox/Lightbox.tsx`
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/lightbox/LightboxInfo.tsx`
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/lightbox/LightboxActions.tsx`

Lightbox overlay should be darker in dark mode:
```tsx
<div className="fixed inset-0 bg-black/90 dark:bg-black/95 z-50">
```

#### Step 3.3: Update Modals
**Files:**
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/features/upload/PhotoUpload.tsx`
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/features/albums/CreateAlbumModal.tsx`
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/features/albums/AddToAlbumModal.tsx`
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/features/sharing/ShareModal.tsx`

Modal pattern:
```tsx
<div className="fixed inset-0 bg-black/50 dark:bg-black/70 flex items-center justify-center z-50">
  <div className="bg-white dark:bg-dark-bg-secondary rounded-xl shadow-xl max-w-md w-full mx-4">
    {/* Modal content */}
  </div>
</div>
```

#### Step 3.4: Update Auth Pages
**Files:**
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/features/auth/Login.tsx`
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/features/auth/Register.tsx`
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/features/auth/ForgotPassword.tsx`
- `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/features/auth/ResetPassword.tsx`

Auth cards should support dark mode:
```tsx
<div className="min-h-screen bg-gray-50 dark:bg-dark-bg-primary flex items-center justify-center p-4">
  <div className="card bg-white dark:bg-dark-bg-secondary max-w-md w-full">
```

### Phase 4: Testing & Polish

#### Step 4.1: Create Test Checklist
- [ ] All pages render in light mode
- [ ] All pages render in dark mode
- [ ] System preference detection works
- [ ] Theme toggle shows correct icon
- [ ] Theme persists across page reloads
- [ ] Smooth transitions between modes
- [ ] All interactive states (hover, focus, active) work in both modes
- [ ] Gradients look good in dark mode
- [ ] No color contrast issues (WCAG AA)
- [ ] Lightbox works in both modes
- [ ] Modals work in both modes
- [ ] Forms work in both modes
- [ ] Share view (public unauthenticated) works in both modes

#### Step 4.2: Add Transition Animations
Add to `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/styles/globals.css`:

```css
@layer base {
  *, *::before, *::after {
    @apply transition-colors duration-200;
  }

  /* Optimize animations for users who prefer reduced motion */
  @media (prefers-reduced-motion: reduce) {
    *, *::before, *::after {
      transition-property: none !important;
    }
  }
}
```

### Phase 5: Documentation

#### Step 5.1: Update CLAUDE.md
Add dark mode section to project documentation.

## Files to Create

| File Path | Purpose |
|-----------|---------|
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/hooks/useTheme.ts` | Theme initialization hook |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/theme/ThemeToggle.tsx` | Theme toggle UI component |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/theme/index.ts` | Theme barrel export |

## Files to Modify

| File Path | Changes |
|-----------|---------|
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/stores/uiStore.ts` | Add theme state |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/styles/globals.css` | Add dark mode colors and variants |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/App.tsx` | Initialize theme hook |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/layout/TopBar.tsx` | Add theme toggle |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/layout/Sidebar.tsx` | Dark mode classes |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/layout/AppShell.tsx` | Dark mode classes |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/photos/*.tsx` | Dark mode for photo components |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/components/lightbox/*.tsx` | Dark mode for lightbox |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/client/src/features/**/*.tsx` | Dark mode for all features |

## Potential Pitfalls to Avoid

1. **FOUC (Flash of Unstyled Content):** Initialize theme synchronously before React renders
2. **Hydration mismatch:** Use Zustand persist to avoid mismatch between server and client
3. **Missing dark mode for new components:** Create a dark mode checklist for PR reviews
4. **Poor contrast in dark mode:** Test all text colors against dark backgrounds
5. **Gradients that don't work in dark mode:** Use lighter/more vibrant gradients for dark mode
6. **Performance:** Don't add transitions to layout properties (width, height, position)
7. **System preference not updating:** Ensure event listener is properly attached
8. **Forgetting public pages:** Share view must also support dark mode

## Estimated Effort

| Phase | Tasks | Estimated Time |
|-------|-------|----------------|
| Phase 1 | Foundation | 2-3 hours |
| Phase 2 | UI Components | 2-3 hours |
| Phase 3 | Feature Components | 3-4 hours |
| Phase 4 | Testing & Polish | 2-3 hours |
| Phase 5 | Documentation | 0.5 hours |
| **Total** | | **10-14 hours** |

---

# Feature 2: Separate Email Template Files

## Architecture Recommendations

### Technology Stack Decision
**Use simple Handlebars-like template engine with .html files**

**Rationale:**
- Lightweight: No heavy Razor runtime
- Simple: String replacement with `{{variable}}` syntax
- Testable: Can preview templates in browser
- No build step: Templates are plain HTML
- Easy to localize later: Separate .json files for translations
- Designer-friendly: HTML files can be edited by non-developers

### File Format Decision
**.html files with {{variable}} placeholders**

**Example:**
```html
<div class="header">
  <h1>{{title}}</h1>
</div>
<p>Hello {{userName}},</p>
<p>{{message}}</p>
```

### Storage Location
**Directory Structure:**
```
src/MyPhotoBooth.Infrastructure/
├── Email/
│   ├── Templates/
│   │   ├── PasswordReset.html
│   │   ├── PasswordReset.txt
│   │   ├── Welcome.html
│   │   ├── Welcome.txt
│   │   ├── ShareNotification.html
│   │   └── ShareNotification.txt
│   ├── TemplateEngine.cs
│   └── EmailService.cs (refactored)
```

### Template Metadata
**JSON sidecar files for metadata:**
```
src/MyPhotoBooth.Infrastructure/Email/Templates/
├── PasswordReset.html
├── PasswordReset.txt
├── PasswordReset.metadata.json
```

**metadata.json format:**
```json
{
  "subject": "Reset Your Password - MyPhotoBooth",
  "description": "Email sent when user requests password reset",
  "requiredVariables": ["userName", "resetLink", "expiryTime"]
}
```

## Step-by-Step Implementation Plan

### Phase 1: Template Engine

#### Step 1.1: Create TemplateEngine Class
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/Email/TemplateEngine.cs`

```csharp
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyPhotoBooth.Infrastructure.Email;

public interface ITemplateEngine
{
    Task<string> RenderTemplateAsync(string templateName, object model, CancellationToken cancellationToken = default);
    Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> variables, CancellationToken cancellationToken = default);
    Task<EmailTemplate> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default);
}

public class TemplateEngine : ITemplateEngine
{
    private readonly ILogger<TemplateEngine> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _templatesPath;
    private readonly ConcurrentDictionary<string, string> _templateCache;

    public TemplateEngine(ILogger<TemplateEngine> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _templatesPath = _configuration["EmailSettings:TemplatesPath"]
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Email", "Templates");
        _templateCache = new ConcurrentDictionary<string, string>();
    }

    public async Task<EmailTemplate> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default)
    {
        var metadataPath = Path.Combine(_templatesPath, $"{templateName}.metadata.json");
        var htmlPath = Path.Combine(_templatesPath, $"{templateName}.html");
        var textPath = Path.Combine(_templatesPath, $"{templateName}.txt");

        var metadata = await File.ReadAllTextAsync(metadataPath, cancellationToken);
        var htmlContent = await File.ReadAllTextAsync(htmlPath, cancellationToken);
        var textContent = File.Exists(textPath)
            ? await File.ReadAllTextAsync(textPath, cancellationToken)
            : null;

        var metadataDoc = System.Text.Json.JsonDocument.Parse(metadata);
        var subject = metadataDoc.RootElement.GetProperty("subject").GetString() ?? templateName;

        return new EmailTemplate
        {
            Name = templateName,
            Subject = subject,
            HtmlContent = htmlContent,
            TextContent = textContent
        };
    }

    public async Task<string> RenderTemplateAsync(string templateName, object model, CancellationToken cancellationToken = default)
    {
        var variables = ConvertModelToDictionary(model);
        return await RenderTemplateAsync(templateName, variables, cancellationToken);
    }

    public async Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> variables, CancellationToken cancellationToken = default)
    {
        var templatePath = Path.Combine(_templatesPath, $"{templateName}.html");

        var template = _templateCache.GetOrAdd(templateName, _ =>
        {
            _logger.LogDebug("Loading template: {TemplateName}", templateName);
            return File.ReadAllText(templatePath);
        });

        var rendered = template;

        foreach (var kvp in variables)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}";
            rendered = rendered.Replace(placeholder, kvp.Value);
        }

        return rendered;
    }

    private Dictionary<string, string> ConvertModelToDictionary(object model)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var properties = model.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(model)?.ToString() ?? string.Empty;
            dict[prop.Name] = value;
        }

        return dict;
    }
}

public class EmailTemplate
{
    public required string Name { get; set; }
    public required string Subject { get; set; }
    public required string HtmlContent { get; set; }
    public string? TextContent { get; set; }
}
```

### Phase 2: Extract Templates

#### Step 2.1: Create Password Reset Template
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/Email/Templates/PasswordReset.metadata.json`

```json
{
  "subject": "Reset Your Password - MyPhotoBooth",
  "description": "Email sent when user requests password reset",
  "requiredVariables": ["appName", "resetLink", "expiryTime", "year"]
}
```

**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/Email/Templates/PasswordReset.html`

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center; }
        .header h1 { color: white; margin: 0; font-size: 24px; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }
        .button { display: inline-block; padding: 12px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }
        .footer { text-align: center; margin-top: 30px; padding: 20px; color: #666; font-size: 12px; }
        .code { background: #eee; padding: 10px; border-radius: 5px; font-family: monospace; font-size: 14px; word-break: break-all; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Reset Your Password</h1>
        </div>
        <div class='content'>
            <p>Hello,</p>
            <p>We received a request to reset the password for your {{appName}} account.</p>
            <p>Click the button below to reset your password:</p>
            <p style='text-align: center;'>
                <a href='{{resetLink}}' class='button'>Reset Password</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p class='code'>{{resetLink}}</p>
            <p><strong>This link will expire in {{expiryTime}}.</strong></p>
            <p>If you didn't request this password reset, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>&copy; {{year}} {{appName}}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
```

**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/Email/Templates/PasswordReset.txt`

```text
Reset Your Password - {{appName}}

Hello,

We received a request to reset the password for your {{appName}} account.

Click the link below to reset your password:
{{resetLink}}

This link will expire in {{expiryTime}}.

If you didn't request this password reset, please ignore this email.

© {{year}} {{appName}}. All rights reserved.
```

#### Step 2.2: Create Welcome Email Template (Bonus)
**Files:**
- `Welcome.metadata.json`
- `Welcome.html`
- `Welcome.txt`

#### Step 2.3: Create Share Notification Template (Bonus)
**Files:**
- `ShareNotification.metadata.json`
- `ShareNotification.html`
- `ShareNotification.txt`

### Phase 3: Refactor EmailService

#### Step 3.1: Update EmailService to Use Templates
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/Email/EmailService.cs`

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ITemplateEngine _templateEngine;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _provider;
    private readonly string? _mailpitUrl;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IHttpClientFactory httpClientFactory,
        ITemplateEngine templateEngine)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _templateEngine = templateEngine;
        _provider = _configuration["EmailSettings:Provider"] ?? "Mailpit";
        _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@photobooth.local";
        _fromName = _configuration["EmailSettings:FromName"] ?? "MyPhotoBooth";
        _mailpitUrl = _configuration["EmailSettings:MailpitUrl"];
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null, CancellationToken cancellationToken = default)
    {
        if (_provider.Equals("Mailpit", StringComparison.OrdinalIgnoreCase))
        {
            await SendViaMailpitAsync(toEmail, subject, htmlContent, plainTextContent, cancellationToken);
        }
        else if (_provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
        {
            await SendViaSendGridAsync(toEmail, subject, htmlContent, plainTextContent, cancellationToken);
        }
        else
        {
            _logger.LogWarning("Unknown email provider: {Provider}. Email not sent.", _provider);
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken, string callbackUrl, CancellationToken cancellationToken = default)
    {
        var template = await _templateEngine.GetTemplateAsync("PasswordReset", cancellationToken);

        var variables = new Dictionary<string, string>
        {
            { "appName", _fromName },
            { "resetLink", callbackUrl },
            { "expiryTime", "2 hours" },
            { "year", DateTime.UtcNow.Year.ToString() }
        };

        var htmlContent = await _templateEngine.RenderTemplateAsync("PasswordReset", variables, cancellationToken);
        var textContent = await _templateEngine.RenderTemplateAsync("PasswordReset.txt", variables, cancellationToken);

        await SendEmailAsync(email, template.Subject, htmlContent, textContent, cancellationToken);
    }

    // ... rest of implementation (SendViaMailpitAsync, SendViaSendGridAsync) remains the same
}
```

### Phase 4: Dependency Injection

#### Step 4.1: Register TemplateEngine
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/DependencyInjection.cs`

```csharp
services.AddSingleton<ITemplateEngine, TemplateEngine>();
```

### Phase 5: Copy Templates to Output

#### Step 5.1: Update .csproj
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/MyPhotoBooth.Infrastructure.csproj`

```xml
<ItemGroup>
  <Content Include="Email\Templates\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### Phase 6: Configuration

#### Step 6.1: Add TemplatesPath to Configuration
**File:** `appsettings.Development.json`

```json
{
  "EmailSettings": {
    "Provider": "Mailpit",
    "FromEmail": "noreply@photobooth.local",
    "FromName": "MyPhotoBooth",
    "MailpitUrl": "http://localhost:8025",
    "TemplatesPath": "Email/Templates"
  }
}
```

### Phase 7: Testing

#### Step 7.1: Create Unit Tests
**File:** `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/tests/MyPhotoBooth.Infrastructure.Tests/Email/TemplateEngineTests.cs`

```csharp
using Xunit;
using MyPhotoBooth.Infrastructure.Email;

public class TemplateEngineTests
{
    [Fact]
    public async Task RenderTemplateAsync_ReplacesVariables()
    {
        // Arrange
        var engine = new TemplateEngine(logger, configuration);
        var variables = new Dictionary<string, string>
        {
            { "appName", "TestApp" },
            { "resetLink", "http://example.com/reset" }
        };

        // Act
        var result = await engine.RenderTemplateAsync("PasswordReset", variables);

        // Assert
        Assert.Contains("TestApp", result);
        Assert.Contains("http://example.com/reset", result);
    }

    [Fact]
    public async Task RenderTemplateAsync_MissingVariable_LeavesPlaceholder()
    {
        // Test behavior when variable is missing
    }
}
```

### Phase 8: Localization Preparation (Future)

#### Step 8.1: Add Culture Support
Modify template loading to support language-specific templates:

```
Email/Templates/
├── PasswordReset.html
├── PasswordReset.en.html
├── PasswordReset.es.html
├── PasswordReset.vi.html
```

## Files to Create

| File Path | Purpose |
|-----------|---------|
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/Email/TemplateEngine.cs` | Template rendering engine |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/Email/Templates/PasswordReset.metadata.json` | Password reset metadata |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/Email/Templates/PasswordReset.html` | Password reset HTML template |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/Email/Templates/PasswordReset.txt` | Password reset plain text template |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/tests/MyPhotoBooth.Infrastructure.Tests/Email/TemplateEngineTests.cs` | Unit tests for template engine |

## Files to Modify

| File Path | Changes |
|-----------|---------|
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/Email/EmailService.cs` | Use template engine instead of hardcoded strings |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/DependencyInjection.cs` | Register ITemplateEngine |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.Infrastructure/MyPhotoBooth.Infrastructure.csproj` | Copy templates to output directory |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.API/appsettings.Development.json` | Add TemplatesPath configuration |
| `/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/src/MyPhotoBooth.API/appsettings.Production.json` | Add TemplatesPath configuration |

## Potential Pitfalls to Avoid

1. **Template path issues in production:** Use `CopyToOutputDirectory` to ensure templates are deployed
2. **Template cache not invalidating:** Add file watcher for development or cache duration
3. **HTML injection attacks:** Sanitize user-provided variables before rendering
4. **Missing variables:** Decide on strategy (leave placeholder, throw exception, or use default)
5. **Encoding issues:** Ensure templates are saved as UTF-8 with BOM
6. **Line ending differences:** Git might change CRLF to LF - normalize in CI/CD
7. **CSS not working in email clients:** Keep styles inline for maximum compatibility
8. **Subject line not being templateable:** Include subject in metadata.json

## Estimated Effort

| Phase | Tasks | Estimated Time |
|-------|-------|----------------|
| Phase 1 | Template Engine | 2-3 hours |
| Phase 2 | Extract Templates | 1-2 hours |
| Phase 3 | Refactor EmailService | 1 hour |
| Phase 4 | Dependency Injection | 0.5 hours |
| Phase 5 | Copy Templates | 0.5 hours |
| Phase 6 | Configuration | 0.5 hours |
| Phase 7 | Testing | 2-3 hours |
| Phase 8 | Localization Prep | Future (optional) |
| **Total** | | **8-11 hours** |

---

# Implementation Priority & Dependencies

## Recommended Order

1. **Start with Email Templates** (easier, isolated)
   - No UI changes
   - Pure backend work
   - Can be tested independently
   - Lower risk

2. **Then Dark Mode** (more complex, affects everything)
   - Requires touching many UI components
   - Requires visual testing
   - Higher risk of visual regressions

## Concurrent Work Possible

- Email templates and dark mode foundation (Phase 1 of both) can be done in parallel
- Email testing can happen while dark mode UI components are being updated

---

# Testing Strategy

## Dark Mode Testing

1. **Manual Testing Checklist**
   - Test each page in light/dark/system modes
   - Test theme persistence
   - Test system preference changes
   - Visual regression testing (screenshots)

2. **Automated Testing**
   - Add theme state to unit tests
   - Test theme toggle logic
   - Test system preference listener

## Email Template Testing

1. **Unit Tests**
   - Template variable replacement
   - Template loading
   - Missing variable handling

2. **Integration Tests**
   - Email sending with templates
   - Template rendering in production context

3. **Manual Testing**
   - Send test emails to Mailpit
   - Verify in multiple email clients (Gmail, Outlook, Apple Mail)
   - Test plain text fallback

---

# Rollout Strategy

## Phase 1: Email Templates (Canary Release)
1. Deploy to development environment
2. Test password reset flow
3. Deploy to staging
4. Get design approval on email templates
5. Deploy to production

## Phase 2: Dark Mode (Feature Flag)
1. Deploy backend changes (if any)
2. Deploy frontend changes
3. Dark mode is opt-in (default to system/light mode)
4. Monitor for issues
5. Gradual rollout

---

# Success Criteria

## Dark Mode
- [ ] All pages work in light, dark, and system modes
- [ ] Theme preference persists across sessions
- [ ] No FOUC on page load
- [ ] All text meets WCAG AA contrast requirements
- [ ] Smooth transitions between modes
- [ ] Works on mobile, tablet, desktop

## Email Templates
- [ ] Password reset email uses template files
- [ ] Templates are stored separately from code
- [ ] Templates are editable by designers
- [ ] HTML and plain text versions exist
- [ ] Templates work in major email clients
- [ ] Unit tests pass

---

# Future Enhancements

## Dark Mode
1. Custom accent color selection
2. High contrast mode
3. Blue light filter
4. Per-album theme customization
5. Scheduled dark mode (time-based)

## Email Templates
1. Multi-language support
2. Email template preview tool
3. A/B testing for email templates
4. Email analytics tracking
5. Rich text editor for templates

---

# Questions for User

1. **Dark Mode:**
   - Do you want a "high contrast" mode option?
   - Should the lightbox have a separate dark mode setting (always dark)?
   - Any specific accessibility concerns or color blindness considerations?

2. **Email Templates:**
   - Do you want multi-language support now or later?
   - Should we create additional email templates (welcome, share notifications)?
   - Do you want a web-based template editor for designers?

3. **Both Features:**
   - Should we implement email templates first (lower risk)?
   - Do you have a staging environment for testing?
   - Any specific email clients we must support (Gmail, Outlook, Apple Mail)?
