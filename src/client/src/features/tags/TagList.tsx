import { Link } from 'react-router-dom'
import { useTags, useDeleteTag, type Tag } from '../../hooks/useTags'

export default function TagList() {
 const { data: tags, isLoading } = useTags()
 const deleteTag = useDeleteTag()

 const handleDelete = async (tag: Tag) => {
  if (!confirm(`Delete tag "${tag.name}"? This will remove it from all photos.`)) return
  await deleteTag.mutateAsync(tag.id)
 }

 return (
  <div className="p-6 max-w-[1800px] mx-auto">
   {/* Header */}
   <div className="mb-8">
    <h1 className="text-3xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent">Tags</h1>
    <p className="text-sm text-gray-600 mt-2 text-lg">
     {tags?.length || 0} {tags?.length === 1 ? 'tag' : 'tags'}
    </p>
   </div>

   {/* Loading */}
   {isLoading && (
    <div className="flex items-center justify-center py-32">
     <div className="flex flex-col items-center gap-4 text-gray-600">
      <div className="w-12 h-12 border-3 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
      <span className="text-lg font-medium">Loading tags...</span>
     </div>
    </div>
   )}

   {/* Empty state */}
   {!isLoading && tags?.length === 0 && (
    <div className="flex flex-col items-center justify-center py-32 text-gray-600">
     <div className="w-28 h-28 mb-8 rounded-3xl bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center shadow-xl float">
      <svg className="w-14 h-14 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
        d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A2 2 0 013 12V7a4 4 0 014-4z"
       />
      </svg>
     </div>
     <h3 className="text-3xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent mb-3">No tags yet</h3>
     <p className="text-gray-600 text-lg">Tags are created when you add them to photos</p>
    </div>
   )}

   {/* Tag list */}
   {!isLoading && tags && tags.length > 0 && (
    <div className="flex flex-wrap gap-3">
     {tags.map((tag) => (
      <div
       key={tag.id}
       className="group relative flex items-center gap-2 px-4 py-2 bg-white rounded-full border border-gray-200 hover:border-primary-300 hover:shadow-sm transition-all"
      >
       <Link
        to={`/tags/${tag.id}`}
        className="flex items-center gap-2 text-gray-700 hover:text-primary-600"
       >
        <svg className="w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
         <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
          d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A2 2 0 013 12V7a4 4 0 014-4z"
         />
        </svg>
        <span className="font-medium">{tag.name}</span>
       </Link>
       <button
        onClick={() => handleDelete(tag)}
        className="p-1 rounded-full text-gray-400 hover:text-red-500 hover:bg-red-50 opacity-0 group-hover:opacity-100 transition-all"
        title="Delete tag"
       >
        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
         <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
        </svg>
       </button>
      </div>
     ))}
    </div>
   )}
  </div>
 )
}
