import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { ApiError } from '../../../core/api/apiError';
import { authService, type AuthUser } from '../api/authService';
import { AuthContext } from './auth-context';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    void authService
      .me()
      .then((currentUser) => {
        if (active) setUser(currentUser);
      })
      .catch((requestError: unknown) => {
        if (!active) return;
        if (requestError instanceof ApiError && requestError.status === 401) {
          setUser(null);
          return;
        }

        setError(
          requestError instanceof Error
            ? requestError.message
            : 'Unable to check your session.',
        );
      })
      .finally(() => {
        if (active) setLoading(false);
      });

    return () => {
      active = false;
    };
  }, []);

  const signIn = useCallback((authenticatedUser: AuthUser) => {
    setUser(authenticatedUser);
    setError(null);
  }, []);

  const updateUser = useCallback((updatedUser: AuthUser) => {
    setUser(updatedUser);
  }, []);

  const signOut = useCallback(async () => {
    await authService.logout();
    setUser(null);
    setError(null);
  }, []);

  const clearSession = useCallback(() => {
    setUser(null);
    setError(null);
  }, []);

  const clearError = useCallback(() => setError(null), []);

  const value = useMemo(
    () => ({
      user,
      loading,
      error,
      signIn,
      updateUser,
      signOut,
      clearSession,
      clearError,
    }),
    [user, loading, error, signIn, updateUser, signOut, clearSession, clearError],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}