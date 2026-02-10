import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useGroup, useGroupMembers, useLeaveGroup, useRemoveGroupMember, type Group } from '../../hooks/useGroups'
import { AddMemberModal } from './AddMemberModal'

export default function GroupDetail() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [showAddMember, setShowAddMember] = useState(false)
  const { data: group, isLoading } = useGroup(id || null)
  const { data: members } = useGroupMembers(id || null)
  const leaveGroup = useLeaveGroup()
  const removeMember = useRemoveGroupMember()

  const handleLeave = async () => {
    if (!id || !confirm('Are you sure you want to leave this group?')) return
    try {
      await leaveGroup.mutateAsync(id)
      navigate('/groups')
    } catch (error) {
      console.error('Failed to leave group:', error)
    }
  }

  const handleRemoveMember = async (memberUserId: string) => {
    if (!id || !confirm('Remove this member from the group?')) return
    try {
      await removeMember.mutateAsync({ groupId: id, memberUserId })
    } catch (error) {
      console.error('Failed to remove member:', error)
    }
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="flex flex-col items-center gap-4 text-gray-600">
          <div className="w-12 h-12 border-3 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
          <span className="text-lg font-medium">Loading group...</span>
        </div>
      </div>
    )
  }

  if (!group) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Group not found</h2>
          <button onClick={() => navigate('/groups')} className="btn-primary">
            Back to Groups
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="p-6 max-w-6xl mx-auto">
      {/* Header */}
      <div className="mb-8">
        <button onClick={() => navigate('/groups')} className="text-gray-600 hover:text-gray-900 mb-4 flex items-center gap-2">
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
          Back to Groups
        </button>
        <div className="flex items-start justify-between">
          <div>
            <h1 className="text-3xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent">
              {group.name}
            </h1>
            {group.description && (
              <p className="text-gray-600 mt-2">{group.description}</p>
            )}
            <div className="flex items-center gap-4 mt-3 text-sm text-gray-500">
              <span>{group.memberCount} members</span>
              <span>Created {new Date(group.createdAt).toLocaleDateString()}</span>
              {group.isDeletionScheduled && (
                <span className="text-red-600">
                  Deleting in {group.daysUntilDeletion} days
                </span>
              )}
            </div>
          </div>
          <div className="flex gap-2">
            {group.isOwner ? (
              <button
                onClick={() => setShowAddMember(true)}
                className="btn-primary"
              >
                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                Add Member
              </button>
            ) : (
              <button
                onClick={handleLeave}
                className="btn-secondary text-red-600 hover:bg-red-50"
                disabled={leaveGroup.isPending}
              >
                {leaveGroup.isPending ? 'Leaving...' : 'Leave Group'}
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Members Section */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Members</h2>
        {!members || members.length === 0 ? (
          <p className="text-gray-500">No members yet</p>
        ) : (
          <div className="space-y-3">
            {members.map((member) => (
              <MemberItem
                key={member.id}
                member={member}
                isOwner={group.isOwner}
                groupOwnerId={group.ownerId}
                onRemove={() => handleRemoveMember(member.userId)}
              />
            ))}
          </div>
        )}
      </div>

      {/* Add member modal */}
      {showAddMember && (
        <AddMemberModal groupId={group.id} onClose={() => setShowAddMember(false)} />
      )}
    </div>
  )
}

function MemberItem({
  member,
  isOwner,
  groupOwnerId,
  onRemove
}: {
  member: {
    id: string
    userId: string
    email: string | null
    joinedAt: string
    isActive: boolean
    isInGracePeriod: boolean
  }
  isOwner: boolean
  groupOwnerId: string
  onRemove: () => void
}) {
  const [menuOpen, setMenuOpen] = useState(false)

  const canRemove = isOwner && member.userId !== groupOwnerId

  return (
    <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary-400 to-primary-600 flex items-center justify-center text-white font-semibold">
          {member.email?.charAt(0).toUpperCase() || '?'}
        </div>
        <div>
          <p className="font-medium text-gray-900">{member.email || 'Unknown'}</p>
          <p className="text-sm text-gray-500">
            Joined {new Date(member.joinedAt).toLocaleDateString()}
            {member.userId === groupOwnerId && ' • Owner'}
          </p>
        </div>
      </div>
      {canRemove && (
        <div className="relative">
          <button
            onClick={() => setMenuOpen(!menuOpen)}
            className="p-2 hover:bg-gray-200 rounded-lg transition-colors"
          >
            <svg className="w-5 h-5 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
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
                  onClick={() => {
                    setMenuOpen(false)
                    onRemove()
                  }}
                  className="w-full px-4 py-2 text-left text-sm text-red-600 hover:bg-red-50"
                >
                  Remove member
                </button>
              </div>
            </>
          )}
        </div>
      )}
    </div>
  )
}
