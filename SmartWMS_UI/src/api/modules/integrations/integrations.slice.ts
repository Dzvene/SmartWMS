/**
 * Integrations API Slice
 */

import { baseApi } from '@/api/baseApi';
import type { ApiResponse } from '@/api/types';
import type {
  IntegrationListResponse,
  IntegrationResponse,
  IntegrationFilters,
  CreateIntegrationRequest,
  UpdateIntegrationRequest,
  SyncJobListResponse,
  SyncJobResponse,
  SyncJobFilters,
  TriggerSyncRequest,
  SyncLogListResponse,
  WebhookListResponse,
  WebhookResponse,
  CreateWebhookRequest,
  UpdateWebhookRequest,
} from './integrations.types';

export const integrationsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Integrations
    // ========================================================================

    getIntegrations: builder.query<IntegrationListResponse, IntegrationFilters | void>({
      query: (params) => ({
        url: `/integrations`,
        params: params || {},
      }),
      providesTags: ['Integrations'],
    }),

    getIntegrationById: builder.query<IntegrationResponse, string>({
      query: (id) => `/integrations/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'Integrations', id }],
    }),

    createIntegration: builder.mutation<IntegrationResponse, CreateIntegrationRequest>({
      query: (body) => ({
        url: `/integrations`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Integrations'],
    }),

    updateIntegration: builder.mutation<IntegrationResponse, { id: string; data: UpdateIntegrationRequest }>({
      query: ({ id, data }) => ({
        url: `/integrations/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['Integrations'],
    }),

    deleteIntegration: builder.mutation<ApiResponse<void>, string>({
      query: (id) => ({
        url: `/integrations/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Integrations'],
    }),

    activateIntegration: builder.mutation<IntegrationResponse, string>({
      query: (id) => ({
        url: `/integrations/${id}/activate`,
        method: 'POST',
      }),
      invalidatesTags: ['Integrations'],
    }),

    deactivateIntegration: builder.mutation<IntegrationResponse, string>({
      query: (id) => ({
        url: `/integrations/${id}/deactivate`,
        method: 'POST',
      }),
      invalidatesTags: ['Integrations'],
    }),

    testIntegrationConnection: builder.mutation<ApiResponse<{ success: boolean; message: string }>, string>({
      query: (id) => ({
        url: `/integrations/${id}/test`,
        method: 'POST',
      }),
    }),

    // ========================================================================
    // Sync Jobs
    // ========================================================================

    getSyncJobs: builder.query<SyncJobListResponse, SyncJobFilters | void>({
      query: (params) => ({
        url: `/integrations/sync-jobs`,
        params: params || {},
      }),
      providesTags: ['SyncJobs'],
    }),

    getSyncJobById: builder.query<SyncJobResponse, string>({
      query: (id) => `/integrations/sync-jobs/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'SyncJobs', id }],
    }),

    triggerSync: builder.mutation<SyncJobResponse, { integrationId: string; data: TriggerSyncRequest }>({
      query: ({ integrationId, data }) => ({
        url: `/integrations/${integrationId}/sync`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['SyncJobs', 'Integrations'],
    }),

    cancelSyncJob: builder.mutation<ApiResponse<void>, string>({
      query: (id) => ({
        url: `/integrations/sync-jobs/${id}/cancel`,
        method: 'POST',
      }),
      invalidatesTags: ['SyncJobs'],
    }),

    retrySyncJob: builder.mutation<SyncJobResponse, string>({
      query: (id) => ({
        url: `/integrations/sync-jobs/${id}/retry`,
        method: 'POST',
      }),
      invalidatesTags: ['SyncJobs'],
    }),

    getSyncLogs: builder.query<SyncLogListResponse, { syncJobId: string; page?: number; pageSize?: number }>({
      query: ({ syncJobId, page, pageSize }) => ({
        url: `/integrations/sync-jobs/${syncJobId}/logs`,
        params: { page, pageSize },
      }),
      providesTags: ['SyncLogs'],
    }),

    // ========================================================================
    // Webhooks
    // ========================================================================

    getWebhooks: builder.query<WebhookListResponse, string>({
      query: (integrationId) => `/integrations/${integrationId}/webhooks`,
      providesTags: ['Webhooks'],
    }),

    createWebhook: builder.mutation<WebhookResponse, CreateWebhookRequest>({
      query: (body) => ({
        url: `/integrations/webhooks`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Webhooks'],
    }),

    updateWebhook: builder.mutation<WebhookResponse, { id: string; data: UpdateWebhookRequest }>({
      query: ({ id, data }) => ({
        url: `/integrations/webhooks/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['Webhooks'],
    }),

    deleteWebhook: builder.mutation<ApiResponse<void>, string>({
      query: (id) => ({
        url: `/integrations/webhooks/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Webhooks'],
    }),

    testWebhook: builder.mutation<ApiResponse<{ success: boolean; message: string }>, string>({
      query: (id) => ({
        url: `/integrations/webhooks/${id}/test`,
        method: 'POST',
      }),
    }),
  }),
});

export const {
  // Integrations
  useGetIntegrationsQuery,
  useGetIntegrationByIdQuery,
  useCreateIntegrationMutation,
  useUpdateIntegrationMutation,
  useDeleteIntegrationMutation,
  useActivateIntegrationMutation,
  useDeactivateIntegrationMutation,
  useTestIntegrationConnectionMutation,
  // Sync Jobs
  useGetSyncJobsQuery,
  useGetSyncJobByIdQuery,
  useTriggerSyncMutation,
  useCancelSyncJobMutation,
  useRetrySyncJobMutation,
  useGetSyncLogsQuery,
  // Webhooks
  useGetWebhooksQuery,
  useCreateWebhookMutation,
  useUpdateWebhookMutation,
  useDeleteWebhookMutation,
  useTestWebhookMutation,
} = integrationsApi;
