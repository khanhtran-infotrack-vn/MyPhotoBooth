# MyPhotoBooth Design System

A comprehensive design system for MyPhotoBooth, inspired by Google Photos with clean, modern aesthetics and full dark mode support.

## Table of Contents

1. [Overview](#overview)
2. [Color System](#color-system)
3. [Typography](#typography)
4. [Spacing](#spacing)
5. [Components](#components)
6. [Animations](#animations)
7. [Dark Mode](#dark-mode)
8. [Accessibility](#accessibility)

---

## Overview

The MyPhotoBooth design system provides:

- **Consistent visual language** across all components
- **Comprehensive dark mode support** with automatic theme switching
- **Accessibility-first design** with WCAG AA contrast ratios
- **Responsive components** that work on all screen sizes
- **Smooth animations** with reduced motion support

### Design Principles

1. **Clarity over density** - Generous spacing and clear visual hierarchy
2. **Purposeful color** - Color used intentionally to guide users
3. **Smooth interactions** - All transitions feel natural and responsive
4. **Inclusive design** - Accessible to all users regardless of ability

---

## Color System

### Primary Colors (Brand)

Used for primary actions, links, and brand elements.

| Token | Value | Usage |
|-------|-------|-------|
| `--color-primary-50` | #e8f0fe | Light backgrounds |
| `--color-primary-100` | #d2e3fc | Hover states |
| `--color-primary-300` | #8ab4f8 | Disabled states |
| `--color-primary-500` | #4285f4 | Links, info badges |
| `--color-primary-600` | #1a73e8 | Primary buttons, active states |
| `--color-primary-700` | #1967d2 | Button hover |
| `--color-primary-900` | #174ea6 | Dark mode active states |

### Gray Scale (Neutral)

Used for text, borders, and backgrounds.

| Token | Value | Usage |
|-------|-------|-------|
| `--color-gray-50` | #f8fafc | Page background |
| `--color-gray-100` | #f1f5f9 | Card backgrounds, hover |
| `--color-gray-200` | #e2e8f0 | Borders |
| `--color-gray-300` | #cbd5e1 | Dividers |
| `--color-gray-400` | #94a3b8 | Placeholder text |
| `--color-gray-500` | #64748b | Secondary text |
| `--color-gray-700` | #334155 | Headers, labels |
| `--color-gray-900` | #0f172a | Primary text |

### Semantic Colors

Used for status messages and feedback.

| Token | Value | Usage |
|-------|-------|-------|
| `--color-success` | #34a853 | Success messages |
| `--color-success-light` | #b7e1cd | Success backgrounds |
| `--color-warning` | #fbbc04 | Warning messages |
| `--color-warning-light` | #fce8b2 | Warning backgrounds |
| `--color-error` | #ea4335 | Error messages |
| `--color-error-light` | #fad2cf | Error backgrounds |

### Dark Mode Colors (Enhanced)

| Token | Value | Usage |
|-------|-------|-------|
| `--color-dark-bg-primary` | #0a0e1a | Main background - deep blue-black |
| `--color-dark-bg-secondary` | #121829 | Cards, panels - elevated |
| `--color-dark-bg-tertiary` | #1a2236 | Hover states - surface |
| `--color-dark-text-primary` | #f1f5f9 | Primary text - warm white |
| `--color-dark-text-secondary` | #a8b1c7 | Secondary text - muted |
| `--color-dark-border-default` | #2a3447 | Borders - subtle |
| `--color-dark-border-focus` | #60a5fa | Focus - bright blue |

**Dark Mode Gradients:**
- `--color-dark-gradient-primary`: Linear gradient from blue to purple
- `--color-dark-gradient-surface`: Subtle gradient overlay for depth
- `--color-dark-gradient-overlay`: Bottom gradient for text readability

---

## Typography

### Font Families

```css
--font-family: 'Google Sans', 'Roboto', -apple-system, BlinkMacSystemFont, sans-serif;
```

### Font Sizes

| Token | Size | Usage |
|-------|------|-------|
| `text-xs` | 12px | Captions, labels |
| `text-sm` | 14px | Body text, buttons |
| `text-base` | 16px | Default body |
| `text-lg` | 18px | Subheadings |
| `text-xl` | 20px | Small headings |
| `text-2xl` | 24px | Section headings |
| `text-3xl` | 30px | Page headings |
| `text-4xl` | 36px | Hero headings |

### Font Weights

| Weight | Value | Usage |
|--------|-------|-------|
| `font-normal` | 400 | Body text |
| `font-medium` | 500 | Emphasis |
| `font-semibold` | 600 | Headings, buttons |
| `font-bold` | 700 | Strong emphasis |

### Line Heights

| Token | Value | Usage |
|-------|-------|-------|
| `leading-tight` | 1.25 | Headings |
| `leading-normal` | 1.5 | Body text |
| `leading-relaxed` | 1.75 | Relaxed text |

---

## Spacing

Based on a 4px grid system.

| Token | Size | Usage |
|-------|------|-------|
| `spacing-1` | 4px | Tight spacing |
| `spacing-2` | 8px | Small gaps |
| `spacing-3` | 12px | Compact padding |
| `spacing-4` | 16px | Default padding |
| `spacing-5` | 20px | Medium padding |
| `spacing-6` | 24px | Large padding |
| `spacing-8` | 32px | Extra large |
| `spacing-12` | 48px | Section spacing |
| `spacing-16` | 64px | Large sections |

### Border Radius

| Token | Size | Usage |
|-------|------|-------|
| `rounded-lg` | 8px | Cards, buttons |
| `rounded-xl` | 12px | Large cards |
| `rounded-full` | 9999px | Pills, circles |

---

## Components

### Buttons

#### Primary Button

Use for the main call-to-action on a page.

```html
<button class="btn-primary">Save Changes</button>
<button class="btn-primary btn-primary-lg">Large Button</button>
<button class="btn-primary btn-primary-sm">Small Button</button>
```

#### Secondary Button

Use for alternative actions.

```html
<button class="btn-secondary">Cancel</button>
<button class="btn-secondary btn-secondary-lg">Large Button</button>
<button class="btn-secondary btn-secondary-sm">Small Button</button>
```

#### Ghost Button

Use for subtle, tertiary actions.

```html
<button class="btn-ghost">Learn More</button>
<button class="btn-ghost btn-ghost-lg">Large Button</button>
<button class="btn-ghost btn-ghost-sm">Small Button</button>
```

#### Danger Button

Use for destructive actions.

```html
<button class="btn-danger">Delete</button>
<button class="btn-danger btn-danger-lg">Large Button</button>
<button class="btn-danger btn-danger-sm">Small Button</button>
```

#### Icon Button

Use for icon-only actions.

```html
<button class="btn-icon">
  <svg>...</svg>
</button>
<button class="btn-icon btn-icon-sm">
  <svg>...</svg>
</button>
```

### Form Inputs

#### Text Input

```html
<input type="text" class="input" placeholder="Enter text..." />
<input type="text" class="input input-sm" placeholder="Small input" />
<input type="text" class="input input-lg" placeholder="Large input" />
```

#### With Label

```html
<label class="label" for="email">Email Address</label>
<input id="email" type="email" class="input" placeholder="you@example.com" />
<p class="input-helper-text">We'll never share your email.</p>
```

#### Required Field

```html
<label class="label label-required" for="name">Full Name</label>
<input id="name" type="text" class="input" required />
```

#### Error State

```html
<label class="label" for="password">Password</label>
<input id="password" type="password" class="input input-error" />
<p class="input-error-message">Password must be at least 8 characters.</p>
```

#### Textarea

```html
<textarea class="textarea" placeholder="Enter your message..."></textarea>
```

#### Select Dropdown

```html
<select class="select">
  <option>Option 1</option>
  <option>Option 2</option>
</select>
```

#### Checkbox

```html
<label class="inline-flex items-center gap-2">
  <input type="checkbox" class="checkbox" />
  <span>Remember me</span>
</label>
```

#### Radio

```html
<label class="inline-flex items-center gap-2">
  <input type="radio" name="plan" class="radio" />
  <span>Monthly</span>
</label>
```

### Cards (Enhanced)

#### Basic Card

```html
<div class="card p-4">
  <h3>Card Title</h3>
  <p>Card content goes here.</p>
</div>
```

#### Hover Card

```html
<div class="card-hover p-4 cursor-pointer">
  <h3>Clickable Card</h3>
  <p>This card has a hover effect with lift.</p>
</div>
```

#### Interactive Card

```html
<div class="card-interactive p-4">
  <h3>Interactive Card</h3>
  <p>This card is more interactive on hover.</p>
</div>
```

#### Glassmorphism Card

```html
<div class="card-glass p-4">
  <h3>Glass Card</h3>
  <p>Translucent card with blur effect.</p>
</div>
```

#### Gradient Card

```html
<div class="card-gradient p-4">
  <h3>Gradient Card</h3>
  <p>Card with gradient background.</p>
</div>
```

### Badges

```html
<span class="badge badge-primary">New</span>
<span class="badge badge-success">Completed</span>
<span class="badge badge-warning">Pending</span>
<span class="badge badge-error">Error</span>
<span class="badge badge-gray">Archived</span>
```

### Navigation Items

```html
<a href="#" class="nav-item">
  <svg>...</svg>
  <span>Photos</span>
</a>

<a href="#" class="nav-item nav-item-active">
  <svg>...</svg>
  <span>Albums</span>
</a>
```

### Avatar

```html
<div class="avatar avatar-md">JD</div>
<div class="avatar avatar-lg">
  <img src="..." alt="User" class="avatar-img" />
</div>
```

### Modals

```html
<div class="modal-overlay"></div>
<div class="modal-content w-full max-w-md p-6">
  <h2>Modal Title</h2>
  <p>Modal content goes here.</p>
  <div class="mt-4 flex gap-2">
    <button class="btn-primary">Confirm</button>
    <button class="btn-secondary">Cancel</button>
  </div>
</div>
```

---

## Animations

### Duration Tokens

| Token | Duration | Usage |
|-------|----------|-------|
| `--duration-fast` | 150ms | Micro-interactions |
| `--duration-normal` | 200ms | Default transitions |
| `--duration-slow` | 300ms | Complex animations |
| `--duration-slower` | 500ms | Page transitions |

### Easing Functions

| Token | Curve | Usage |
|-------|-------|-------|
| `--ease-out` | cubic-bezier(0, 0, 0.2, 1) | Entering elements |
| `--ease-in` | cubic-bezier(0.4, 0, 1, 1) | Leaving elements |
| `--ease-in-out` | cubic-bezier(0.4, 0, 0.2, 1) | Movement |

### Animation Classes

```html
<!-- Fade in -->
<div class="animate-fade-in">...</div>

<!-- Fade in up -->
<div class="animate-fade-in-up">...</div>

<!-- Slide in -->
<div class="animate-slide-in">...</div>

<!-- Scale in -->
<div class="animate-scale-in">...</div>

<!-- Bounce in -->
<div class="animate-bounce-in">...</div>
```

### Reduced Motion

The design system automatically respects the user's `prefers-reduced-motion` setting. All animations are disabled or simplified when this preference is detected.

---

## Dark Mode

### Implementation

Dark mode is implemented using Tailwind CSS's `dark:` variant with class-based switching. The `dark` class is applied to the `<html>` element.

```css
/* Light mode (default) */
body {
  @apply bg-gray-50 text-gray-900;
}

/* Dark mode */
html.dark body {
  @apply bg-dark-bg-primary text-dark-text-primary;
}
```

### Component Dark Mode

All components include dark mode variants:

```css
.button {
  @apply bg-white text-gray-900;           /* Light mode */
  @apply dark:bg-dark-bg-secondary dark:text-dark-text-primary; /* Dark mode */
}
```

### Dark Mode Toggle

The theme toggle supports three states:

1. **Light** - Forces light mode
2. **Dark** - Forces dark mode
3. **System** - Follows OS preference

```typescript
// Set theme mode
const setThemeMode = (mode: 'light' | 'dark' | 'system') => {
  localStorage.setItem('theme-mode', mode)
  // Theme is applied automatically by useTheme hook
}
```

---

## Accessibility

### Focus Management

All interactive elements have visible focus states:

```css
*:focus-visible {
  @apply outline-2 outline-offset-2 outline-primary-600;
}
```

### Color Contrast

All color combinations meet WCAG AA standards:

- Normal text: 4.5:1 contrast ratio
- Large text: 3:1 contrast ratio
- UI components: 3:1 contrast ratio

### Keyboard Navigation

- All interactive elements are keyboard accessible
- Tab order follows visual layout
- Focus indicators are clearly visible
- Skip links available for main content

### Screen Readers

- Semantic HTML elements used throughout
- ARIA labels provided where needed
- Form inputs have associated labels
- Error messages are announced

---

## Best Practices

### When to Use Components

| Situation | Component |
|-----------|-----------|
| Primary action on page | `btn-primary` |
| Secondary/cancel action | `btn-secondary` |
| Low-emphasis action | `btn-ghost` |
| Destructive action | `btn-danger` |
| Icon-only action | `btn-icon` |

### Spacing Guidelines

- Use consistent spacing multiples of 4px
- Give components room to breathe (8px minimum)
- Use larger spacing for sections (24px+)

### Color Usage

- Reserve primary color for CTAs and links
- Use semantic colors for status only
- Gray scale for borders and backgrounds
- Always provide dark mode alternatives

### Typography

- Use heading hierarchy (h1-h6)
- Keep line length under 80 characters
- Use adequate line height (1.5 for body)
- Avoid using more than 3 font sizes per page

---

## File Structure

```
src/client/src/styles/
├── globals.css          # Main design system file
├── index.css            # Base reset styles
└── design-system.css    # Design token reference (optional)
```

---

## Contributing

When adding new components:

1. Follow existing patterns in `globals.css`
2. Include dark mode variants
3. Add focus states for accessibility
4. Document usage in this file
5. Test in both light and dark modes

---

## Modern Design Features

### Glassmorphism

Glass effect with backdrop blur for modern, layered interfaces:

```html
<div class="glass">Standard glass effect</div>
<div class="glass-strong">Strong glass effect (more opaque)</div>
<div class="glass-subtle">Subtle glass effect (more transparent)</div>
<div class="glass-gradient">Glass with gradient overlay</div>
```

### Micro-interactions

- **Hover lift**: Cards and buttons lift slightly on hover (`-translate-y-0.5`)
- **Scale effect**: Interactive elements scale on hover (`hover:scale-105`)
- **Smooth transitions**: All color changes use 200ms duration
- **Focus rings**: Enhanced focus states with colored rings

### Animation Classes

```html
<!-- Fade effects -->
<div class="animate-fade-in">Fade in</div>
<div class="animate-fade-in-up">Fade in and slide up</div>

<!-- Scale effects -->
<div class="animate-scale-in">Scale in from center</div>

<!-- Special effects -->
<div class="float">Gentle floating animation</div>
<div class="pulse-glow">Pulsing glow effect</div>
<div class="shimmer">Shimmer loading effect</div>
```

### Enhanced Buttons

Primary buttons now feature:
- Gradient backgrounds (blue to purple in dark mode)
- Shadow effects on hover
- Subtle lift animation
- Enhanced focus rings

### Navigation Enhancements

- Active states use gradient backgrounds
- Hover effects with text color transitions
- Smooth transform animations
- Better visual hierarchy

---

## Dark Mode Best Practices

### 1. Background Depth

Use the three-tier background system for visual hierarchy:
- `dark-bg-primary` - Deepest background (page level)
- `dark-bg-secondary` - Elevated surfaces (cards, panels)
- `dark-bg-tertiary` - Interactive elements (hover states)

### 2. Text Contrast

Ensure proper contrast ratios:
- Use `dark-text-primary` for headings and important text
- Use `dark-text-secondary` for body text and descriptions
- Use `dark-text-tertiary` for disabled states

### 3. Border Visibility

Borders should be subtle but visible:
- Use `dark-border-default` for standard borders
- Use white/10 or white/5 for subtle separators
- Avoid pure black borders - use dark blue-gray tones

### 4. Accent Colors

In dark mode, accent colors should be slightly enhanced:
- Primary blue works well but can be warmed
- Purple accents complement blue in dark mode
- Gradients add depth and visual interest

---

## Version

**Current Version:** 3.0.0

**Last Updated:** 2026-02-10

**Changes in v3.0.0:**
- Enhanced dark mode color palette with deeper, richer backgrounds
- Added glassmorphism utility classes
- Improved button gradients and hover effects
- Enhanced modal and navigation styling
- Better focus states and accessibility
- New animation utilities (float, shimmer, pulse-glow)

---

## Resources

- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [Material Design Guidelines](https://material.io/design)
