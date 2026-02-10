import { useState } from 'react'
import { useCreateGroup } from '../../hooks/useGroups'

interface Props {
  onClose: () => void
}

export function CreateGroupModal({ onClose }: Props) {
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const createGroup = useCreateGroup()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await createGroup.mutateAsync({ name, description: description || undefined })
      onClose()
    } catch (error) {
      console.error('Failed to create group:', error)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="fixed inset-0 bg-black/50" onClick={onClose} />
      <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md mx-4 animate-scale-in">
        <div className="p-6">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Create New Group</h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="name" className="label label-required">Group Name</label>
              <input
                id="name"
                type="text"
                className="input"
                placeholder="Family, Friends, etc."
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                maxLength={200}
              />
            </div>

            <div>
              <label htmlFor="description" className="label">Description (optional)</label>
              <textarea
                id="description"
                className="textarea"
                placeholder="What's this group for?"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={3}
                maxLength={1000}
              />
            </div>

            <div className="flex gap-3 pt-4">
              <button type="button" onClick={onClose} className="btn-secondary flex-1">
                Cancel
              </button>
              <button
                type="submit"
                className="btn-primary flex-1"
                disabled={createGroup.isPending || !name.trim()}
              >
                {createGroup.isPending ? 'Creating...' : 'Create Group'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
