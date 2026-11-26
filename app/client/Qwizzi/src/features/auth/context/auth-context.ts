import { createContext } from 'react';
import type { AuthUser } from '../api/authService';

export interface AuthContextValue {
  user: AuthUser | null;
  loading: boolean;
  error: string | null;
  signIn: (user: AuthUser) => void;
  updateUser: (user: AuthUser) => void;
  signOut: () => Promise<void>;
  clearSession: () => void;
  clearError: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
