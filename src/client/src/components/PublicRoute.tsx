import { Navigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';

export default function PublicRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  if (isAuthenticated) {
    return <Navigate to="/photos" replace />;
  }

  return <>{children}</>;
}
