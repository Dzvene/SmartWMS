/**
 * Notifications API Slice
 */

import { baseApi } from '@/api/baseApi';
import type { ApiResponse } from '@/api/types';
import type {
  NotificationListResponse,
  NotificationResponse,
  NotificationFilters,
  SendNotificationRequest,
  NotificationPreferencesResponse,
  UpdateNotificationPreferencesRequest,
  UnreadCountResponse,
  NotificationTemplateListResponse,
  NotificationTemplateResponse,
  CreateNotificationTemplateRequest,
  UpdateNotificationTemplateRequest,
} from './notifications.types';

export const notificationsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // User Notifications
    // ========================================================================

    getNotifications: builder.query<NotificationListResponse, NotificationFilters | void>({
      query: (params) => ({
        url: `/notifications`,
        params: params || {},
      }),
      providesTags: ['Notifications'],
    }),

    getNotificationById: builder.query<NotificationResponse, string>({
      query: (id) => `/notifications/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'Notifications', id }],
    }),

    getUnreadCount: builder.query<UnreadCountResponse, void>({
      query: () => `/notifications/unread-count`,
      providesTags: ['NotificationCount'],
    }),

    markAsRead: builder.mutation<ApiResponse<void>, string>({
      query: (id) => ({
        url: `/notifications/${id}/read`,
        method: 'POST',
      }),
      invalidatesTags: ['Notifications', 'NotificationCount'],
    }),

    markAllAsRead: builder.mutation<ApiResponse<{ markedCount: number }>, void>({
      query: () => ({
        url: `/notifications/read-all`,
        method: 'POST',
      }),
      invalidatesTags: ['Notifications', 'NotificationCount'],
    }),

    deleteNotification: builder.mutation<ApiResponse<void>, string>({
      query: (id) => ({
        url: `/notifications/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Notifications', 'NotificationCount'],
    }),

    deleteAllNotifications: builder.mutation<ApiResponse<{ deletedCount: number }>, void>({
      query: () => ({
        url: `/notifications`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Notifications', 'NotificationCount'],
    }),

    // ========================================================================
    // Send Notifications (Admin)
    // ========================================================================

    sendNotification: builder.mutation<NotificationResponse, SendNotificationRequest>({
      query: (body) => ({
        url: `/notifications/send`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Notifications'],
    }),

    sendBulkNotification: builder.mutation<ApiResponse<{ sentCount: number }>, SendNotificationRequest>({
      query: (body) => ({
        url: `/notifications/send-bulk`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Notifications'],
    }),

    // ========================================================================
    // User Preferences
    // ========================================================================

    getNotificationPreferences: builder.query<NotificationPreferencesResponse, string>({
      query: (userId) => `/notifications/preferences/${userId}`,
      providesTags: ['NotificationPreferences'],
    }),

    updateNotificationPreferences: builder.mutation<
      NotificationPreferencesResponse,
      { userId: string; data: UpdateNotificationPreferencesRequest }
    >({
      query: ({ userId, data }) => ({
        url: `/notifications/preferences/${userId}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['NotificationPreferences'],
    }),

    // ========================================================================
    // Notification Templates (Admin)
    // ========================================================================

    getNotificationTemplates: builder.query<NotificationTemplateListResponse, { page?: number; pageSize?: number }>({
      query: (params) => ({
        url: `/notifications/templates`,
        params,
      }),
      providesTags: ['NotificationTemplates'],
    }),

    getNotificationTemplateById: builder.query<NotificationTemplateResponse, string>({
      query: (id) => `/notifications/templates/${id}`,
      providesTags: (_r, _e, id) => [{ type: 'NotificationTemplates', id }],
    }),

    createNotificationTemplate: builder.mutation<NotificationTemplateResponse, CreateNotificationTemplateRequest>({
      query: (body) => ({
        url: `/notifications/templates`,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['NotificationTemplates'],
    }),

    updateNotificationTemplate: builder.mutation<
      NotificationTemplateResponse,
      { id: string; data: UpdateNotificationTemplateRequest }
    >({
      query: ({ id, data }) => ({
        url: `/notifications/templates/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['NotificationTemplates'],
    }),

    deleteNotificationTemplate: builder.mutation<ApiResponse<void>, string>({
      query: (id) => ({
        url: `/notifications/templates/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['NotificationTemplates'],
    }),
  }),
});

export const {
  // User Notifications
  useGetNotificationsQuery,
  useGetNotificationByIdQuery,
  useGetUnreadCountQuery,
  useMarkAsReadMutation,
  useMarkAllAsReadMutation,
  useDeleteNotificationMutation,
  useDeleteAllNotificationsMutation,
  // Send Notifications
  useSendNotificationMutation,
  useSendBulkNotificationMutation,
  // Preferences
  useGetNotificationPreferencesQuery,
  useUpdateNotificationPreferencesMutation,
  // Templates
  useGetNotificationTemplatesQuery,
  useGetNotificationTemplateByIdQuery,
  useCreateNotificationTemplateMutation,
  useUpdateNotificationTemplateMutation,
  useDeleteNotificationTemplateMutation,
} = notificationsApi;
