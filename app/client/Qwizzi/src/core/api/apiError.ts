export class ApiError extends Error {
  public readonly status: number;
  public readonly code?: string;

  constructor(
    status: number,
    message: string,
    code?: string,
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
  }

  static fromResponse(status: number, payload: unknown): ApiError {
    if (typeof payload === 'string' && payload.trim()) {
      return new ApiError(status, payload);
    }

    if (typeof payload === 'object' && payload !== null) {
      const body = payload as Record<string, unknown>;
      const message =
        (typeof body.detail === 'string' && body.detail) ||
        (typeof body.message === 'string' && body.message) ||
        (typeof body.title === 'string' && body.title) ||
        'The request could not be completed.';

      return new ApiError(
        status,
        message,
        typeof body.code === 'string' ? body.code : undefined,
      );
    }

    return new ApiError(status, 'The request could not be completed.');
  }
}