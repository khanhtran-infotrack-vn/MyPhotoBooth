import { useSelectionStore } from '../../stores/selectionStore'

interface DateGroupHeaderProps {
  date: string
  photoCount: number
  photoIds: string[]
}

export function DateGroupHeader({ date, photoCount, photoIds }: DateGroupHeaderProps) {
  const { isSelectionMode, selectedIds, selectMultiple, deselectMultiple } = useSelectionStore()

  const allSelected = photoIds.every((id) => selectedIds.has(id))
  const someSelected = photoIds.some((id) => selectedIds.has(id))

  const handleSelectAll = () => {
    if (allSelected) {
      deselectMultiple(photoIds)
    } else {
      selectMultiple(photoIds)
    }
  }

  return (
    <div className="sticky top-0 z-10 flex items-center gap-3 py-2 px-1 bg-gray-50/95 backdrop-blur-sm">
      {isSelectionMode && (
        <button
          onClick={handleSelectAll}
          className={`w-5 h-5 rounded border-2 flex items-center justify-center transition-colors ${
            allSelected
              ? 'bg-primary-600 border-primary-600 text-white'
              : someSelected
                ? 'bg-primary-100 border-primary-600'
                : 'border-gray-400 hover:border-primary-600'
          }`}
        >
          {(allSelected || someSelected) && (
            <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={3}
                d={allSelected ? 'M5 13l4 4L19 7' : 'M20 12H4'}
              />
            </svg>
          )}
        </button>
      )}
      <h2 className="text-sm font-semibold text-gray-700">{date}</h2>
      <span className="text-xs text-gray-400">
        {photoCount} {photoCount === 1 ? 'photo' : 'photos'}
      </span>
    </div>
  )
}
