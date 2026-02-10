import { NavLink, useNavigate } from 'react-router-dom'
import { useUIStore } from '../../stores/uiStore'
import { useAuthStore } from '../../stores/authStore'

const navItems = [
 {
  to: '/photos',
  label: 'Photos',
  icon: (
   <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
     d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
    />
   </svg>
  ),
 },
 {
  to: '/albums',
  label: 'Albums',
  icon: (
   <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
     d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"
    />
   </svg>
  ),
 },
 {
  to: '/tags',
  label: 'Tags',
  icon: (
   <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
     d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A2 2 0 013 12V7a4 4 0 014-4z"
    />
   </svg>
  ),
 },
 {
  to: '/groups',
  label: 'Groups',
  icon: (
   <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
     d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
    />
   </svg>
  ),
 },
 {
  to: '/shares',
  label: 'Shared Links',
  icon: (
   <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
     d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1"
    />
   </svg>
  ),
 },
]

export function Sidebar() {
 const navigate = useNavigate()
 const { sidebarCollapsed, toggleSidebarCollapsed, sidebarOpen, setSidebarOpen } = useUIStore()
 const { user, logout } = useAuthStore()

 const handleLogout = async () => {
  await logout()
  navigate('/login')
 }

 const sidebarContent = (
  <div className="flex flex-col h-full">
   {/* Logo / Brand */}
   <div className="flex items-center gap-3 px-4 py-5 border-b border-gray-200/50">
    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary-600 to-purple-600 flex items-center justify-center shadow-lg">
     <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
       d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z"
      />
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
       d="M15 13a3 3 0 11-6 0 3 3 0 016 0z"
      />
     </svg>
    </div>
    {!sidebarCollapsed && (
     <span className="text-lg font-semibold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent">MyPhotoBooth</span>
    )}
   </div>

   {/* Navigation */}
   <nav className="flex-1 px-3 py-4 space-y-1">
    {navItems.map((item) => (
     <NavLink
      key={item.to}
      to={item.to}
      onClick={() => setSidebarOpen(false)}
      className={({ isActive }) =>
       `nav-item group ${isActive ? 'nav-item-active' : ''} ${sidebarCollapsed ? 'justify-center px-0' : ''}`
      }
     >
      <span className={`transition-colors duration-200 ${sidebarCollapsed ? '' : 'group-hover:text-primary-600'}`}>
       {item.icon}
      </span>
      {!sidebarCollapsed && <span className="group-hover:translate-x-0.5 transition-transform duration-200">{item.label}</span>}
     </NavLink>
    ))}
   </nav>

   {/* Collapse toggle (desktop only) */}
   <div className="hidden lg:block px-3 py-2 border-t border-gray-200/50">
    <button
     onClick={toggleSidebarCollapsed}
     className="nav-item w-full group"
     title={sidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
    >
     <svg
      className={`w-6 h-6 transition-transform duration-300 ${sidebarCollapsed ? 'rotate-180' : ''}`}
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
     >
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
       d="M11 19l-7-7 7-7m8 14l-7-7 7-7"
      />
     </svg>
     {!sidebarCollapsed && <span className="group-hover:translate-x-0.5 transition-transform duration-200">Collapse</span>}
    </button>
   </div>

   {/* User profile */}
   <div className="px-3 py-4 border-t border-gray-200/50">
    <div className={`flex items-center gap-3 ${sidebarCollapsed ? 'justify-center' : ''}`}>
     <div className="w-10 h-10 rounded-full bg-gradient-to-br from-gray-100 to-gray-200 flex items-center justify-center flex-shrink-0 shadow-sm">
      <svg className="w-6 h-6 text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
       <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
        d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
       />
      </svg>
     </div>
     {!sidebarCollapsed && (
      <div className="flex-1 min-w-0">
       <p className="text-sm font-medium text-gray-900 truncate">
        {user?.displayName || user?.email || 'User'}
       </p>
       <button
        onClick={handleLogout}
        className="text-sm text-gray-500 hover:text-primary-600 transition-colors hover:underline"
       >
        Sign out
       </button>
      </div>
     )}
    </div>
   </div>
  </div>
 )

 return (
  <>
   {/* Mobile overlay */}
   {sidebarOpen && (
    <div
     className="fixed inset-0 bg-black/50 z-40 lg:hidden"
     onClick={() => setSidebarOpen(false)}
    />
   )}

   {/* Mobile drawer */}
   <aside
    className={`
     fixed inset-y-0 left-0 z-50 w-[280px] bg-white border-r border-gray-200
     transform transition-transform duration-300 ease-in-out lg:hidden
     ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'}
    `}
   >
    {sidebarContent}
   </aside>

   {/* Desktop sidebar */}
   <aside
    className={`
     hidden lg:flex flex-col flex-shrink-0 bg-white border-r border-gray-200
     transition-all duration-300 ease-in-out
     ${sidebarCollapsed ? 'w-[72px]' : 'w-[280px]'}
    `}
   >
    {sidebarContent}
   </aside>
  </>
 )
}
