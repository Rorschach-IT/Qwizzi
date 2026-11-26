import { apiRequest } from '../../../core/api/httpClient';

export interface SecurityEvent {
  sequence: number;
  userId: string | null;
  username: string | null;
  occurredAt: string;
  ipAddress: string | null;
  action: string;
  description: string;
  succeeded: boolean;
}

export interface AuditVerification {
  isIntact: boolean;
  firstInvalidSequence: number | null;
  verifiedEntries: number;
}

export const adminSecurityService = {
  listEvents: () => apiRequest<SecurityEvent[]>('/admin/security/events?limit=100'),
  verifyEvents: () => apiRequest<AuditVerification>('/admin/security/events/verify', { method: 'POST' }),
  enforcePasswordPolicy: () =>
    apiRequest<{ affectedUsers: number }>('/admin/security/password-policy/enforce', { method: 'POST' }),
};
