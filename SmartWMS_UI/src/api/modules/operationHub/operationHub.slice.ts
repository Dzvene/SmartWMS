/**
 * Operation Hub API Slice
 * RTK Query endpoints for Operation Hub module
 *
 * Endpoints cover:
 * - Session management
 * - Unified task queue
 * - Barcode scanning
 * - Operator productivity
 *
 * Note: tenantId is automatically injected by baseApi
 */

import { baseApi } from '@/api/baseApi';
import type { ApiResponse } from '@/api/types';
import type {
  StartSessionRequest,
  UpdateSessionStatusRequest,
  OperatorSessionResponse,
  ActiveSessionsResponse,
  TaskQueueQueryParams,
  TaskQueueResponse,
  UnifiedTaskResponse,
  AssignTaskRequest,
  CompleteTaskRequest,
  PauseTaskRequest,
  ScanRequest,
  ScanResultResponse,
  ScanLogQueryParams,
  ScanLogsResponse,
  OperatorStatsResponse,
  OperatorProductivityResponse,
  ProductivityQueryParams,
  ProductivityHistoryResponse,
  ProductivitySummaryResponse,
  WarehouseOverviewResponse,
  TaskActionLogQueryParams,
  TaskActionLogsResponse,
} from './operationHub.types';

export const operationHubApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Session Management
    // ========================================================================

    startSession: builder.mutation<OperatorSessionResponse, StartSessionRequest>({
      query: (body) => ({
        url: `/operation-hub/sessions/start`,
        method: 'POST',
        body,
      }),
    }),

    getCurrentSession: builder.query<OperatorSessionResponse, void>({
      query: () => `/operation-hub/sessions/current`,
    }),

    updateSessionStatus: builder.mutation<OperatorSessionResponse, { sessionId: string; data: UpdateSessionStatusRequest }>({
      query: ({ sessionId, data }) => ({
        url: `/operation-hub/sessions/${sessionId}/status`,
        method: 'PUT',
        body: data,
      }),
    }),

    endSession: builder.mutation<ApiResponse<void>, string>({
      query: (sessionId) => ({
        url: `/operation-hub/sessions/${sessionId}/end`,
        method: 'POST',
      }),
    }),

    getActiveSessions: builder.query<ActiveSessionsResponse, { warehouseId?: string } | void>({
      query: (params) => ({
        url: `/operation-hub/sessions/active`,
        params: params || {},
      }),
    }),

    // ========================================================================
    // Unified Task Queue
    // ========================================================================

    getTaskQueue: builder.query<TaskQueueResponse, TaskQueueQueryParams | void>({
      query: (params: TaskQueueQueryParams = {}) => ({
        url: `/operation-hub/tasks`,
        params,
      }),
    }),

    getTaskById: builder.query<UnifiedTaskResponse, { taskType: string; taskId: string }>({
      query: ({ taskType, taskId }) => `/operation-hub/tasks/${taskType}/${taskId}`,
    }),

    getNextTask: builder.query<UnifiedTaskResponse, { warehouseId: string; preferredTaskType?: string }>({
      query: ({ warehouseId, preferredTaskType }) => ({
        url: `/operation-hub/tasks/next`,
        params: { warehouseId, preferredTaskType },
      }),
    }),

    assignTask: builder.mutation<UnifiedTaskResponse, AssignTaskRequest>({
      query: (body) => ({
        url: `/operation-hub/tasks/assign`,
        method: 'POST',
        body,
      }),
    }),

    startTask: builder.mutation<UnifiedTaskResponse, { taskType: string; taskId: string }>({
      query: ({ taskType, taskId }) => ({
        url: `/operation-hub/tasks/${taskType}/${taskId}/start`,
        method: 'POST',
      }),
    }),

    completeTask: builder.mutation<UnifiedTaskResponse, CompleteTaskRequest>({
      query: ({ taskType, taskId, ...body }) => ({
        url: `/operation-hub/tasks/${taskType}/${taskId}/complete`,
        method: 'POST',
        body,
      }),
    }),

    pauseTask: builder.mutation<UnifiedTaskResponse, PauseTaskRequest>({
      query: ({ taskType, taskId, ...body }) => ({
        url: `/operation-hub/tasks/${taskType}/${taskId}/pause`,
        method: 'POST',
        body,
      }),
    }),

    resumeTask: builder.mutation<UnifiedTaskResponse, { taskType: string; taskId: string }>({
      query: ({ taskType, taskId }) => ({
        url: `/operation-hub/tasks/${taskType}/${taskId}/resume`,
        method: 'POST',
      }),
    }),

    // ========================================================================
    // Barcode Scanning
    // ========================================================================

    processScan: builder.mutation<ScanResultResponse, ScanRequest>({
      query: (body) => ({
        url: `/operation-hub/scan`,
        method: 'POST',
        body,
      }),
    }),

    validateBarcode: builder.query<ScanResultResponse, { barcode: string; context?: string }>({
      query: ({ barcode, context }) => ({
        url: `/operation-hub/scan/validate`,
        params: { barcode, context },
      }),
    }),

    getScanLogs: builder.query<ScanLogsResponse, ScanLogQueryParams | void>({
      query: (params: ScanLogQueryParams = {}) => ({
        url: `/operation-hub/scan/logs`,
        params,
      }),
    }),

    // ========================================================================
    // Operator Productivity
    // ========================================================================

    getOperatorStats: builder.query<OperatorStatsResponse, string>({
      query: (userId) => `/operation-hub/operator/${userId}/stats`,
    }),

    getMyStats: builder.query<OperatorStatsResponse, void>({
      query: () => `/operation-hub/operator/me/stats`,
    }),

    getOperatorProductivity: builder.query<OperatorProductivityResponse, { userId: string; date?: string }>({
      query: ({ userId, date }) => ({
        url: `/operation-hub/operator/${userId}/productivity`,
        params: date ? { date } : {},
      }),
    }),

    getProductivityHistory: builder.query<ProductivityHistoryResponse, ProductivityQueryParams | void>({
      query: (params: ProductivityQueryParams = {}) => ({
        url: `/operation-hub/productivity/history`,
        params,
      }),
    }),

    getProductivitySummary: builder.query<ProductivitySummaryResponse, ProductivityQueryParams | void>({
      query: (params: ProductivityQueryParams = {}) => ({
        url: `/operation-hub/productivity/summary`,
        params,
      }),
    }),

    getWarehouseOperatorsOverview: builder.query<WarehouseOverviewResponse, string>({
      query: (warehouseId) => `/operation-hub/warehouse/${warehouseId}/operators`,
    }),

    // ========================================================================
    // Task Action Logs
    // ========================================================================

    getTaskActionLogs: builder.query<TaskActionLogsResponse, TaskActionLogQueryParams | void>({
      query: (params: TaskActionLogQueryParams = {}) => ({
        url: `/operation-hub/logs/actions`,
        params,
      }),
    }),

    getTaskHistory: builder.query<TaskActionLogsResponse, { taskType: string; taskId: string }>({
      query: ({ taskType, taskId }) => `/operation-hub/tasks/${taskType}/${taskId}/history`,
    }),
  }),
});

