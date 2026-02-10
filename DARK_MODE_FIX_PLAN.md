# Dark Mode Fix & Design Enhancement Plan

## Executive Summary

**Bug Root Cause**: Tailwind CSS v4's `@apply` directive with `html.dark` descendant selector doesn't process correctly, causing dark mode styles to never apply despite the `dark` class being added to the `<html>` element.

**Solution Strategy**: Replace `html.dark` selectors with proper `:where(html)` or direct `dark:` utility classes, and ensure proper Tailwind v4 configuration.

---

## Phase 1: Critical Bug Fix (Dark Mode)

### 1.1 Root Cause Analysis

**Current Implementation Issues:**

1. **Selector Mismatch in globals.css (Line 55-65)**
   ```css
   html.dark body {
     @apply bg-dark-bg-primary text-dark-text-primary;
   }
   ```
   - `html.dark` with `@apply` doesn't work in Tailwind v4
   - The styles are never generated/applied

2. **Hardcoded Colors in index.css**
   ```css
   body {
     color: #1a202c;
     background-color: #f7fafc;
   }
   ```
   - These override Tailwind's dark mode classes

3. **Potential Timing Issue**
   - `useTheme` may run before CSS is fully loaded
   - Need to ensure theme applies on initial render

### 1.2 Fix Strategy

#### Step 1: Update `globals.css` - Fix Dark Mode Selectors

**Replace problematic `html.dark` patterns:**

```css
/* BEFORE (BROKEN) */
html.dark body {
  @apply bg-dark-bg-primary text-dark-text-primary;
}

/* AFTER (FIXED) - Use :where for lower specificity or direct dark: utilities */
@layer base {
  body {
    @apply bg-gray-50 text-gray-900 dark:bg-dark-bg-primary dark:text-dark-text-primary;
  }
}
```

**Update all component utilities to use `dark:` prefix:**

```css
/* BEFORE */
.btn-secondary {
  @apply bg-white dark:bg-dark-bg-secondary ...;
}

/* AFTER - Already correct, but verify all use dark: prefix */
```

#### Step 2: Remove Overriding Styles in `index.css`

```css
/* REMOVE these hardcoded colors */
body {
  color: #1a202c;  /* REMOVE */
  background-color: #f7fafc;  /* REMOVE */
}
```

#### Step 3: Enhance `useTheme` Hook

**Add flash prevention and ensure initial render:**

```typescript
useEffect(() => {
  const root = document.documentElement
  const effectiveTheme = getEffectiveTheme()

  // Prevent FOUC - set before first paint
  root.classList.add(effectiveTheme)

  // Also set data-theme for CSS selectors that need it
  root.setAttribute('data-theme', effectiveTheme)

  // Remove opposite class
  const opposite = effectiveTheme === 'dark' ? 'light' : 'dark'
  root.classList.remove(opposite)

  // Listen for system changes...
}, [themeMode, getEffectiveTheme])
```

#### Step 4: Add Tailwind v4 Configuration

**Update `vite.config.ts` to ensure dark mode is configured:**

```typescript
export default defineConfig({
  plugins: [
    react(),
    tailwindcss({
      darkMode: 'class', // Explicitly enable class-based dark mode
    })
  ],
  // ... rest of config
})
```

#### Step 5: Update CSS Custom Properties

**Add proper dark mode variable support in `globals.css`:**

```css
@theme {
  /* Light mode defaults */
  --color-bg-primary: oklch(0.98 0 0);
  --color-bg-secondary: oklch(0.95 0 0);
  --color-text-primary: oklch(0.2 0 0);

  /* Dark mode overrides */
  @media (prefers-color-scheme: dark) {
    --color-bg-primary: oklch(0.15 0 0);
    --color-bg-secondary: oklch(0.2 0 0);
    --color-text-primary: oklch(0.95 0 0);
  }
}

html.dark {
  --color-bg-primary: oklch(0.15 0 0);
  --color-bg-secondary: oklch(0.2 0 0);
  --color-text-primary: oklch(0.95 0 0);
}
```

### 1.3 Verification Checklist

- [ ] Theme toggle switches between light/dark/system
- [ ] Visual difference is immediately apparent
- [ ] All pages respect theme setting
- [ ] Public pages (login, register, shared) support dark mode
- [ ] System preference changes trigger updates
- [ ] localStorage persists theme choice
- [ ] No flash of wrong theme on page load
- [ ] All components using dark mode classes render correctly

---

## Phase 2: Design System Enhancement

### 2.1 Color System

