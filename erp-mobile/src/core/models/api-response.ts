// 1:1 port of erp/src/app/core/models/api-response.model.ts — every controller wraps its
// response in this shape (Models/ApiResponse.cs on the backend).
export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string;
  data: T | null;
  errors: string[] | null;
}
