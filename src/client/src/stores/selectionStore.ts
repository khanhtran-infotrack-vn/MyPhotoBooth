import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface LastSelection {
  ids: string[]
  action: string
  timestamp: number
}

export interface SelectionContext {
  view: 'gallery' | 'album' | 'tags' | 'shared'
  filter?: 'all' | 'favorites' | 'recent' | 'search'
  entityId?: string // album ID, tag ID, etc.
}

interface SelectionState {
  // Current selection
  isSelectionMode: boolean
  selectedIds: Set<string>

  // Context awareness
  currentContext: SelectionContext | null
  allowedContexts: Set<string>

  // Undo (keep existing)
  lastSelection: LastSelection | null

  // Actions
  enterSelectionMode: () => void
  exitSelectionMode: () => void
  toggleSelectionMode: () => void
  toggleSelection: (id: string) => void
  selectMultiple: (ids: string[]) => void
  deselectMultiple: (ids: string[]) => void
  selectAll: (ids: string[]) => void
  clearSelection: () => void
  clearAll: () => void // Combined clear + exit

  // Context management
  setContext: (context: SelectionContext) => void
  clearContext: () => void
  isSelectionAllowedInContext: (context: string) => boolean

  // Undo
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

// Generate a context key for comparison
function getContextKey(context: SelectionContext | null): string {
  if (!context) return 'none'
  const { view, filter, entityId } = context
  return `${view}:${filter || 'none'}:${entityId || 'none'}`
}

export const useSelectionStore = create<SelectionState>()(
  persist(
    (set, get) => ({
      isSelectionMode: false,
      selectedIds: new Set(),
      currentContext: null,
      allowedContexts: new Set(['gallery', 'album', 'tags']), // Enable selection in all views
      lastSelection: null,

      enterSelectionMode: () => set({ isSelectionMode: true }),

      exitSelectionMode: () =>
        set({
          isSelectionMode: false,
          // Keep selectedIds - don't clear on exit
        }),

      toggleSelectionMode: () =>
        set((state) => ({
          isSelectionMode: !state.isSelectionMode,
          // Clear selections when exiting selection mode
          selectedIds: state.isSelectionMode ? new Set() : state.selectedIds,
        })),

      toggleSelection: (id) =>
        set((state) => {
          // Only allow selection if in selection mode
          if (!state.isSelectionMode) {
            return state
          }

          const newSet = new Set(state.selectedIds)
          if (newSet.has(id)) {
            newSet.delete(id)
          } else {
            newSet.add(id)
          }
          // Don't auto-enter or auto-exit selection mode
          return {
            selectedIds: newSet,
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
          // Don't auto-exit selection mode - stay in mode
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
          // Don't exit selection mode - stay in mode
        }),

      clearAll: () =>
        set({
          selectedIds: new Set(),
          isSelectionMode: false,
        }),

      setContext: (context) =>
        set((state) => {
          const newContextKey = getContextKey(context)
          const oldContextKey = getContextKey(state.currentContext)

          // If context changed, clear selection and exit mode
          if (newContextKey !== oldContextKey) {
            return {
              currentContext: context,
              selectedIds: new Set(),
              isSelectionMode: false,
            }
          }

          // Same context, just update
          return {
            currentContext: context,
          }
        }),

      clearContext: () =>
        set({
          currentContext: null,
          selectedIds: new Set(),
          isSelectionMode: false,
        }),

      isSelectionAllowedInContext: (context) => {
        const state = get()
        return state.allowedContexts.has(context)
      },

      saveLastAction: (action) =>
        set((state) => ({
          lastSelection: {
            ids: setToArray(state.selectedIds),
            action,
            timestamp: Date.now(),
          },
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
          lastSelection: null,
        })

        return { ids: last.ids, canUndo: true }
      },

      canUndo: () => {
        const state = get()
        if (!state.lastSelection) return false

        const now = Date.now()
        return now - state.lastSelection.timestamp <= UNDO_DURATION
      },
    }),
    {
      name: 'photo-selection-storage',
      partialize: (state) => ({
        lastSelection: state.lastSelection,
        // Don't persist isSelectionMode or selectedIds - should not survive refresh
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
        },
      },
    }
  )
)
