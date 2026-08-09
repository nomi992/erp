import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { AdminUser, CreateUserRequest, UpdateUserRequest } from './user.types';

export async function getUsers(): Promise<AdminUser[]> {
  const response = await apiClient.get<ApiResponse<AdminUser[]>>('/api/users');
  return response.data.data ?? [];
}

export async function createUser(request: CreateUserRequest): Promise<AdminUser> {
  const response = await apiClient.post<ApiResponse<AdminUser>>('/api/users', request);
  if (!response.data.data) {
    throw new Error(response.data.message || 'Unable to create user.');
  }
  return response.data.data;
}

export async function updateUser(id: number, request: UpdateUserRequest): Promise<AdminUser> {
  const response = await apiClient.put<ApiResponse<AdminUser>>(`/api/users/${id}`, request);
  if (!response.data.data) {
    throw new Error(response.data.message || 'Unable to update user.');
  }
  return response.data.data;
}

export async function activateUser(id: number): Promise<void> {
  await apiClient.patch(`/api/users/${id}/activate`, {});
}

export async function deactivateUser(id: number): Promise<void> {
  await apiClient.patch(`/api/users/${id}/deactivate`, {});
}

export async function grantBranch(userId: number, branchId: number): Promise<void> {
  await apiClient.post(`/api/users/${userId}/branches`, { branchId });
}

export async function revokeBranch(userId: number, branchId: number): Promise<void> {
  await apiClient.delete(`/api/users/${userId}/branches/${branchId}`);
}
