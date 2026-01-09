/**
 * Notifications Module Types
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================================================
// Enums
// ============================================================================

export type NotificationType =
  | 'Info'
  | 'Warning'
  | 'Error'
  | 'Success'
  | 'Alert'
  | 'Reminder'
  | 'Task'
  | 'System';

export type NotificationPriority = 'Low' | 'Normal' | 'High' | 'Urgent';

export type NotificationChannel = 'InApp' | 'Email' | 'SMS' | 'Push' | 'Webhook';

export type NotificationStatus = 'Pending' | 'Sent' | 'Delivered' | 'Read' | 'Failed';

// ============================================================================
// DTOs
// ============================================================================

export interface NotificationDto {
  id: string;
  userId: string;
  type: NotificationType;
  priority: NotificationPriority;
  title: string;
  message: string;
  data?: Record<string, unknown>;
  actionUrl?: string;
  actionLabel?: string;
  isRead: boolean;
  readAt?: string;
  expiresAt?: string;
  createdAt: string;
}

export interface NotificationSummaryDto {
  id: string;
  type: NotificationType;
  priority: NotificationPriority;
  title: string;
  message: string;
  isRead: boolean;
  actionUrl?: string;
  createdAt: string;
}

export interface NotificationPreferencesDto {
  userId: string;
  emailEnabled: boolean;
  smsEnabled: boolean;
  pushEnabled: boolean;
  inAppEnabled: boolean;
  digestEnabled: boolean;
  digestFrequency?: 'Daily' | 'Weekly';
  quietHoursEnabled: boolean;
  quietHoursStart?: string;
  quietHoursEnd?: string;
  preferences: NotificationCategoryPreference[];
}

export interface NotificationCategoryPreference {
  category: string;
  channels: NotificationChannel[];
  enabled: boolean;
}

export interface NotificationTemplateDto {
  id: string;
  name: string;
  type: NotificationType;
  subject: string;
  bodyTemplate: string;
  channels: NotificationChannel[];
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface UnreadCountDto {
  total: number;
  byType: Record<NotificationType, number>;
  byPriority: Record<NotificationPriority, number>;
}

// ============================================================================
// Request Types
// ============================================================================

export interface NotificationFilters {
  type?: NotificationType;
  priority?: NotificationPriority;
  isRead?: boolean;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  pageSize?: number;
}

export interface SendNotificationRequest {
  userId?: string;
  userIds?: string[];
  roleIds?: string[];
  type: NotificationType;
  priority?: NotificationPriority;
  title: string;
  message: string;
  data?: Record<string, unknown>;
  actionUrl?: string;
  actionLabel?: string;
  channels?: NotificationChannel[];
  expiresAt?: string;
}

export interface UpdateNotificationPreferencesRequest {
  emailEnabled?: boolean;
  smsEnabled?: boolean;
  pushEnabled?: boolean;
  inAppEnabled?: boolean;
  digestEnabled?: boolean;
  digestFrequency?: 'Daily' | 'Weekly';
  quietHoursEnabled?: boolean;
  quietHoursStart?: string;
  quietHoursEnd?: string;
  preferences?: NotificationCategoryPreference[];
}

export interface CreateNotificationTemplateRequest {
  name: string;
  type: NotificationType;
  subject: string;
  bodyTemplate: string;
  channels: NotificationChannel[];
  isActive?: boolean;
}

export interface UpdateNotificationTemplateRequest {
  name?: string;
  subject?: string;
  bodyTemplate?: string;
  channels?: NotificationChannel[];
  isActive?: boolean;
}

// ============================================================================
// Response Types
// ============================================================================

export type NotificationResponse = ApiResponse<NotificationDto>;
export type NotificationListResponse = ApiResponse<PaginatedResponse<NotificationSummaryDto>>;
export type NotificationPreferencesResponse = ApiResponse<NotificationPreferencesDto>;
export type UnreadCountResponse = ApiResponse<UnreadCountDto>;
export type NotificationTemplateResponse = ApiResponse<NotificationTemplateDto>;
export type NotificationTemplateListResponse = ApiResponse<PaginatedResponse<NotificationTemplateDto>>;
