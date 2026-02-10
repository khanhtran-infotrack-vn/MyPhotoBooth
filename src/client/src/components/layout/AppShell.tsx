import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import { PhotoUpload } from '../../features/upload/PhotoUpload'

export function AppShell() {
  const [showUpload, setShowUpload] = useState(false)

  return (
    <div className="flex h-screen bg-gradient-to-br from-gray-50 via-blue-50/20 to-purple-50/10 dark:from-dark-bg-primary dark:via-[#0d1220] dark:to-black">
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
