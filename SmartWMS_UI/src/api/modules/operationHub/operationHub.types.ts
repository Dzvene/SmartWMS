/**
 * Operation Hub API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/OperationHub/DTOs/
 */

import type { ApiResponse, PaginatedResponse } from '../../types';

// ============================================================================
// Session Types
// ============================================================================

export type OperatorSessionStatus = 'Active' | 'OnBreak' | 'Idle' | 'Ended';

export interface OperatorSessionDto {
  id: string;
  userId: string;
  userName?: string;
  warehouseId: string;
  warehouseName?: string;

  // Device info
  deviceId?: string;
  deviceType?: string;
  deviceName?: string;

  // Session
  status: OperatorSessionStatus;
  startedAt: string;
  endedAt?: string;
  lastActivityAt?: string;

  // Current work
  currentTaskType?: string;
  currentTaskId?: string;
  currentZone?: string;
  currentLocation?: string;

  // Shift
  shiftCode?: string;
  shiftStart?: string;
  shiftEnd?: string;

  // Calculated
  sessionDurationMinutes: number;
  idleMinutes: number;
}

export interface StartSessionRequest {
  warehouseId: string;
  deviceId?: string;
  deviceType?: string;
  deviceName?: string;
  shiftCode?: string;
}

export interface UpdateSessionStatusRequest {
  status: OperatorSessionStatus;
  currentZone?: string;
  currentLocation?: string;
}

// ============================================================================
// Unified Task Types
// ============================================================================

export interface UnifiedTaskDto {
  id: string;
  taskType: string;
  taskNumber: string;
  status: string;
  priority: number;

  // Location info
  sourceLocation?: string;
  destinationLocation?: string;
  zone?: string;
  aisle?: string;

  // Product info
  sku?: string;
  productName?: string;
  quantity?: number;
  uom?: string;

  // Container info
  containerId?: string;
  lpnNumber?: string;

  // Assignment
  assignedToUserId?: string;
  assignedToUserName?: string;
  assignedAt?: string;

  // Timing
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
  dueBy?: string;

  // Parent reference
  orderNumber?: string;
  batchNumber?: string;
}

export interface TaskQueueQueryParams {
  warehouseId?: string;
  taskType?: string;
  status?: string;
  zone?: string;
  assignedToUserId?: string;
  unassignedOnly?: boolean;
  sortBy?: string;
  page?: number;
  pageSize?: number;
}

export interface AssignTaskRequest {
  taskId: string;
  taskType: string;
  userId: string;
}

export interface StartTaskRequest {
  taskId: string;
  taskType: string;
}

export interface CompleteTaskRequest {
  taskId: string;
  taskType: string;
  actualQuantity?: number;
  notes?: string;
  reasonCode?: string;
}

export interface PauseTaskRequest {
  taskId: string;
  taskType: string;
  reason?: string;
}

// ============================================================================
// Scan Types
// ============================================================================

export type ScanType = 'Barcode' | 'QRCode' | 'RFID';
export type ScanContext = 'Picking' | 'Putaway' | 'Packing' | 'CycleCount' | 'Receiving' | 'Shipping' | 'Other';

export interface ScanRequest {
  barcode: string;
  scanType?: ScanType;
  context: ScanContext;
  taskType?: string;
  taskId?: string;
  expectedSku?: string;
  expectedLocation?: string;
  expectedQuantity?: number;
  deviceId?: string;
}

export interface ScanResponse {
  success: boolean;
  errorCode?: string;
  errorMessage?: string;

  entityType: string;
  entityId?: string;

  resolvedSku?: string;
  resolvedProductName?: string;
  resolvedLocation?: string;
  resolvedLpn?: string;

  availableQuantity?: number;
  uom?: string;

  matchesExpected: boolean;
  validationMessage?: string;
  nextAction?: string;
}

export interface ScanLogDto {
  id: string;
  userId: string;
  userName?: string;
  sessionId?: string;

  barcode: string;
  scanType: ScanType;
  context: ScanContext;

  entityType?: string;
  resolvedSku?: string;
  resolvedLocation?: string;

  taskType?: string;
  taskId?: string;

  success: boolean;
  errorCode?: string;
  errorMessage?: string;

  deviceId?: string;
  scannedAt: string;
}

export interface ScanLogQueryParams {
  warehouseId?: string;
  userId?: string;
  sessionId?: string;
  context?: ScanContext;
  successOnly?: boolean;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

// ============================================================================
// Productivity Types
// ============================================================================

export interface OperatorProductivityDto {
  id: string;
  userId: string;
  userName?: string;
  warehouseId: string;
  date: string;

