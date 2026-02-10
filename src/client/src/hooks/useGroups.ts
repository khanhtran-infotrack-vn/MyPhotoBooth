import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../lib/api'

export interface Group {
  id: string
  name: string
  description: string | null
  ownerId: string
  isOwner: boolean
  memberCount: number
  contentCount: number
  isDeleted: boolean
  isDeletionScheduled: boolean
  daysUntilDeletion: number
  createdAt: string
  updatedAt: string
}

export interface GroupMember {
  id: string
  userId: string
  email: string | null
  joinedAt: string
  isActive: boolean
  isInGracePeriod: boolean
}

export interface GroupContent {
  id: string
  contentType: 'Photo' | 'Album'
  photoId: string | null
  albumId: string | null
  photoName: string | null
  albumName: string | null
  sharedByUserId: string
  sharedAt: string
  isActive: boolean
}

export interface GroupDetails extends Omit<Group, 'memberCount' | 'contentCount'> {
  members: GroupMember[]
  sharedContent: GroupContent[]
}

// List all groups
export function useGroups() {
  return useQuery({
    queryKey: ['groups'],
    queryFn: async () => {
      const { data } = await api.get<Group[]>('/groups')
      return data
    },
  })
}

// Get single group with details
export function useGroup(id: string | null) {
  return useQuery({
    queryKey: ['group', id],
    queryFn: async () => {
      const { data } = await api.get<GroupDetails>(`/groups/${id}`)
      return data
    },
    enabled: !!id,
  })
}

// Get group members
export function useGroupMembers(groupId: string | null) {
  return useQuery({
    queryKey: ['group', groupId, 'members'],
    queryFn: async () => {
      const { data } = await api.get<GroupMember[]>(`/groups/${groupId}/members`)
      return data
    },
    enabled: !!groupId,
  })
}

// Get group photos
export function useGroupPhotos(groupId: string | null) {
  return useQuery({
    queryKey: ['group', groupId, 'photos'],
    queryFn: async () => {
      const { data } = await api.get<GroupContent[]>(`/groups/${groupId}/photos`)
      return data
    },
    enabled: !!groupId,
  })
}

// Get group albums
export function useGroupAlbums(groupId: string | null) {
  return useQuery({
    queryKey: ['group', groupId, 'albums'],
    queryFn: async () => {
      const { data } = await api.get<GroupContent[]>(`/groups/${groupId}/albums`)
      return data
    },
    enabled: !!groupId,
  })
}

// Create group
export function useCreateGroup() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (group: { name: string; description?: string }) => {
      const { data } = await api.post<Group>('/groups', group)
      return data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['groups'] })
    },
  })
}

// Update group
export function useUpdateGroup() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({
      id,
      ...group
    }: {
      id: string
      name: string
      description?: string
    }) => {
      await api.put(`/groups/${id}`, group)
    },
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['group', id] })
      queryClient.invalidateQueries({ queryKey: ['groups'] })
    },
  })
}

// Delete group
export function useDeleteGroup() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/groups/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['groups'] })
    },
  })
}

// Add member to group
export function useAddGroupMember() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ groupId, email }: { groupId: string; email: string }) => {
      const { data } = await api.post<GroupMember>(`/groups/${groupId}/members`, { email })
      return data
    },
    onSuccess: (_, { groupId }) => {
      queryClient.invalidateQueries({ queryKey: ['group', groupId] })
      queryClient.invalidateQueries({ queryKey: ['group', groupId, 'members'] })
      queryClient.invalidateQueries({ queryKey: ['groups'] })
    },
  })
}

// Remove member from group
export function useRemoveGroupMember() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ groupId, memberUserId }: { groupId: string; memberUserId: string }) => {
      await api.delete(`/groups/${groupId}/members/${memberUserId}`)
    },
    onSuccess: (_, { groupId }) => {
      queryClient.invalidateQueries({ queryKey: ['group', groupId] })
      queryClient.invalidateQueries({ queryKey: ['group', groupId, 'members'] })
      queryClient.invalidateQueries({ queryKey: ['groups'] })
    },
  })
}

// Leave group
export function useLeaveGroup() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (groupId: string) => {
      await api.post(`/groups/${groupId}/leave`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['groups'] })
    },
  })
}

// Transfer ownership
export function useTransferOwnership() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ groupId, userId }: { groupId: string; userId: string }) => {
      const { data } = await api.post<GroupMember>(`/groups/${groupId}/transfer-ownership`, { userId })
      return data
    },
    onSuccess: (_, { groupId }) => {
      queryClient.invalidateQueries({ queryKey: ['group', groupId] })
      queryClient.invalidateQueries({ queryKey: ['group', groupId, 'members'] })
      queryClient.invalidateQueries({ queryKey: ['groups'] })
    },
  })
}

// Share photo to group
export function useSharePhotoToGroup() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ groupId, photoId }: { groupId: string; photoId: string }) => {
      await api.post(`/groups/${groupId}/photos`, { photoId })
    },
    onSuccess: (_, { groupId }) => {
      queryClient.invalidateQueries({ queryKey: ['group', groupId] })
      queryClient.invalidateQueries({ queryKey: ['group', groupId, 'photos'] })
    },
  })
}

// Remove photo from group
export function useRemovePhotoFromGroup() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ groupId, photoId }: { groupId: string; photoId: string }) => {
      await api.delete(`/groups/${groupId}/photos/${photoId}`)
    },
    onSuccess: (_, { groupId }) => {
      queryClient.invalidateQueries({ queryKey: ['group', groupId] })
      queryClient.invalidateQueries({ queryKey: ['group', groupId, 'photos'] })
    },
  })
}

// Share album to group
export function useShareAlbumToGroup() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ groupId, albumId }: { groupId: string; albumId: string }) => {
      await api.post(`/groups/${groupId}/albums`, { albumId })
    },
    onSuccess: (_, { groupId }) => {
      queryClient.invalidateQueries({ queryKey: ['group', groupId] })
      queryClient.invalidateQueries({ queryKey: ['group', groupId, 'albums'] })
    },
  })
}

// Remove album from group
export function useRemoveAlbumFromGroup() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ groupId, albumId }: { groupId: string; albumId: string }) => {
      await api.delete(`/groups/${groupId}/albums/${albumId}`)
    },
    onSuccess: (_, { groupId }) => {
      queryClient.invalidateQueries({ queryKey: ['group', groupId] })
      queryClient.invalidateQueries({ queryKey: ['group', groupId, 'albums'] })
    },
  })
}
