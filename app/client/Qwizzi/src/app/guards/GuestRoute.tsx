import { Navigate } from 'react-router-dom';
import { useAuth } from '../../features/auth/context/useAuth';

export function GuestRoute({ children }: { children: React.ReactNode }) {
  const { user, loading } = useAuth();
  if (loading) return <div className="page-loading" role="status">Loading your account…</div>;
  if (user) return <Navigate to="/" replace />;
  return children;
}