  pickTasksCompleted: number;
  packTasksCompleted: number;
  putawayTasksCompleted: number;
  cycleCountsCompleted: number;
  totalTasksCompleted: number;

  totalUnitsPicked: number;
  totalUnitsPacked: number;
  totalUnitsPutaway: number;
  totalLocationsVisited: number;

  totalWorkMinutes: number;
  totalIdleMinutes: number;
  totalBreakMinutes: number;
  productiveTimePercent: number;

  totalScans: number;
  correctScans: number;
  errorScans: number;
  accuracyRate: number;

  picksPerHour: number;
  unitsPerHour: number;
  tasksPerHour: number;
}

export interface ProductivityQueryParams {
  warehouseId?: string;
  userId?: string;
  fromDate?: string;
  toDate?: string;
  groupBy?: string;
}

export interface ProductivitySummaryDto {
  fromDate: string;
  toDate: string;
  totalOperators: number;
  totalWorkDays: number;

  totalTasksCompleted: number;
  totalUnitsProcessed: number;
  totalWorkMinutes: number;

  avgTasksPerOperatorPerDay: number;
  avgUnitsPerOperatorPerDay: number;
  avgAccuracyRate: number;
  avgPicksPerHour: number;

  topPerformers: TopPerformerDto[];
}

export interface TopPerformerDto {
  userId: string;
  userName: string;
  tasksCompleted: number;
  unitsProcessed: number;
  accuracyRate: number;
  avgTasksPerHour: number;
}

// ============================================================================
// Operator Stats Types
// ============================================================================

export interface TodayStatsDto {
  tasksCompleted: number;
  unitsProcessed: number;
  workMinutes: number;
  idleMinutes: number;
  accuracyRate: number;
  tasksPerHour: number;
}

export interface OperatorStatsDto {
  userId: string;
  userName: string;
  warehouseId: string;

  currentSession?: OperatorSessionDto;
  isOnline: boolean;
  minutesSinceLastActivity: number;

  today: TodayStatsDto;
  currentTask?: UnifiedTaskDto;
}

export interface WarehouseOperatorsOverviewDto {
  warehouseId: string;
  warehouseName: string;

  totalOperators: number;
  onlineOperators: number;
  onBreakOperators: number;
  idleOperators: number;

  totalTasksCompletedToday: number;
  totalUnitsProcessedToday: number;
  avgAccuracyToday: number;

  activeSessions: OperatorSessionDto[];
}

// ============================================================================
// Task Action Types
// ============================================================================

export type TaskAction = 'Assigned' | 'Started' | 'Paused' | 'Resumed' | 'Completed' | 'Cancelled' | 'Scanned' | 'Error';

export interface TaskActionLogDto {
  id: string;
  userId: string;
  userName?: string;

  taskType: string;
  taskId: string;
  taskNumber: string;

  action: TaskAction;
  actionAt: string;

  fromStatus?: string;
  toStatus?: string;
  locationCode?: string;
  productSku?: string;
  quantity?: number;

  durationSeconds?: number;
  notes?: string;
  reasonCode?: string;
}

export interface TaskActionLogQueryParams {
  warehouseId?: string;
  userId?: string;
  taskType?: string;
  taskId?: string;
  action?: TaskAction;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

// ============================================================================
// Response Types
// ============================================================================

export type OperatorSessionResponse = ApiResponse<OperatorSessionDto>;
export type ActiveSessionsResponse = ApiResponse<OperatorSessionDto[]>;
export type TaskQueueResponse = ApiResponse<PaginatedResponse<UnifiedTaskDto>>;
export type UnifiedTaskResponse = ApiResponse<UnifiedTaskDto>;
export type ScanResultResponse = ApiResponse<ScanResponse>;
export type ScanLogsResponse = ApiResponse<PaginatedResponse<ScanLogDto>>;
export type OperatorStatsResponse = ApiResponse<OperatorStatsDto>;
export type OperatorProductivityResponse = ApiResponse<OperatorProductivityDto>;
export type ProductivityHistoryResponse = ApiResponse<PaginatedResponse<OperatorProductivityDto>>;
export type ProductivitySummaryResponse = ApiResponse<ProductivitySummaryDto>;
export type WarehouseOverviewResponse = ApiResponse<WarehouseOperatorsOverviewDto>;
export type TaskActionLogsResponse = ApiResponse<PaginatedResponse<TaskActionLogDto>>;
