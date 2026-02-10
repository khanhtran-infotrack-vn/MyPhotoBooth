import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useGroups, useDeleteGroup, type Group } from '../../hooks/useGroups'
import { CreateGroupModal } from './CreateGroupModal'

export default function GroupList() {
  const [showCreateModal, setShowCreateModal] = useState(false)
  const { data: groups, isLoading } = useGroups()
  const deleteGroup = useDeleteGroup()

  const handleDelete = async (group: Group) => {
    if (!confirm(`Delete group "${group.name}"? This will schedule the group for deletion.`)) return
    await deleteGroup.mutateAsync(group.id)
  }

  return (
    <div className="p-6 max-w-[1800px] mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent">Groups</h1>
          <p className="text-sm text-gray-600 mt-2 text-lg">
            {groups?.length || 0} {groups?.length === 1 ? 'group' : 'groups'}
          </p>
        </div>
        <button onClick={() => setShowCreateModal(true)} className="btn-primary">
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          New Group
        </button>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex items-center justify-center py-32">
          <div className="flex flex-col items-center gap-4 text-gray-600">
            <div className="w-12 h-12 border-3 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
            <span className="text-lg font-medium">Loading groups...</span>
          </div>
        </div>
      )}

      {/* Empty state */}
      {!isLoading && groups?.length === 0 && (
        <div className="flex flex-col items-center justify-center py-32 text-gray-600">
          <div className="w-28 h-28 mb-8 rounded-3xl bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center shadow-xl float">
            <svg className="w-14 h-14 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
                d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
              />
            </svg>
          </div>
          <h3 className="text-3xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent mb-3">No groups yet</h3>
          <p className="text-gray-600 text-lg">Create your first group to collaborate with friends and family</p>
          <button onClick={() => setShowCreateModal(true)} className="btn-primary mt-4">
            Create Group
          </button>
        </div>
      )}

      {/* Group grid */}
      {!isLoading && groups && groups.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {groups.map((group) => (
            <GroupCard key={group.id} group={group} onDelete={() => handleDelete(group)} />
          ))}
        </div>
      )}

      {/* Create modal */}
      {showCreateModal && <CreateGroupModal onClose={() => setShowCreateModal(false)} />}
    </div>
  )
}

function GroupCard({ group, onDelete }: { group: Group; onDelete: () => void }) {
  const [menuOpen, setMenuOpen] = useState(false)

  return (
    <div className="group relative bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
      <Link to={`/groups/${group.id}`}>
        {/* Cover image placeholder */}
        <div className="aspect-[4/3] bg-gradient-to-br from-primary-50 to-primary-100 relative">
          <div className="w-full h-full flex items-center justify-center text-primary-300">
            <svg className="w-16 h-16" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
                d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
              />
            </svg>
          </div>
        </div>

        {/* Info */}
        <div className="p-4">
          <div className="flex items-center justify-between">
            <h3 className="font-semibold text-gray-900 truncate">{group.name}</h3>
            {group.isOwner && (
              <span className="text-xs bg-primary-100 text-primary-700 px-2 py-0.5 rounded-full">Owner</span>
            )}
          </div>
          <p className="text-sm text-gray-500 mt-1">
            {group.memberCount} {group.memberCount === 1 ? 'member' : 'members'}
          </p>
          {group.isDeletionScheduled && (
            <p className="text-xs text-red-600 mt-1">
              Deleting in {group.daysUntilDeletion} days
            </p>
          )}
        </div>
      </Link>

      {/* Menu button */}
      {group.isOwner && (
        <div className="absolute top-2 right-2">
          <button
            onClick={(e) => {
              e.preventDefault()
              setMenuOpen(!menuOpen)
            }}
            className="p-2 bg-black/40 hover:bg-black/60 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
          >
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"
              />
            </svg>
          </button>

          {menuOpen && (
            <>
              <div className="fixed inset-0 z-10" onClick={() => setMenuOpen(false)} />
              <div className="absolute right-0 mt-1 w-40 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-20 animate-scale-in">
                <button
                  onClick={(e) => {
                    e.preventDefault()
                    setMenuOpen(false)
                    onDelete()
                  }}
                  className="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-red-50"
                >
                  Delete group
                </button>
              </div>
            </>
          )}
        </div>
      )}
    </div>
  )
}
