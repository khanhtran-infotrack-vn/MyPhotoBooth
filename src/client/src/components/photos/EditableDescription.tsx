import { useState, useRef, useEffect } from 'react'

interface EditableDescriptionProps {
  description: string | null
  onSave: (description: string) => Promise<void>
  readOnly?: boolean
}

export function EditableDescription({ description, onSave, readOnly = false }: EditableDescriptionProps) {
  const [isEditing, setIsEditing] = useState(false)
  const [value, setValue] = useState(description || '')
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const textareaRef = useRef<HTMLTextAreaElement>(null)

  useEffect(() => {
    setValue(description || '')
  }, [description])

  const handleStartEdit = () => {
    if (readOnly) return
    setIsEditing(true)
    setError(null)
    setTimeout(() => {
      textareaRef.current?.focus()
      textareaRef.current?.setSelectionRange(textareaRef.current.value.length, textareaRef.current.value.length)
    }, 0)
  }

  const handleCancel = () => {
    setValue(description || '')
    setIsEditing(false)
    setError(null)
  }

  const handleSave = async () => {
    if (isSaving) return

    const newValue = value.trim()
    if (newValue === (description || '')) {
      setIsEditing(false)
      return
    }

    setIsSaving(true)
    setError(null)

    try {
      await onSave(newValue || '')
      setIsEditing(false)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save description')
      setValue(description || '')
    } finally {
      setIsSaving(false)
    }
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') {
      handleCancel()
    } else if (e.key === 'Enter' && (e.metaKey || e.ctrlKey)) {
      e.preventDefault()
      handleSave()
    }
  }

  if (readOnly) {
    return (
      <section>
        <h4 className="text-xs font-semibold text-white/50 uppercase tracking-wider mb-3">
          Description
        </h4>
        {description ? (
          <p className="text-white/80 text-sm whitespace-pre-wrap">{description}</p>
        ) : (
          <p className="text-white/40 text-sm italic">No description</p>
        )}
      </section>
    )
  }

  if (isEditing) {
    return (
      <section>
        <h4 className="text-xs font-semibold text-white/50 uppercase tracking-wider mb-3">
          Description
        </h4>
        <div className="space-y-2">
          <textarea
            ref={textareaRef}
            value={value}
            onChange={e => setValue(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Add a description..."
            className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-white/40 focus:outline-none focus:ring-2 focus:ring-white/30 resize-none"
            rows={4}
            maxLength={1000}
            disabled={isSaving}
          />
          {error && (
            <p className="text-red-400 text-xs">{error}</p>
          )}
          <div className="flex gap-2">
            <button
              onClick={handleSave}
              disabled={isSaving}
              className="flex-1 px-3 py-1.5 bg-primary-600 hover:bg-primary-700 text-white text-sm font-medium rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isSaving ? 'Saving...' : 'Save'}
            </button>
            <button
              onClick={handleCancel}
              disabled={isSaving}
              className="flex-1 px-3 py-1.5 bg-white/10 hover:bg-white/20 text-white text-sm font-medium rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Cancel
            </button>
          </div>
          <p className="text-white/40 text-xs">
            Press <kbd className="px-1.5 py-0.5 bg-white/10 rounded">Cmd+Enter</kbd> to save
          </p>
        </div>
      </section>
    )
  }

  return (
    <section>
      <div className="flex items-center justify-between mb-3">
        <h4 className="text-xs font-semibold text-white/50 uppercase tracking-wider">
          Description
        </h4>
        <button
          onClick={handleStartEdit}
          className="p-1 text-white/40 hover:text-white/70 transition-colors rounded hover:bg-white/10"
          aria-label="Edit description"
        >
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
          </svg>
        </button>
      </div>
      {description ? (
        <p
          onClick={handleStartEdit}
          className="text-white/80 text-sm whitespace-pre-wrap cursor-pointer hover:text-white/90 transition-colors"
        >
          {description}
        </p>
      ) : (
        <button
          onClick={handleStartEdit}
          className="text-white/40 text-sm italic hover:text-white/60 transition-colors"
        >
          + Add description...
        </button>
      )}
    </section>
  )
}
