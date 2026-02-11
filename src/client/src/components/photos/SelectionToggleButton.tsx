import { useSelectionStore } from '../../stores/selectionStore'

interface SelectionToggleButtonProps {
 disabled?: boolean
 photoCount?: number
}

export function SelectionToggleButton({ disabled = false, photoCount = 0 }: SelectionToggleButtonProps) {
 const { isSelectionMode, toggleSelectionMode } = useSelectionStore()

 return (
  <button
   onClick={toggleSelectionMode}
   disabled={disabled || photoCount === 0}
   className={`flex items-center gap-2 px-4 py-2 rounded-lg border shadow-sm transition-all disabled:opacity-50 disabled:cursor-not-allowed ${
    isSelectionMode
     ? 'bg-primary-50 border-primary-400 hover:bg-primary-100'
     : 'bg-white border-gray-200 hover:shadow-md hover:border-primary-300'
   }`}
   aria-label={isSelectionMode ? 'Exit selection mode' : 'Enter selection mode'}
  >
   {isSelectionMode ? (
    <>
     <svg className="w-5 h-5 text-primary-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
     </svg>
     <span className="text-sm font-medium text-primary-700">Cancel</span>
    </>
   ) : (
    <>
     <svg className="w-5 h-5 text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
     </svg>
     <span className="text-sm font-medium text-gray-700">Select</span>
    </>
   )}
  </button>
 )
}
