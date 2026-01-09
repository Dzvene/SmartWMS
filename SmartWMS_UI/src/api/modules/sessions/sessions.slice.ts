/**
 * Sessions API Slice
 * RTK Query endpoints for Sessions module
 */

import { baseApi } from '@/api/baseApi';
import type {
  UserSessionListResponse,
  UserSessionResponse,
  UserActiveSessionsResponse,
  LoginAttemptListResponse,
  LoginStatsResponse,
  TrustedDeviceListResponse,
  SessionQueryParams,
  LoginAttemptQueryParams,
  CreateSessionRequest,
  RevokeSessionRequest,
  RegisterDeviceRequest,
} from './sessions.types';
import type { ApiResponse } from '@/api/types';

export const sessionsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // User Sessions
    // ========================================================================

    getUserSessions: builder.query<
      UserSessionListResponse,
      { userId: string; params?: SessionQueryParams }
    >({
      query: ({ userId, params }) => ({
        url: `/sessions/user/${userId}`,
        params: params || {},
      }),
      providesTags: (_result, _error, { userId }) => [{ type: 'Sessions', id: `user-${userId}` }],
    }),

    getSessionById: builder.query<UserSessionResponse, string>({
      query: (sessionId) => `/sessions/${sessionId}`,
      providesTags: (_result, _error, id) => [{ type: 'Sessions', id }],
    }),

    getUserActiveSessions: builder.query<
      UserActiveSessionsResponse,
      { userId: string; currentSessionId?: string }
    >({
      query: ({ userId, currentSessionId }) => ({
        url: `/sessions/user/${userId}/active`,
        params: currentSessionId ? { currentSessionId } : {},
      }),
      providesTags: (_result, _error, { userId }) => [{ type: 'Sessions', id: `active-${userId}` }],
    }),

    createSession: builder.mutation<
      UserSessionResponse,
      { userId: string; data: CreateSessionRequest }
    >({
      query: ({ userId, data }) => ({
        url: `/sessions/user/${userId}`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { userId }) => [
        { type: 'Sessions', id: `user-${userId}` },
        { type: 'Sessions', id: `active-${userId}` },
      ],
    }),

    refreshSession: builder.mutation<
      UserSessionResponse,
      { sessionId: string; refreshToken: string }
    >({
      query: ({ sessionId, refreshToken }) => ({
        url: `/sessions/${sessionId}/refresh`,
        method: 'POST',
        body: { refreshToken },
      }),
      invalidatesTags: (_result, _error, { sessionId }) => [{ type: 'Sessions', id: sessionId }],
    }),

    updateSessionActivity: builder.mutation<ApiResponse<void>, string>({
      query: (sessionId) => ({
        url: `/sessions/${sessionId}/activity`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, sessionId) => [{ type: 'Sessions', id: sessionId }],
    }),

    revokeSession: builder.mutation<
      ApiResponse<void>,
      { sessionId: string; data?: RevokeSessionRequest }
    >({
      query: ({ sessionId, data }) => ({
        url: `/sessions/${sessionId}/revoke`,
        method: 'POST',
        body: data || {},
      }),
      invalidatesTags: ['Sessions'],
    }),

    revokeAllSessions: builder.mutation<
      ApiResponse<{ revokedCount: number }>,
      { userId: string; exceptSessionId?: string }
    >({
      query: ({ userId, exceptSessionId }) => ({
        url: `/sessions/user/${userId}/revoke-all`,
        method: 'POST',
        params: exceptSessionId ? { exceptSessionId } : {},
      }),
      invalidatesTags: (_result, _error, { userId }) => [
        { type: 'Sessions', id: `user-${userId}` },
        { type: 'Sessions', id: `active-${userId}` },
      ],
    }),

    // ========================================================================
    // Login Attempts
    // ========================================================================

    getLoginAttempts: builder.query<
      LoginAttemptListResponse,
      { userId?: string; params?: LoginAttemptQueryParams }
    >({
      query: ({ userId, params }) => ({
        url: `/sessions/login-attempts`,
        params: { ...(params || {}), userId },
      }),
      providesTags: ['LoginAttempts'],
    }),

    getLoginStats: builder.query<LoginStatsResponse, { userId: string; days?: number }>({
      query: ({ userId, days }) => ({
        url: `/sessions/user/${userId}/login-stats`,
        params: days ? { days } : {},
      }),
      providesTags: (_result, _error, { userId }) => [{ type: 'LoginAttempts', id: `stats-${userId}` }],
    }),

    // ========================================================================
    // Trusted Devices
    // ========================================================================

    getTrustedDevices: builder.query<TrustedDeviceListResponse, string>({
      query: (userId) => `/sessions/user/${userId}/trusted-devices`,
      providesTags: (_result, _error, userId) => [{ type: 'TrustedDevices', id: userId }],
    }),

    registerTrustedDevice: builder.mutation<
      ApiResponse<void>,
      { userId: string; data: RegisterDeviceRequest }
    >({
      query: ({ userId, data }) => ({
        url: `/sessions/user/${userId}/trusted-devices`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_result, _error, { userId }) => [{ type: 'TrustedDevices', id: userId }],
    }),

    removeTrustedDevice: builder.mutation<ApiResponse<void>, string>({
      query: (deviceId) => ({
        url: `/sessions/trusted-devices/${deviceId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['TrustedDevices'],
    }),

    // ========================================================================
    // Cleanup
    // ========================================================================

    cleanupExpiredSessions: builder.mutation<ApiResponse<{ cleanedCount: number }>, void>({
      query: () => ({
        url: `/sessions/cleanup-expired`,
        method: 'POST',
      }),
      invalidatesTags: ['Sessions'],
    }),
  }),
});

export const {
  useGetUserSessionsQuery,
  useLazyGetUserSessionsQuery,
  useGetSessionByIdQuery,
  useGetUserActiveSessionsQuery,
  useCreateSessionMutation,
  useRefreshSessionMutation,
  useUpdateSessionActivityMutation,
  useRevokeSessionMutation,
  useRevokeAllSessionsMutation,
  useGetLoginAttemptsQuery,
  useLazyGetLoginAttemptsQuery,
  useGetLoginStatsQuery,
  useGetTrustedDevicesQuery,
  useRegisterTrustedDeviceMutation,
  useRemoveTrustedDeviceMutation,
  useCleanupExpiredSessionsMutation,
} = sessionsApi;
