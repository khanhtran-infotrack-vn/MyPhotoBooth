import { useState, useRef, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useUIStore } from '../../stores/uiStore'
import { useAuthStore } from '../../stores/authStore'

interface TopBarProps {
 onUploadClick?: () => void
}

export function TopBar({ onUploadClick }: TopBarProps) {
 const navigate = useNavigate()
 const { setSidebarOpen } = useUIStore()
 const { user, logout } = useAuthStore()
 const [userMenuOpen, setUserMenuOpen] = useState(false)
 const [searchQuery, setSearchQuery] = useState('')
 const menuRef = useRef<HTMLDivElement>(null)

 const handleLogout = async () => {
  await logout()
  navigate('/login')
 }

 // Close menu when clicking outside
 useEffect(() => {
  function handleClickOutside(event: MouseEvent) {
   if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
    setUserMenuOpen(false)
   }
  }
  document.addEventListener('mousedown', handleClickOutside)
  return () => document.removeEventListener('mousedown', handleClickOutside)
 }, [])

 return (
  <header className="sticky top-0 z-30 glass-strong border-b border-gray-200/50">
   <div className="flex items-center gap-4 h-16 px-4">
    {/* Mobile menu button */}
    <button
     onClick={() => setSidebarOpen(true)}
     className="btn-icon lg:hidden"
     aria-label="Open menu"
    >
     <svg className="w-6 h-6 text-gray-700" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
     </svg>
    </button>

    {/* Search bar */}
    <div className="flex-1 max-w-2xl">
     <div className="relative group">
      <svg
       className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 transition-colors group-focus-within:text-primary-500"
       fill="none"
       viewBox="0 0 24 24"
       stroke="currentColor"
      >
       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
        d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
       />
      </svg>
      <input
       type="text"
       placeholder="Search your photos"
       value={searchQuery}
       onChange={(e) => setSearchQuery(e.target.value)}
       className="w-full pl-10 pr-4 py-2.5 bg-gray-100/80 border border-gray-200/50 rounded-full
            placeholder-gray-500
            focus:bg-white
            focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20
            text-gray-900 transition-all duration-200"
      />
     </div>
    </div>

    {/* Actions */}
    <div className="flex items-center gap-2">
     {/* Upload button */}
     <button
      onClick={onUploadClick}
      className="btn-primary"
     >
      <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
        d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"
       />
      </svg>
      <span className="hidden sm:inline">Upload</span>
     </button>

     {/* User menu */}
     <div className="relative" ref={menuRef}>
      <button
       onClick={() => setUserMenuOpen(!userMenuOpen)}
       className="w-10 h-10 rounded-full bg-gradient-to-br from-primary-100 to-primary-200 flex items-center justify-center
            text-primary-600 font-medium shadow-sm hover:shadow-md transition-all duration-200 hover:scale-105"
      >
       {(user?.displayName || user?.email || 'U')[0].toUpperCase()}
      </button>

      {userMenuOpen && (
       <div className="absolute right-0 mt-2 w-64 glass-strong rounded-2xl shadow-2xl border border-gray-200/50 py-2 animate-scale-in overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 bg-gradient-to-r from-gray-50 to-white">
         <p className="text-sm font-semibold text-gray-900">
          {user?.displayName || 'User'}
         </p>
         <p className="text-sm text-gray-500 truncate">{user?.email}</p>
        </div>
        <button
         onClick={handleLogout}
         className="w-full px-4 py-2.5 text-left text-sm text-gray-700 hover:bg-gray-100 flex items-center gap-3 transition-colors"
        >
         <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
           d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"
          />
         </svg>
         Sign out
        </button>
       </div>
      )}
     </div>
    </div>
   </div>
  </header>
 )
}
