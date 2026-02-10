import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface LastSelection {
  ids: string[]
  action: string
  timestamp: number
}

interface SelectionState {
  isSelectionMode: boolean
  selectedIds: Set<string>
  lastSelection: LastSelection | null
  enterSelectionMode: () => void
  exitSelectionMode: () => void
  toggleSelection: (id: string) => void
  selectMultiple: (ids: string[]) => void
  deselectMultiple: (ids: string[]) => void
  selectAll: (ids: string[]) => void
  clearSelection: () => void
  saveLastAction: (action: string) => void
  undoLastAction: () => { ids: string[]; canUndo: boolean }
  canUndo: () => boolean
}

const UNDO_DURATION = 5 * 60 * 1000 // 5 minutes

// Helper to convert Set to Array for storage
function setToArray<T>(set: Set<T>): T[] {
  return Array.from(set)
}

// Helper to convert Array to Set
function arrayToSet<T>(arr: T[]): Set<T> {
  return new Set(arr)
}

export const useSelectionStore = create<SelectionState>()(
  persist(
    (set, get) => ({
      isSelectionMode: false,
      selectedIds: new Set(),
      lastSelection: null,

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
          // Set selection mode to true when any item is selected, false when empty
          return {
            selectedIds: newSet,
            isSelectionMode: newSet.size > 0,
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
          // Exit selection mode if no items remain selected
          return { selectedIds: newSet, isSelectionMode: newSet.size > 0 }
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

      saveLastAction: (action) =>
        set((state) => ({
          lastSelection: {
            ids: setToArray(state.selectedIds),
            action,
            timestamp: Date.now()
          }
        })),

      undoLastAction: () => {
        const state = get()
        const last = state.lastSelection

        if (!last) {
          return { ids: [], canUndo: false }
        }

        // Check if within 5 minutes
        const now = Date.now()
        if (now - last.timestamp > UNDO_DURATION) {
          set({ lastSelection: null })
          return { ids: [], canUndo: false }
        }

        set({
          selectedIds: arrayToSet(last.ids),
          isSelectionMode: last.ids.length > 0,
          lastSelection: null
        })

        return { ids: last.ids, canUndo: true }
      },

      canUndo: () => {
        const state = get()
        if (!state.lastSelection) return false

        const now = Date.now()
        return now - state.lastSelection.timestamp <= UNDO_DURATION
      }
    }),
    {
      name: 'photo-selection-storage',
      partialize: (state) => ({
        lastSelection: state.lastSelection
      }),
      // Custom storage to handle Set serialization
      storage: {
        getItem: (name) => {
          const str = localStorage.getItem(name)
          if (!str) return null
          try {
            const data = JSON.parse(str)
            return data
          } catch {
            return null
          }
        },
        setItem: (name, value) => {
          const valueToStore = { ...value }
          localStorage.setItem(name, JSON.stringify(valueToStore))
        },
        removeItem: (name) => {
          localStorage.removeItem(name)
        }
      }
    }
  )
)
