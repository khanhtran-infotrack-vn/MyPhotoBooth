import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import { PhotoUpload } from '../../features/upload/PhotoUpload'

export function AppShell() {
  const [showUpload, setShowUpload] = useState(false)

  return (
    <div className="flex h-screen bg-gray-50">
      <Sidebar />

      <div className="flex-1 flex flex-col min-w-0">
        <TopBar onUploadClick={() => setShowUpload(true)} />

        <main className="flex-1 overflow-auto">
          <Outlet />
        </main>
      </div>

      {/* Upload Modal */}
      {showUpload && (
        <PhotoUpload onClose={() => setShowUpload(false)} />
      )}
    </div>
  )
}
