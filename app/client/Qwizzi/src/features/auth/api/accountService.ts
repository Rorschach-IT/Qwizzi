import { apiRequest } from '../../../core/api/httpClient';
import type { AuthUser } from './authService';

export const accountService = {
  updateUsername: (username: string) =>
    apiRequest<AuthUser>('/account/username', {
      method: 'PATCH',
      body: { username },
    }),

  updateEmail: (email: string) =>
    apiRequest<AuthUser>('/account/email', {
      method: 'PATCH',
      body: { email },
    }),

  changePassword: (currentPassword: string, newPassword: string, turnstileToken: string) =>
    apiRequest<void>('/account/password', {
      method: 'PATCH',
      body: { currentPassword, newPassword, turnstileToken },
    }),

  deleteAccount: () =>
    apiRequest<void>('/account', { method: 'DELETE' }),
};
