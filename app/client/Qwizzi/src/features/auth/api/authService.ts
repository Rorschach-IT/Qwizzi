import { apiRequest } from '../../../core/api/httpClient';

export interface AuthUser {
  userId: string;
  username: string;
  email: string;
  role: 'User' | 'Administrator' | string;
}

export const authService = {
  login: (values: {
    email: string;
    password: string;
    rememberMe: boolean;
    website: string;
  }) =>
    apiRequest<AuthUser>('/auth/login', {
      method: 'POST',
      body: values,
    }),

  register: (values: {
    username: string;
    email: string;
    password: string;
    website: string;
  }) =>
    apiRequest<AuthUser>('/auth/register', {
      method: 'POST',
      body: values,
    }),

  me: () => apiRequest<AuthUser>('/auth/me'),

  logout: () => apiRequest<void>('/auth/logout', { method: 'POST' }),
};