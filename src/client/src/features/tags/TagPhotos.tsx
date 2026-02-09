import { Link, useParams } from 'react-router-dom'

export default function TagPhotos() {
  const { id: _tagId } = useParams<{ id: string }>()

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center gap-2 text-sm text-gray-500 mb-2">
          <Link to="/tags" className="hover:text-primary-600">Tags</Link>
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
          </svg>
          <span className="text-gray-700">Tag Photos</span>
        </div>
      </div>

      {/* Note: The backend doesn't have an endpoint to get photos by tag */}
      <div className="flex flex-col items-center justify-center py-20 text-gray-500">
        <svg className="w-20 h-20 mb-4 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
            d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A2 2 0 013 12V7a4 4 0 014-4z"
          />
        </svg>
        <h3 className="text-xl font-semibold text-gray-700">Tagged Photos</h3>
        <p className="mt-2 text-center max-w-md">
          View photos with this tag. This feature requires a backend endpoint to fetch photos by tag ID.
        </p>
        <Link to="/tags" className="btn-secondary mt-4">
          Back to Tags
        </Link>
      </div>
    </div>
  )
}
