/**
 * Integrations Module Types
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================================================
// Enums
// ============================================================================

export type IntegrationType = 'ERP' | 'Ecommerce' | 'Carrier' | 'Accounting' | 'CRM' | 'Custom';

export type IntegrationStatus = 'Active' | 'Inactive' | 'Error' | 'Pending' | 'Disconnected';

export type SyncDirection = 'Inbound' | 'Outbound' | 'Bidirectional';

export type SyncStatus = 'Pending' | 'InProgress' | 'Completed' | 'Failed' | 'PartiallyCompleted';

export type SyncEntityType =
  | 'Product'
  | 'Order'
  | 'Inventory'
  | 'Customer'
  | 'Supplier'
  | 'Location'
  | 'Receipt'
  | 'Shipment';

// ============================================================================
// DTOs
// ============================================================================

export interface IntegrationDto {
  id: string;
  name: string;
  integrationType: IntegrationType;
  status: IntegrationStatus;
  providerName: string;
  description?: string;
  baseUrl?: string;
  apiVersion?: string;
  authType?: string;
  isDefault: boolean;
  lastSyncAt?: string;
  lastSyncStatus?: SyncStatus;
  syncSchedule?: string;
  settings?: Record<string, unknown>;
  createdAt: string;
  updatedAt?: string;
}

export interface IntegrationSummaryDto {
  id: string;
  name: string;
  integrationType: IntegrationType;
  status: IntegrationStatus;
  providerName: string;
  lastSyncAt?: string;
  lastSyncStatus?: SyncStatus;
  isDefault: boolean;
}

export interface SyncJobDto {
  id: string;
  integrationId: string;
  integrationName?: string;
  entityType: SyncEntityType;
  direction: SyncDirection;
  status: SyncStatus;
  startedAt?: string;
  completedAt?: string;
  totalRecords: number;
  processedRecords: number;
  failedRecords: number;
  errorMessage?: string;
  createdAt: string;
}

export interface SyncLogDto {
  id: string;
  syncJobId: string;
  entityId?: string;
  entityType: SyncEntityType;
  action: string;
  status: 'Success' | 'Failed' | 'Skipped';
  message?: string;
  details?: Record<string, unknown>;
  createdAt: string;
}

export interface WebhookDto {
  id: string;
  integrationId: string;
  name: string;
  url: string;
  events: string[];
  isActive: boolean;
  secret?: string;
  lastTriggeredAt?: string;
  failureCount: number;
  createdAt: string;
  updatedAt?: string;
}

// ============================================================================
// Request Types
// ============================================================================

export interface IntegrationFilters {
  integrationType?: IntegrationType;
  status?: IntegrationStatus;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface SyncJobFilters {
  integrationId?: string;
  entityType?: SyncEntityType;
  direction?: SyncDirection;
  status?: SyncStatus;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateIntegrationRequest {
  name: string;
  integrationType: IntegrationType;
  providerName: string;
  description?: string;
  baseUrl?: string;
  apiVersion?: string;
  authType?: string;
  credentials?: Record<string, unknown>;
  settings?: Record<string, unknown>;
  syncSchedule?: string;
}

export interface UpdateIntegrationRequest {
  name?: string;
  description?: string;
  baseUrl?: string;
  apiVersion?: string;
  credentials?: Record<string, unknown>;
  settings?: Record<string, unknown>;
  syncSchedule?: string;
}

export interface TriggerSyncRequest {
  entityType: SyncEntityType;
  direction: SyncDirection;
  entityIds?: string[];
  fullSync?: boolean;
}

export interface CreateWebhookRequest {
  integrationId: string;
  name: string;
  url: string;
  events: string[];
  secret?: string;
}

export interface UpdateWebhookRequest {
  name?: string;
  url?: string;
  events?: string[];
  isActive?: boolean;
  secret?: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type IntegrationResponse = ApiResponse<IntegrationDto>;
export type IntegrationListResponse = ApiResponse<PaginatedResponse<IntegrationSummaryDto>>;

export type SyncJobResponse = ApiResponse<SyncJobDto>;
export type SyncJobListResponse = ApiResponse<PaginatedResponse<SyncJobDto>>;

export type SyncLogListResponse = ApiResponse<PaginatedResponse<SyncLogDto>>;

export type WebhookResponse = ApiResponse<WebhookDto>;
export type WebhookListResponse = ApiResponse<WebhookDto[]>;
