import { apiRequest } from '../../../core/api/httpClient';

export interface AdminUser {
  userId: string;
  username: string;
  email: string;
  passwordIsSet: boolean;
  role: string;
  status: boolean;
  createdAt: string;
}

export type UserSortField = 'username' | 'status' | 'role' | 'createdAt';

export const adminUsersService = {
  list: (sortBy: UserSortField, sortOrder: 'asc' | 'desc') =>
    apiRequest<AdminUser[]>(
      `/admin/users?sortBy=${sortBy}&sortOrder=${sortOrder}`,
    ),

  updateUsername: (userId: string, username: string) =>
    apiRequest<void>(`/admin/users/${userId}/username`, {
      method: 'PATCH',
      body: { username },
    }),

  updateEmail: (userId: string, email: string) =>
    apiRequest<void>(`/admin/users/${userId}/email`, {
      method: 'PATCH',
      body: { email },
    }),

  setPassword: (userId: string, password: string) =>
    apiRequest<void>(`/admin/users/${userId}/password`, {
      method: 'PATCH',
      body: { password },
    }),

  setStatus: (userId: string, status: boolean) =>
    apiRequest<void>(`/admin/users/${userId}/status`, {
      method: 'PATCH',
      body: status,
    }),

  delete: (userId: string) =>
    apiRequest<void>(`/admin/users/${userId}`, { method: 'DELETE' }),
};