// Export hooks
export const {
  // Sessions
  useStartSessionMutation,
  useGetCurrentSessionQuery,
  useLazyGetCurrentSessionQuery,
  useUpdateSessionStatusMutation,
  useEndSessionMutation,
  useGetActiveSessionsQuery,
  useLazyGetActiveSessionsQuery,

  // Tasks
  useGetTaskQueueQuery,
  useLazyGetTaskQueueQuery,
  useGetTaskByIdQuery,
  useLazyGetTaskByIdQuery,
  useGetNextTaskQuery,
  useLazyGetNextTaskQuery,
  useAssignTaskMutation,
  useStartTaskMutation,
  useCompleteTaskMutation,
  usePauseTaskMutation,
  useResumeTaskMutation,

  // Scanning
  useProcessScanMutation,
  useValidateBarcodeQuery,
  useLazyValidateBarcodeQuery,
  useGetScanLogsQuery,
  useLazyGetScanLogsQuery,

  // Productivity
  useGetOperatorStatsQuery,
  useLazyGetOperatorStatsQuery,
  useGetMyStatsQuery,
  useLazyGetMyStatsQuery,
  useGetOperatorProductivityQuery,
  useLazyGetOperatorProductivityQuery,
  useGetProductivityHistoryQuery,
  useLazyGetProductivityHistoryQuery,
  useGetProductivitySummaryQuery,
  useLazyGetProductivitySummaryQuery,
  useGetWarehouseOperatorsOverviewQuery,
  useLazyGetWarehouseOperatorsOverviewQuery,

  // Logs
  useGetTaskActionLogsQuery,
  useLazyGetTaskActionLogsQuery,
  useGetTaskHistoryQuery,
  useLazyGetTaskHistoryQuery,
} = operationHubApi;
