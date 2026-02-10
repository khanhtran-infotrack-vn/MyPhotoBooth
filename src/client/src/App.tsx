import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import Login from './features/auth/Login'
import Register from './features/auth/Register'
import ForgotPassword from './features/auth/ForgotPassword'
import ResetPassword from './features/auth/ResetPassword'
import ProtectedRoute from './components/ProtectedRoute'
import PublicRoute from './components/PublicRoute'
import { AppShell } from './components/layout'
import PhotoGallery from './features/gallery/PhotoGallery'
import { AlbumList, AlbumDetail } from './features/albums'
import { TagList, TagPhotos } from './features/tags'
import { GroupList, GroupDetail } from './features/groups'
import ShareManagement from './features/sharing/ShareManagement'
import { SharedView } from './features/sharing/SharedView'

const queryClient = new QueryClient({
 defaultOptions: {
  queries: {
   staleTime: 1000 * 60 * 5, // 5 minutes
   retry: 1,
  },
 },
})

function App() {
 return (
  <QueryClientProvider client={queryClient}>
   <BrowserRouter>
    <Routes>
     <Route
      path="/login"
      element={
       <PublicRoute>
        <Login />
       </PublicRoute>
      }
     />
     <Route
      path="/register"
      element={
       <PublicRoute>
        <Register />
       </PublicRoute>
      }
     />
     <Route
      path="/forgot-password"
      element={
       <PublicRoute>
        <ForgotPassword />
       </PublicRoute>
      }
     />
     <Route
      path="/reset-password"
      element={
       <PublicRoute>
        <ResetPassword />
       </PublicRoute>
      }
     />
     <Route path="/shared/:token" element={<SharedView />} />
     <Route
      element={
       <ProtectedRoute>
        <AppShell />
       </ProtectedRoute>
      }
     >
      <Route index element={<Navigate to="/photos" replace />} />
      <Route path="photos" element={<PhotoGallery />} />
      <Route path="albums" element={<AlbumList />} />
      <Route path="albums/:id" element={<AlbumDetail />} />
      <Route path="tags" element={<TagList />} />
      <Route path="tags/:id" element={<TagPhotos />} />
      <Route path="groups" element={<GroupList />} />
      <Route path="groups/:id" element={<GroupDetail />} />
      <Route path="shares" element={<ShareManagement />} />
     </Route>
     <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
   </BrowserRouter>
  </QueryClientProvider>
 )
}

export default App
