import { ApiError } from './apiError';
import { API_BASE_URL } from '../config/env';

type RequestOptions = Omit<RequestInit, 'body'> & {
  body?: unknown;
};

async function getCsrfToken(): Promise<string> {
  const response = await fetch(`${API_BASE_URL}/auth/csrf`, {
    credentials: 'include',
    cache: 'no-store',
  });

  if (!response.ok) {
    throw new ApiError(response.status, 'Unable to prepare a secure request.');
  }

  const payload: { token?: string } = await response.json();
  if (!payload.token) {
    throw new ApiError(response.status, 'The security token was missing from the response.');
  }

  return payload.token;
}

export async function apiRequest<T>(
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const method = (options.method ?? 'GET').toUpperCase();
  const headers = new Headers(options.headers);
  const isFormData = options.body instanceof FormData;

  if (options.body !== undefined && !isFormData) {
    headers.set('Content-Type', 'application/json');
  }

  if (!['GET', 'HEAD', 'OPTIONS'].includes(method)) {
    headers.set('X-CSRF-TOKEN', await getCsrfToken());
  }

  const body: BodyInit | undefined =
    options.body instanceof FormData
      ? options.body
      : options.body === undefined
        ? undefined
        : JSON.stringify(options.body);

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    method,
    headers,
    credentials: 'include',
    body,
  });

  if (response.status === 204) {
    return undefined as T;
  }

  const contentType = response.headers.get('content-type') ?? '';
  const payload: unknown = contentType.includes('json')
    ? await response.json()
    : await response.text();

  if (!response.ok) {
    throw ApiError.fromResponse(response.status, payload);
  }

  return payload as T;
}
