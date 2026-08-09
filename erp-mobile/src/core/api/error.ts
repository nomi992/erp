import axios from 'axios';
import type { ApiResponse } from '../models/api-response';

/** Pulls the backend's ApiResponse.message/errors out of a failed axios call, if present. */
export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (axios.isAxiosError<ApiResponse<unknown>>(error)) {
    const data = error.response?.data;
    if (data?.errors && data.errors.length > 0) {
      return data.errors.join('\n');
    }
    if (data?.message) {
      return data.message;
    }
  }
  if (error instanceof Error) {
    return error.message;
  }
  return fallback;
}
