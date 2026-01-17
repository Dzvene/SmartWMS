/**
 * Users API Slice
 * RTK Query endpoints for Users module
 *
 * Endpoints:
 * - /users - User CRUD
 * - /roles - Role CRUD
 *
 * Note: tenantId is automatically injected by baseApi
 */

import { baseApi } from '@/api/baseApi';
import type { ApiResponse } from '@/api/types';
import type {
  UsersListParams,
  UsersListResponse,
  UserDetailResponse,
  CreateUserRequest,
  UpdateUserRequest,
  ResetPasswordRequest,
  RolesListResponse,
  RoleDetailResponse,
  CreateRoleRequest,
  UpdateRoleRequest,
  PermissionsResponse,
} from './users.types';

export const usersApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Users CRUD
    // ========================================================================

    getUsers: builder.query<UsersListResponse, UsersListParams | void>({
      query: ({
        page = 1,
        pageSize = 25,
        search,
        isActive,
      }: UsersListParams = {}) => ({
        url: `/users`,
        params: {
          page,
          pageSize,
          search,
          isActive,
        },
      }),
      providesTags: (result) =>
        result?.data
          ? [
              ...result.data.items.map(({ id }) => ({ type: 'User' as const, id })),
              { type: 'User', id: 'LIST' },
            ]
          : [{ type: 'User', id: 'LIST' }],
    }),

    getUserById: builder.query<UserDetailResponse, string>({
      query: (id: string) => `/users/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'User', id }],
    }),

    getCurrentUser: builder.query<UserDetailResponse, void>({
      query: () => `/users/me`,
      providesTags: [{ type: 'User', id: 'CURRENT' }],
    }),

    createUser: builder.mutation<UserDetailResponse, CreateUserRequest>({
      query: (body: CreateUserRequest) => ({
        url: `/users`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [{ type: 'User', id: 'LIST' }],
    }),

    updateUser: builder.mutation<UserDetailResponse, { id: string; data: UpdateUserRequest }>({
      query: ({ id, data }: { id: string; data: UpdateUserRequest }) => ({
        url: `/users/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'User', id },
        { type: 'User', id: 'LIST' },
      ],
    }),

    deactivateUser: builder.mutation<ApiResponse<void>, string>({
      query: (id: string) => ({
        url: `/users/${id}/deactivate`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'User', id },
        { type: 'User', id: 'LIST' },
      ],
    }),

    activateUser: builder.mutation<ApiResponse<void>, string>({
      query: (id: string) => ({
        url: `/users/${id}/activate`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'User', id },
        { type: 'User', id: 'LIST' },
      ],
    }),

    resetPassword: builder.mutation<ApiResponse<void>, { id: string; data: ResetPasswordRequest }>({
      query: ({ id, data }: { id: string; data: ResetPasswordRequest }) => ({
        url: `/users/${id}/reset-password`,
        method: 'POST',
        body: data,
      }),
    }),

    deleteUser: builder.mutation<ApiResponse<void>, string>({
      query: (id: string) => ({
        url: `/users/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'User', id },
        { type: 'User', id: 'LIST' },
      ],
    }),

    // ========================================================================
    // Roles CRUD
    // ========================================================================

    getRoles: builder.query<RolesListResponse, void>({
      query: () => `/roles`,
      providesTags: ['User'],
    }),

    getRoleById: builder.query<RoleDetailResponse, string>({
      query: (id: string) => `/roles/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'User', id: `role-${id}` }],
    }),

    createRole: builder.mutation<RoleDetailResponse, CreateRoleRequest>({
      query: (body: CreateRoleRequest) => ({
        url: `/roles`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['User'],
    }),

    updateRole: builder.mutation<RoleDetailResponse, { id: string; data: UpdateRoleRequest }>({
      query: ({ id, data }: { id: string; data: UpdateRoleRequest }) => ({
        url: `/roles/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'User', id: `role-${id}` }, 'User'],
    }),

    deleteRole: builder.mutation<ApiResponse<void>, string>({
      query: (id: string) => ({
        url: `/roles/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['User'],
    }),

    getPermissions: builder.query<PermissionsResponse, void>({
      query: () => `/roles/permissions`,
    }),
  }),
});

// Export hooks
export const {
  // Users
  useGetUsersQuery,
  useLazyGetUsersQuery,
  useGetUserByIdQuery,
  useLazyGetUserByIdQuery,
  useGetCurrentUserQuery,
  useCreateUserMutation,
  useUpdateUserMutation,
  useDeactivateUserMutation,
  useActivateUserMutation,
  useResetPasswordMutation,
  useDeleteUserMutation,
  // Roles
  useGetRolesQuery,
  useLazyGetRolesQuery,
  useGetRoleByIdQuery,
  useLazyGetRoleByIdQuery,
  useCreateRoleMutation,
  useUpdateRoleMutation,
  useDeleteRoleMutation,
  useGetPermissionsQuery,
} = usersApi;
