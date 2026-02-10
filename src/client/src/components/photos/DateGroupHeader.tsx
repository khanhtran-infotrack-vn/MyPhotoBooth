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
  <div className="sticky top-0 z-10 flex items-center gap-4 py-3 px-2 -mx-2 bg-gray-50/95 backdrop-blur-md border-b border-gray-200/50">
   {isSelectionMode && (
    <button
     onClick={handleSelectAll}
     className={`w-6 h-6 rounded-lg border-2 flex items-center justify-center transition-all duration-200 shadow-sm ${
      allSelected
       ? 'bg-primary-600 border-primary-600 text-white shadow-primary-600/30'
       : someSelected
        ? 'bg-primary-100 border-primary-600'
        : 'bg-white border-gray-300 hover:border-primary-500'
     }`}
    >
     {(allSelected || someSelected) && (
      <svg className="w-3.5 h-3.5 animate-scale-in" fill="none" viewBox="0 0 24 24" stroke="currentColor">
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
   <div className="flex items-center gap-3">
    <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-primary-500 to-purple-600 flex items-center justify-center shadow-md">
     <svg className="w-4 h-4 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
     </svg>
    </div>
    <div>
     <h2 className="text-base font-bold text-gray-900">{date}</h2>
     <p className="text-xs text-gray-500">
      {photoCount.toLocaleString()} {photoCount === 1 ? 'photo' : 'photos'}
     </p>
    </div>
   </div>
  </div>
 )
}
