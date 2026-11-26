import { apiRequest } from '../../../core/api/httpClient';
import type { QuizSummary } from '../../quizzes/api/quizzesService';

export const adminQuizzesService = {
  list: () => apiRequest<QuizSummary[]>('/admin/quizzes'),
  setStatus: (quizId: string, status: 3 | 4, rejectionReason = '') =>
    apiRequest<void>(`/admin/quizzes/${quizId}/status`, {
      method: 'PATCH',
      body: { status, rejectionReason: rejectionReason || null },
    }),
};
