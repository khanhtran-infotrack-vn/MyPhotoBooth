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
    <div className="p-6">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">Tags</h1>
        <p className="text-sm text-gray-500 mt-1">
          {tags?.length || 0} {tags?.length === 1 ? 'tag' : 'tags'}
        </p>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex items-center justify-center py-20">
          <div className="flex items-center gap-3 text-gray-500">
            <div className="w-6 h-6 border-2 border-gray-300 border-t-primary-600 rounded-full animate-spin" />
            <span>Loading tags...</span>
          </div>
        </div>
      )}

      {/* Empty state */}
      {!isLoading && tags?.length === 0 && (
        <div className="flex flex-col items-center justify-center py-20 text-gray-500">
          <svg className="w-20 h-20 mb-4 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
              d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A2 2 0 013 12V7a4 4 0 014-4z"
            />
          </svg>
          <h3 className="text-xl font-semibold text-gray-700">No tags yet</h3>
          <p className="mt-2">Tags are created when you add them to photos</p>
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
