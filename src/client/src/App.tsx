import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import Login from './features/auth/Login'
import Register from './features/auth/Register'
import ProtectedRoute from './components/ProtectedRoute'
import { AppShell } from './components/layout'
import PhotoGallery from './features/gallery/PhotoGallery'
import { AlbumList, AlbumDetail } from './features/albums'
import { TagList, TagPhotos } from './features/tags'
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
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
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
            <Route path="shares" element={<ShareManagement />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

export default App