#### Light Mode Palette
```css
/* Primary (Brand Colors) */
--primary-50:  #e8f0fe;
--primary-500: #4285f4;
--primary-600: #1a73e8;
--primary-700: #1967d2;

/* Neutral/Gray Scale */
--gray-50:  #f8fafc;
--gray-100: #f1f5f9;
--gray-200: #e2e8f0;
--gray-300: #cbd5e1;
--gray-400: #94a3b8;
--gray-500: #64748b;
--gray-600: #475569;
--gray-700: #334155;
--gray-800: #1e293b;
--gray-900: #0f172a;

/* Semantic */
--success: #34a853;
--warning: #fbbc04;
--error:   #ea4335;
--info:    #4285f4;
```

#### Dark Mode Palette
```css
/* Backgrounds */
--dark-bg-primary:   #0f172a;  /* Main background */
--dark-bg-secondary: #1e293b;  /* Cards, panels */
--dark-bg-tertiary:  #334155;  /* Hover states */

/* Text */
--dark-text-primary:   #f8fafc;  /* Headings, primary text */
--dark-text-secondary: #94a3b8;  /* Secondary text */
--dark-text-tertiary:  #64748b;  /* Disabled text */

/* Borders */
--dark-border-default: #334155;
--dark-border-muted:   #1e293b;
```

### 2.2 Typography Scale

```css
/* Font Families */
--font-sans: 'Google Sans', 'Roboto', -apple-system, sans-serif;
--font-mono: 'Roboto Mono', 'SF Mono', monospace;

/* Font Sizes */
--text-xs:   0.75rem;   /* 12px */
--text-sm:   0.875rem;  /* 14px */
--text-base: 1rem;      /* 16px */
--text-lg:   1.125rem;  /* 18px */
--text-xl:   1.25rem;   /* 20px */
--text-2xl:  1.5rem;    /* 24px */
--text-3xl:  1.875rem;  /* 30px */
--text-4xl:  2.25rem;   /* 36px */

/* Font Weights */
--font-normal:  400;
--font-medium:  500;
--font-semibold: 600;
--font-bold:    700;
```

### 2.3 Spacing Scale

```css
/* Base unit: 4px */
--spacing-1:  0.25rem;  /* 4px */
--spacing-2:  0.5rem;   /* 8px */
--spacing-3:  0.75rem;  /* 12px */
--spacing-4:  1rem;     /* 16px */
--spacing-5:  1.25rem;  /* 20px */
--spacing-6:  1.5rem;   /* 24px */
--spacing-8:  2rem;     /* 32px */
--spacing-10: 2.5rem;   /* 40px */
--spacing-12: 3rem;     /* 48px */
--spacing-16: 4rem;     /* 64px */
```

### 2.4 Component Standardization

#### Buttons
```css
/* Primary - CTA buttons */
.btn-primary {
  @apply inline-flex items-center justify-center gap-2 px-4 py-2
         text-sm font-semibold rounded-lg
         bg-primary-600 text-white
         hover:bg-primary-700 active:bg-primary-800
         focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-600
         disabled:opacity-50 disabled:cursor-not-allowed
         transition-all duration-200;
}

/* Secondary - Alternative actions */
.btn-secondary {
  @apply inline-flex items-center justify-center gap-2 px-4 py-2
         text-sm font-semibold rounded-lg
         bg-white dark:bg-dark-bg-secondary
         text-gray-700 dark:text-dark-text-primary
         border border-gray-300 dark:border-dark-border-default
         hover:bg-gray-50 dark:hover:bg-dark-bg-tertiary
         focus:outline-none focus-visible:ring-2 focus-visible:ring-gray-500;
}

/* Ghost - Subtle actions */
.btn-ghost {
  @apply inline-flex items-center justify-center gap-2 px-4 py-2
         text-sm font-medium rounded-lg
         text-gray-600 dark:text-dark-text-secondary
         hover:bg-gray-100 dark:hover:bg-dark-bg-tertiary
         active:bg-gray-200 dark:active:bg-dark-bg-tertiary;
}

/* Danger - Destructive actions */
.btn-danger {
  @apply inline-flex items-center justify-center gap-2 px-4 py-2
         text-sm font-semibold rounded-lg
         bg-error text-white
         hover:bg-red-600 active:bg-red-700
         focus:outline-none focus-visible:ring-2 focus-visible:ring-error;
}

/* Icon - Icon-only buttons */
.btn-icon {
  @apply p-2 rounded-full
         text-gray-600 dark:text-dark-text-secondary
         hover:bg-gray-100 dark:hover:bg-dark-bg-tertiary
         active:bg-gray-200 dark:active:bg-dark-bg-tertiary
         transition-colors duration-200;
}
```

#### Form Inputs
```css
.input {
  @apply w-full px-4 py-2.5 text-sm
         bg-white dark:bg-dark-bg-secondary
         border border-gray-300 dark:border-dark-border-default
         rounded-lg
         text-gray-900 dark:text-dark-text-primary
         placeholder-gray-400 dark:placeholder-dark-text-secondary
         focus:outline-none
         focus:border-primary-600 focus:ring-1 focus:ring-primary-600
         disabled:bg-gray-100 dark:disabled:bg-dark-bg-tertiary
         transition-colors duration-200;
}

.input-error {
  @apply border-error focus:border-error focus:ring-error;
}
```

