/**
 * Audit API Slice
 * RTK Query endpoints for Audit module
 */

import { baseApi } from '@/api/baseApi';
import type {
  AuditLogListResponse,
  AuditLogResponse,
  AuditLogFilters,
  ActivityLogListResponse,
  ActivityLogFilters,
  SystemEventListResponse,
  SystemEventResponse,
  SystemEventFilters,
  AuditStatisticsResponse,
  UserActivitySummaryResponse,
} from './audit.types';
import type { ApiResponse } from '@/api/types';

export const auditApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Audit Logs
    // ========================================================================

    getAuditLogs: builder.query<AuditLogListResponse, AuditLogFilters | void>({
      query: (params) => ({
        url: `/audit/logs`,
        params: params || {},
      }),
      providesTags: ['AuditLogs'],
    }),

    getAuditLogById: builder.query<AuditLogResponse, string>({
      query: (id) => `/audit/logs/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'AuditLogs', id }],
    }),

    getEntityAuditHistory: builder.query<AuditLogListResponse, { entityType: string; entityId: string }>({
      query: ({ entityType, entityId }) => `/audit/logs/entity/${entityType}/${entityId}`,
      providesTags: (_result, _error, { entityType, entityId }) => [
        { type: 'AuditLogs', id: `${entityType}-${entityId}` },
      ],
    }),

    // ========================================================================
    // Activity Logs
    // ========================================================================

    getActivityLogs: builder.query<ActivityLogListResponse, ActivityLogFilters | void>({
      query: (params) => ({
        url: `/audit/activities`,
        params: params || {},
      }),
      providesTags: ['ActivityLogs'],
    }),

    getUserActivity: builder.query<
      ActivityLogListResponse,
      { userId: string; from?: string; to?: string }
    >({
      query: ({ userId, from, to }) => ({
        url: `/audit/activities/user/${userId}`,
        params: { from, to },
      }),
      providesTags: (_result, _error, { userId }) => [{ type: 'ActivityLogs', id: `user-${userId}` }],
    }),

    // ========================================================================
    // System Events
    // ========================================================================

    getSystemEvents: builder.query<SystemEventListResponse, SystemEventFilters | void>({
      query: (params) => ({
        url: `/audit/system-events`,
        params: params || {},
      }),
      providesTags: ['SystemEvents'],
    }),

    getSystemEventById: builder.query<SystemEventResponse, string>({
      query: (id) => `/audit/system-events/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'SystemEvents', id }],
    }),

    // ========================================================================
    // Statistics
    // ========================================================================

    getAuditStatistics: builder.query<AuditStatisticsResponse, { from?: string; to?: string } | void>({
      query: (params) => ({
        url: `/audit/statistics`,
        params: params || {},
      }),
      providesTags: ['AuditStatistics'],
    }),

    getUserActivitySummary: builder.query<
      UserActivitySummaryResponse,
      { userId: string; from?: string; to?: string }
    >({
      query: ({ userId, from, to }) => ({
        url: `/audit/statistics/user/${userId}`,
        params: { from, to },
      }),
      providesTags: (_result, _error, { userId }) => [{ type: 'AuditStatistics', id: `user-${userId}` }],
    }),

    // ========================================================================
    // Cleanup
    // ========================================================================

    purgeOldLogs: builder.mutation<ApiResponse<{ deletedCount: number }>, number>({
      query: (daysToKeep) => ({
        url: `/audit/purge`,
        method: 'DELETE',
        params: { daysToKeep },
      }),
      invalidatesTags: ['AuditLogs', 'ActivityLogs', 'SystemEvents', 'AuditStatistics'],
    }),
  }),
});

export const {
  useGetAuditLogsQuery,
  useLazyGetAuditLogsQuery,
  useGetAuditLogByIdQuery,
  useGetEntityAuditHistoryQuery,
  useGetActivityLogsQuery,
  useLazyGetActivityLogsQuery,
  useGetUserActivityQuery,
  useGetSystemEventsQuery,
  useLazyGetSystemEventsQuery,
  useGetSystemEventByIdQuery,
  useGetAuditStatisticsQuery,
  useGetUserActivitySummaryQuery,
  usePurgeOldLogsMutation,
} = auditApi;
