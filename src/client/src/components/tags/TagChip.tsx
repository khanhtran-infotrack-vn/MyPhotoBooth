import type { Tag } from '../../types'

interface TagChipProps {
  tag: Tag
  onRemove?: () => void
  onClick?: () => void
  variant?: 'default' | 'light' | 'dark'
  size?: 'sm' | 'md'
}

export function TagChip({
  tag,
  onRemove,
  onClick,
  variant = 'default',
  size = 'md'
}: TagChipProps) {
  const baseStyles = 'inline-flex items-center gap-1.5 rounded-full font-medium transition-all duration-200'

  const variantStyles = {
    default: 'bg-primary-100 text-primary-700 hover:bg-primary-200 border border-primary-200',
    light: 'bg-white/90 backdrop-blur-sm text-gray-700 hover:bg-white border border-gray-200 shadow-sm',
    dark: 'bg-gray-800 text-gray-200 hover:bg-gray-700 border border-gray-700'
  }

  const sizeStyles = {
    sm: 'px-2 py-0.5 text-xs',
    md: 'px-3 py-1 text-sm'
  }

  const handleClick = (e: React.MouseEvent) => {
    e.stopPropagation()
    onClick?.()
  }

  const handleRemove = (e: React.MouseEvent) => {
    e.stopPropagation()
    onRemove?.()
  }

  return (
    <span
      className={`${baseStyles} ${variantStyles[variant]} ${sizeStyles[size]} ${
        onClick ? 'cursor-pointer' : ''
      }`}
      onClick={handleClick}
    >
      <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A2 2 0 013 12V7a4 4 0 014-4z" />
      </svg>
      <span className="truncate max-w-[100px]">{tag.name}</span>
      {onRemove && (
        <button
          type="button"
          onClick={handleRemove}
          className="p-0.5 rounded-full hover:bg-black/10 dark:hover:bg-white/10 transition-colors"
          aria-label={`Remove ${tag.name} tag`}
        >
          <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      )}
    </span>
  )
}
