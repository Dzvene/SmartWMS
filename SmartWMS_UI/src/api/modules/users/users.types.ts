/**
 * Users API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Users/DTOs/
 */

import type { ApiResponse, PaginatedResponse } from '../../types';

// ============================================================================
// User Types
// ============================================================================

export interface UserResponse {
  id: string;
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
  defaultSiteId?: string;
  defaultWarehouseId?: string;
  roleId?: string;
  roleName?: string;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  roleName?: string;
  defaultSiteId?: string;
  defaultWarehouseId?: string;
}

export interface UpdateUserRequest {
  email?: string;
  firstName?: string;
  lastName?: string;
  roleName?: string;
  isActive?: boolean;
  defaultSiteId?: string;
  defaultWarehouseId?: string;
}

export interface ResetPasswordRequest {
  newPassword: string;
}

export interface UsersListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

// ============================================================================
// Role Types
// ============================================================================

export interface RoleResponse {
  id: string;
  name: string;
  description?: string;
  isSystemRole: boolean;
  userCount: number;
  permissions: string[];
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
  permissions: string[];
}

export interface UpdateRoleRequest {
  name: string;
  description?: string;
  permissions: string[];
}

// ============================================================================
// Permission Types
// ============================================================================

export interface PermissionDto {
  code: string;
  name: string;
  description?: string;
  module: string;
  category: string;
}

export interface PermissionCategoryDto {
  category: string;
  categoryName: string;
  permissions: PermissionDto[];
}

export interface PermissionGroupDto {
  module: string;
  moduleName: string;
  categories: PermissionCategoryDto[];
}

// ============================================================================
// Response Types
// ============================================================================

export type UsersListResponse = ApiResponse<PaginatedResponse<UserResponse>>;
export type UserDetailResponse = ApiResponse<UserResponse>;
export type RolesListResponse = ApiResponse<RoleResponse[]>;
export type RoleDetailResponse = ApiResponse<RoleResponse>;
export type PermissionsResponse = ApiResponse<PermissionGroupDto[]>;
