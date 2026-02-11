import { createContext, useContext, useEffect, type ReactNode } from 'react'
import { useSelectionStore, type SelectionContext as SelectionContextType } from '../stores/selectionStore'

interface SelectionContextProviderProps {
  children: ReactNode
  context: SelectionContextType
}

const SelectionContextValue = createContext<SelectionContextType | null>(null)

export function useCurrentSelectionContext() {
  return useContext(SelectionContextValue)
}

export function SelectionContextProvider({ children, context }: SelectionContextProviderProps) {
  const setContext = useSelectionStore((state) => state.setContext)
  const clearContext = useSelectionStore((state) => state.clearContext)

  useEffect(() => {
    // Set context when component mounts or context changes
    setContext(context)

    // Clear context when component unmounts
    return () => {
      clearContext()
    }
  }, [context, setContext, clearContext])

  return (
    <SelectionContextValue.Provider value={context}>
      {children}
    </SelectionContextValue.Provider>
  )
}