#### Cards
```css
.card {
  @apply bg-white dark:bg-dark-bg-secondary
         rounded-xl shadow-sm
         border border-gray-200 dark:border-dark-border-default;
}

.card-hover {
  @apply card transition-shadow duration-200
         hover:shadow-md;
}
```

### 2.5 Animation Standards

```css
/* Duration */
--duration-fast:   150ms;
--duration-normal: 200ms;
--duration-slow:   300ms;

/* Easing */
--ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
--ease-out:   cubic-bezier(0, 0, 0.2, 1);
--ease-in:    cubic-bezier(0.4, 0, 1, 1);
```

---

## Phase 3: Implementation Files

### Files to Modify

1. **`/src/client/src/styles/globals.css`**
   - Fix `html.dark` selectors
   - Add proper dark mode support
   - Define design system variables

2. **`/src/client/src/index.css`**
   - Remove hardcoded body colors
   - Keep only base reset styles

3. **`/src/client/src/hooks/useTheme.ts`**
   - Add FOUC prevention
   - Ensure proper class management

4. **`/src/client/vite.config.ts`**
   - Add explicit dark mode config

5. **All component files**
   - Verify dark mode classes are correct
   - Add missing dark mode variants where needed

### New Files to Create

1. **`/src/client/src/styles/design-system.css`**
   - Document design tokens
   - Provide reference for developers

2. **`/src/client/DESIGN_SYSTEM.md`**
   - Design system documentation
   - Usage examples

---

## Phase 4: Testing Strategy

### Manual Testing Checklist

#### Theme Toggle
- [ ] Click sun icon → switches to light mode
- [ ] Click moon icon → switches to dark mode
- [ ] Click computer icon → uses system preference
- [ ] Toggle persists after page refresh
- [ ] Toggle persists after browser restart

#### Visual Verification (Light Mode)
- [ ] Background is light (#f8fafc)
- [ ] Text is dark (#0f172a)
- [ ] Buttons have proper contrast
- [ ] Cards have white background
- [ ] Borders are visible

#### Visual Verification (Dark Mode)
- [ ] Background is dark (#0f172a)
- [ ] Text is light (#f8fafc)
- [ ] Buttons maintain proper contrast
- [ ] Cards have darker background (#1e293b)
- [ ] Borders are subtle but visible

#### Component Testing
- [ ] Sidebar - colors in both modes
- [ ] TopBar - colors in both modes
- [ ] Forms - inputs and labels in both modes
- [ ] Modals - proper overlay and content
- [ ] PhotoGrid - images and backgrounds
- [ ] Lightbox - all UI elements
- [ ] Navigation - active/inactive states

#### Page Testing
- [ ] Login page
- [ ] Register page
- [ ] Photo gallery
- [ ] Albums list/detail
- [ ] Tags list/detail
- [ ] Share management
- [ ] Public shared view

#### Edge Cases
- [ ] System preference changes while app is open
- [ ] Rapid theme switching
- [ ] Theme state on page load (no FOUC)
- [ ] localStorage cleared (defaults to system)
- [ ] Print styles (should default to light)

---

## Phase 5: Success Criteria

### Must Have (P0)
1. Dark mode toggle produces visible, immediate change
2. Theme preference persists across sessions
3. All pages support both light and dark modes
4. No visual bugs or broken styles in either mode
5. Accessibility maintained (WCAG AA contrast ratios)

### Should Have (P1)
1. Smooth transitions between modes
2. Consistent design across all components
3. System preference updates reflected immediately
4. Design system documentation complete

### Nice to Have (P2)
1. Theme-aware image handling
2. Print-friendly styles
3. Reduced motion support
4. Custom theme options

---

## Implementation Order

1. **Fix dark mode bug first** (Phase 1)
   - Update globals.css
   - Fix useTheme hook
   - Test core functionality

2. **Enhance design system** (Phase 2)
   - Define tokens
   - Standardize components
   - Update all files

3. **Test thoroughly** (Phase 4)
   - Manual testing
   - Visual regression
   - Accessibility audit

4. **Document** (Phase 3 + 5)
   - Create design system docs
   - Update README
   - Add usage examples

---

## Estimated Timeline

- Phase 1 (Bug Fix): 1-2 hours
- Phase 2 (Design System): 3-4 hours
- Phase 3 (Implementation): 4-6 hours
- Phase 4 (Testing): 2-3 hours
- **Total: 10-15 hours**

---

## Open Questions

1. Should we use CSS custom properties for all colors (better for dynamic theming)?
2. Do we need to support custom accent colors in the future?
3. Should we implement automatic image inversion for dark mode?
4. Do we need a high-contrast mode for accessibility?
