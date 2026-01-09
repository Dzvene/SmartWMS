/**
 * Audit Module Types
 * TypeScript interfaces for Audit API endpoints
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================================================
// Enums
// ============================================================================

export type AuditSeverity = 'Debug' | 'Info' | 'Warning' | 'Error' | 'Critical';

export type SystemEventCategory =
  | 'Application'
  | 'Security'
  | 'Performance'
  | 'Integration'
  | 'Scheduler'
  | 'Database'
  | 'FileSystem'
  | 'Network'
  | 'Other';

export type SystemEventSeverity =
  | 'Trace'
  | 'Debug'
  | 'Information'
  | 'Warning'
  | 'Error'
  | 'Critical'
  | 'Fatal';

// ============================================================================
// DTOs
// ============================================================================

export interface AuditLogDto {
  id: string;
  eventType: string;
  entityType: string;
  entityId?: string;
  entityNumber?: string;
  action: string;
  severity: AuditSeverity;
  userId?: string;
  userName?: string;
  userEmail?: string;
  eventTime: string;
  ipAddress?: string;
  oldValues?: string;
  newValues?: string;
  changedFields?: string;
  module?: string;
  subModule?: string;
  correlationId?: string;
  notes?: string;
  isSuccess: boolean;
  errorMessage?: string;
}

export interface AuditLogSummaryDto {
  id: string;
  eventType: string;
  entityType: string;
  entityNumber?: string;
  action: string;
  severity: AuditSeverity;
  userName?: string;
  eventTime: string;
  module?: string;
  isSuccess: boolean;
}

export interface ActivityLogDto {
  id: string;
  activityType: string;
  description: string;
  userId: string;
  userName?: string;
  activityTime: string;
  module?: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  relatedEntityNumber?: string;
  deviceType?: string;
  notes?: string;
}

export interface SystemEventLogDto {
  id: string;
  eventType: string;
  category: SystemEventCategory;
  severity: SystemEventSeverity;
  message: string;
  details?: string;
  eventTime: string;
  source?: string;
  exceptionType?: string;
  exceptionMessage?: string;
  correlationId?: string;
}

export interface AuditStatisticsDto {
  totalEvents: number;
  todayEvents: number;
  errorCount: number;
  warningCount: number;
  eventsByModule: Record<string, number>;
  eventsByType: Record<string, number>;
  eventsByUser: Record<string, number>;
  lastEventTime?: string;
}

export interface UserActivitySummaryDto {
  userId: string;
  userName?: string;
  totalActivities: number;
  lastActivityTime?: string;
  activitiesByType: Record<string, number>;
  activitiesByModule: Record<string, number>;
}

// ============================================================================
// Filter Types
// ============================================================================

export interface AuditLogFilters {
  eventType?: string;
  entityType?: string;
  entityId?: string;
  userId?: string;
  severity?: AuditSeverity;
  module?: string;
  dateFrom?: string;
  dateTo?: string;
  isSuccess?: boolean;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface ActivityLogFilters {
  activityType?: string;
  userId?: string;
  module?: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  deviceType?: string;
  dateFrom?: string;
  dateTo?: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface SystemEventFilters {
  eventType?: string;
  category?: SystemEventCategory;
  minSeverity?: SystemEventSeverity;
  source?: string;
  dateFrom?: string;
  dateTo?: string;
  hasException?: boolean;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

// ============================================================================
// Response Types
// ============================================================================

export type AuditLogResponse = ApiResponse<AuditLogDto>;
export type AuditLogListResponse = ApiResponse<PaginatedResponse<AuditLogSummaryDto>>;
export type ActivityLogListResponse = ApiResponse<PaginatedResponse<ActivityLogDto>>;
export type SystemEventListResponse = ApiResponse<PaginatedResponse<SystemEventLogDto>>;
export type SystemEventResponse = ApiResponse<SystemEventLogDto>;
export type AuditStatisticsResponse = ApiResponse<AuditStatisticsDto>;
export type UserActivitySummaryResponse = ApiResponse<UserActivitySummaryDto>;
