import { create } from 'zustand'

interface SelectionState {
  isSelectionMode: boolean
  selectedIds: Set<string>
  enterSelectionMode: () => void
  exitSelectionMode: () => void
  toggleSelection: (id: string) => void
  selectMultiple: (ids: string[]) => void
  deselectMultiple: (ids: string[]) => void
  selectAll: (ids: string[]) => void
  clearSelection: () => void
}

export const useSelectionStore = create<SelectionState>((set) => ({
  isSelectionMode: false,
  selectedIds: new Set(),

  enterSelectionMode: () => set({ isSelectionMode: true }),

  exitSelectionMode: () =>
    set({
      isSelectionMode: false,
      selectedIds: new Set(),
    }),

  toggleSelection: (id) =>
    set((state) => {
      const newSet = new Set(state.selectedIds)
      if (newSet.has(id)) {
        newSet.delete(id)
      } else {
        newSet.add(id)
      }
      return {
        selectedIds: newSet,
        isSelectionMode: newSet.size > 0 ? true : state.isSelectionMode,
      }
    }),

  selectMultiple: (ids) =>
    set((state) => {
      const newSet = new Set(state.selectedIds)
      ids.forEach((id) => newSet.add(id))
      return { selectedIds: newSet, isSelectionMode: true }
    }),

  deselectMultiple: (ids) =>
    set((state) => {
      const newSet = new Set(state.selectedIds)
      ids.forEach((id) => newSet.delete(id))
      return { selectedIds: newSet }
    }),

  selectAll: (ids) =>
    set({
      selectedIds: new Set(ids),
      isSelectionMode: true,
    }),

  clearSelection: () =>
    set({
      selectedIds: new Set(),
      isSelectionMode: false,
    }),
}))
