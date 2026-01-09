/**
 * Sessions Module Types
 * TypeScript interfaces for Sessions API endpoints
 */

import type { ApiResponse, PaginatedResponse } from '@/api/types';

// ============================================================================
// Enums
// ============================================================================

export type SessionStatus = 'Active' | 'Expired' | 'Revoked' | 'Locked';

// ============================================================================
// DTOs
// ============================================================================

export interface UserSessionDto {
  id: string;
  userId: string;
  deviceId?: string;
  deviceName?: string;
  deviceType?: string;
  browser?: string;
  operatingSystem?: string;
  ipAddress?: string;
  location?: string;
  status: SessionStatus;
  lastActivityAt?: string;
  expiresAt: string;
  isTrustedDevice: boolean;
  isCurrent: boolean;
  createdAt: string;
}

export interface SessionSummaryDto {
  id: string;
  deviceName?: string;
  deviceType?: string;
  ipAddress?: string;
  location?: string;
  status: SessionStatus;
  lastActivityAt?: string;
  isCurrent: boolean;
}

export interface ActiveSessionsDto {
  totalActive: number;
  totalDevices: number;
  currentSession?: SessionSummaryDto;
  otherSessions: SessionSummaryDto[];
}

export interface LoginAttemptDto {
  id: string;
  userId?: string;
  userName?: string;
  success: boolean;
  failureReason?: string;
  ipAddress?: string;
  location?: string;
  createdAt: string;
}

export interface LoginStatsDto {
  totalAttempts: number;
  successfulAttempts: number;
  failedAttempts: number;
  successRate: number;
  recentFailures: LoginAttemptDto[];
}

export interface TrustedDeviceDto {
  id: string;
  userId: string;
  deviceId: string;
  deviceName: string;
  deviceType?: string;
  lastUsedAt?: string;
  lastIpAddress?: string;
  isActive: boolean;
  createdAt: string;
}

// ============================================================================
// Request Types
// ============================================================================

export interface SessionQueryParams {
  page?: number;
  pageSize?: number;
  status?: SessionStatus;
  deviceType?: string;
  fromDate?: string;
  toDate?: string;
}

export interface LoginAttemptQueryParams {
  page?: number;
  pageSize?: number;
  success?: boolean;
  fromDate?: string;
  toDate?: string;
}

export interface CreateSessionRequest {
  deviceId?: string;
  deviceName?: string;
  deviceType?: string;
  browser?: string;
  operatingSystem?: string;
  appVersion?: string;
  ipAddress?: string;
}

export interface RevokeSessionRequest {
  reason?: string;
}

export interface RegisterDeviceRequest {
  deviceId: string;
  deviceName: string;
  deviceType?: string;
}

// ============================================================================
// Response Types
// ============================================================================

export type UserSessionResponse = ApiResponse<UserSessionDto>;
export type UserSessionListResponse = ApiResponse<PaginatedResponse<UserSessionDto>>;
export type UserActiveSessionsResponse = ApiResponse<ActiveSessionsDto>;
export type LoginAttemptListResponse = ApiResponse<PaginatedResponse<LoginAttemptDto>>;
export type LoginStatsResponse = ApiResponse<LoginStatsDto>;
export type TrustedDeviceListResponse = ApiResponse<TrustedDeviceDto[]>;
