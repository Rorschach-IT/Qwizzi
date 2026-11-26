import { apiRequest } from '../../../core/api/httpClient';
import { API_BASE_URL } from '../../../core/config/env';

export interface QuizCategory {
  categoryId: number;
  name: string;
}

export interface QuizSummary {
  quizId: string;
  title: string;
  description: string | null;
  category: string;
  coverImageId: string | null;
  coverStorageKey: string | null;
  author: string;
  questionCount: number;
  status: number;
}

export interface QuizAnswer {
  answerId: number;
  text: string | null;
  imageId: string | null;
  storageKey: string | null;
  isCorrect: boolean | null;
}

export interface QuizQuestion {
  questionId: number;
  text: string;
  imageId: string | null;
  storageKey: string | null;
  explanation: string | null;
  answers: QuizAnswer[];
}

export interface QuizEditor {
  quizId: string;
  title: string;
  description: string | null;
  categoryId: number;
  coverImageId: string | null;
  status: number;
  questions: QuizQuestion[];
}

export interface QuizWriteRequest {
  title: string;
  categoryId: number;
  description: string;
  coverImageId: string | null;
  questions: Array<{
    text: string;
    imageId: string | null;
    explanation: string;
    answers: Array<{ text: string | null; imageId: string | null; isCorrect: boolean }>;
  }>;
}

export interface ImageAsset {
  imageAssetId: string;
  storageKey: string;
  width: number;
  height: number;
}

export interface QuizPlay {
  quizId: string;
  title: string;
  category: string;
  coverImageId: string | null;
  coverStorageKey: string | null;
  questions: QuizQuestion[];
}

export interface QuizStart {
  attemptId: string;
  quiz: QuizPlay;
  startedAt: string;
}

export interface QuizAttemptResult {
  attemptId: string;
  score: number;
  totalQuestions: number;
  completedAt: string;
  questions: Array<{ questionId: number; selectedAnswerId: number; isCorrect: boolean; correctAnswerId: number }>;
}

export interface QuizStatistics {
  totalAttempts: number;
  completedAttempts: number;
  averageScorePercent: number;
  bestScorePercent: number;
  attempts: Array<{
    attemptId: string;
    quizId: string;
    quizTitle: string;
    startedAt: string;
    completedAt: string | null;
    score: number;
    totalQuestions: number;
  }>;
}

export const quizzesService = {
  browse: (search = '', categoryId?: number) => {
    const query = new URLSearchParams();
    if (search.trim()) query.set('search', search.trim());
    if (categoryId !== undefined) query.set('categoryId', String(categoryId));
    return apiRequest<QuizSummary[]>(`/quizzes${query.size ? `?${query}` : ''}`);
  },
  categories: () => apiRequest<QuizCategory[]>('/quizzes/categories'),
  mine: () => apiRequest<QuizSummary[]>('/quizzes/mine'),
  editor: (quizId: string) => apiRequest<QuizEditor>(`/quizzes/${quizId}/edit`),
  create: (request: QuizWriteRequest) =>
    apiRequest<{ quizId: string; status: number }>('/quizzes', { method: 'POST', body: request }),
  update: (quizId: string, request: QuizWriteRequest) =>
    apiRequest<void>(`/quizzes/${quizId}`, { method: 'PUT', body: request }),
  delete: (quizId: string) => apiRequest<void>(`/quizzes/${quizId}`, { method: 'DELETE' }),
  start: (quizId: string) =>
    apiRequest<QuizStart>(`/quizzes/${quizId}/attempts`, { method: 'POST' }),
  submit: (attemptId: string, answers: Record<number, number>) =>
    apiRequest<QuizAttemptResult>(`/quiz-attempts/${attemptId}/submit`, { method: 'POST', body: { answers } }),
  statistics: () => apiRequest<QuizStatistics>('/account/statistics/quizzes'),
};

export function imageUrl(imageId: string) {
  return `${API_BASE_URL}/images/${imageId}`;
}
