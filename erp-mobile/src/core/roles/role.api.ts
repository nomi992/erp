import { apiClient } from '../api/client';
import type { ApiResponse } from '../models/api-response';
import type { RoleOption } from './role.types';

interface RoleResponseDto {
  id: number;
  name: string;
  isSystemRole: boolean;
}

export async function getRoles(): Promise<RoleOption[]> {
  const response = await apiClient.get<ApiResponse<RoleResponseDto[]>>('/api/roles');
  return (response.data.data ?? []).map((r) => ({ id: r.id, name: r.name, isSystemRole: r.isSystemRole }));
}
