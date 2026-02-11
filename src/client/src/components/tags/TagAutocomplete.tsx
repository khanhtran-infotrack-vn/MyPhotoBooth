import { useState, useRef, useEffect } from 'react'
import { useSearchTags, useCreateTag, type Tag } from '../../hooks/useTags'
import { useDebounce } from '../../hooks/useDebounce'
import { TagChip } from './TagChip'

interface TagAutocompleteProps {
  selectedTags: Tag[]
  onTagAdd: (tag: Tag) => void
  onTagRemove: (tagId: string) => void
  excludedTagIds?: string[]
  placeholder?: string
  autoFocus?: boolean
}

export function TagAutocomplete({
  selectedTags,
  onTagAdd,
  onTagRemove,
  excludedTagIds = [],
  placeholder = 'Add a tag...',
  autoFocus = false
}: TagAutocompleteProps) {
  const [inputValue, setInputValue] = useState('')
  const [showDropdown, setShowDropdown] = useState(false)
  const [selectedIndex, setSelectedIndex] = useState(-1)
  const inputRef = useRef<HTMLInputElement>(null)
  const dropdownRef = useRef<HTMLDivElement>(null)

  const debouncedInput = useDebounce(inputValue, 300)
  const { data: searchResults = [], isLoading } = useSearchTags(debouncedInput)
  const createTag = useCreateTag()

  const availableTags = searchResults.filter(
    tag => !excludedTagIds.includes(tag.id) && !selectedTags.some(st => st.id === tag.id)
  )

  const showCreateOption = inputValue.length > 0 &&
    inputValue.length <= 50 &&
    !availableTags.some(t => t.name.toLowerCase() === inputValue.toLowerCase()) &&
    !selectedTags.some(t => t.name.toLowerCase() === inputValue.toLowerCase())

  const handleCreateNew = async () => {
    if (!inputValue.trim()) return

    try {
      const newTag = await createTag.mutateAsync(inputValue.trim())
      onTagAdd(newTag)
      setInputValue('')
      setShowDropdown(false)
      inputRef.current?.focus()
    } catch (error) {
      console.error('Failed to create tag:', error)
    }
  }

  const handleSelectTag = (tag: Tag) => {
    onTagAdd(tag)
    setInputValue('')
    setShowDropdown(false)
    inputRef.current?.focus()
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    const items = [...availableTags, ...(showCreateOption ? ['create-new'] : [])]

    if (!showDropdown) {
      if (e.key === 'ArrowDown' && inputValue.length > 0) {
        e.preventDefault()
        setShowDropdown(true)
      }
      return
    }

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault()
        setSelectedIndex(prev => (prev < items.length - 1 ? prev + 1 : prev))
        break
      case 'ArrowUp':
        e.preventDefault()
        setSelectedIndex(prev => (prev > 0 ? prev - 1 : 0))
        break
      case 'Enter':
        e.preventDefault()
        if (selectedIndex >= 0) {
          if (items[selectedIndex] === 'create-new') {
            handleCreateNew()
          } else {
            handleSelectTag(items[selectedIndex] as Tag)
          }
        } else if (showCreateOption) {
          handleCreateNew()
        }
        break
      case 'Escape':
        e.preventDefault()
        setShowDropdown(false)
        setSelectedIndex(-1)
        break
    }
  }

  useEffect(() => {
    if (autoFocus) {
      inputRef.current?.focus()
    }
  }, [autoFocus])

  useEffect(() => {
    setSelectedIndex(-1)
  }, [availableTags, showCreateOption])

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(e.target as Node) &&
        !inputRef.current?.contains(e.target as Node)
      ) {
        setShowDropdown(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  return (
    <div className="relative" ref={dropdownRef}>
      <div className="flex flex-wrap gap-2 mb-2">
        {selectedTags.map(tag => (
          <TagChip
            key={tag.id}
            tag={tag}
            onRemove={() => onTagRemove(tag.id)}
            variant="light"
            size="md"
          />
        ))}
      </div>

      <div className="relative">
        <input
          ref={inputRef}
          type="text"
          value={inputValue}
          onChange={e => {
            setInputValue(e.target.value)
            if (e.target.value.length > 0) {
              setShowDropdown(true)
            }
          }}
          onFocus={() => {
            if (inputValue.length > 0) {
              setShowDropdown(true)
            }
          }}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          aria-expanded={showDropdown}
          aria-haspopup="listbox"
          role="combobox"
        />

        {isLoading && inputValue.length > 0 && (
          <div className="absolute right-3 top-1/2 -translate-y-1/2">
            <div className="w-4 h-4 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
          </div>
        )}
      </div>

      {showDropdown && (availableTags.length > 0 || showCreateOption) && (
        <div
          className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg max-h-60 overflow-y-auto"
          role="listbox"
        >
          {availableTags.map((tag, index) => (
            <button
              key={tag.id}
              type="button"
              onClick={() => handleSelectTag(tag)}
              className={`w-full px-3 py-2 text-left hover:bg-gray-100 transition-colors ${
                index === selectedIndex ? 'bg-gray-100' : ''
              }`}
              role="option"
              aria-selected={index === selectedIndex}
            >
              <div className="flex items-center gap-2">
                <svg className="w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A2 2 0 013 12V7a4 4 0 014-4z" />
                </svg>
                <span className="truncate">{tag.name}</span>
              </div>
            </button>
          ))}

          {showCreateOption && (
            <button
              type="button"
              onClick={handleCreateNew}
              className={`w-full px-3 py-2 text-left hover:bg-gray-100 transition-colors border-t border-gray-200 ${
                availableTags.length === selectedIndex ? 'bg-gray-100' : ''
              }`}
              role="option"
            >
              <div className="flex items-center gap-2">
                <svg className="w-4 h-4 text-primary-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                <span className="text-primary-600 font-medium">Create "{inputValue.trim()}"</span>
              </div>
            </button>
          )}
        </div>
      )}

      {showDropdown && !isLoading && availableTags.length === 0 && !showCreateOption && inputValue.length > 0 && (
        <div className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg p-3 text-center text-gray-500 text-sm">
          No tags found
        </div>
      )}
    </div>
  )
}
