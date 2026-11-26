import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../features/auth/context/useAuth';
import { ForbiddenPage } from '../../features/system/pages/SystemPages';

export function AdminRoute({ children }: { children: React.ReactNode }) {
  const { user, loading } = useAuth();
  const location = useLocation();

  if (loading) return <div className="page-loading" role="status">Loading your account…</div>;
  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }
  if (user.role !== 'Administrator') return <ForbiddenPage />;

  return children;
}