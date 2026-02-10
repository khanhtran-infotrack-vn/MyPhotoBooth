import { useState } from 'react'
import { useAddGroupMember } from '../../hooks/useGroups'

interface Props {
  groupId: string
  onClose: () => void
}

export function AddMemberModal({ groupId, onClose }: Props) {
  const [email, setEmail] = useState('')
  const addMember = useAddGroupMember()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await addMember.mutateAsync({ groupId, email })
      onClose()
    } catch (error) {
      console.error('Failed to add member:', error)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="fixed inset-0 bg-black/50" onClick={onClose} />
      <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md mx-4 animate-scale-in">
        <div className="p-6">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Add Member</h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="email" className="label label-required">Email Address</label>
              <input
                id="email"
                type="email"
                className="input"
                placeholder="friend@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
              <p className="input-helper-text">The user must already have an account</p>
            </div>

            <div className="flex gap-3 pt-4">
              <button type="button" onClick={onClose} className="btn-secondary flex-1">
                Cancel
              </button>
              <button
                type="submit"
                className="btn-primary flex-1"
                disabled={addMember.isPending || !email.trim()}
              >
                {addMember.isPending ? 'Adding...' : 'Add Member'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
