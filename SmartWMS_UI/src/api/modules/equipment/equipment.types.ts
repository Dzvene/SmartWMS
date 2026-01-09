/**
 * Equipment API Types
 * Based on backend DTOs from SmartWMS_BE/Modules/Equipment/DTOs/
 */

import type { ApiResponse, PaginatedResponse, PaginationParams } from '../../types';

// ============================================================================
// Enums
// ============================================================================

export type EquipmentType =
  | 'Forklift'
  | 'ReachTruck'
  | 'OrderPicker'
  | 'PalletJack'
  | 'HandScanner'
  | 'RFGun'
  | 'Printer'
  | 'Conveyor'
  | 'Sorter'
  | 'ASRS'
  | 'AGV'
  | 'Dock'
  | 'Scale'
  | 'Other';

export type EquipmentStatus =
  | 'Available'
  | 'InUse'
  | 'Maintenance'
  | 'OutOfService'
  | 'Reserved';

// ============================================================================
// Equipment Types
// ============================================================================

export interface EquipmentDto {
  id: string;
  code: string;
  name: string;
  description?: string;

  type: EquipmentType;
  typeName: string;

  status: EquipmentStatus;
  statusName: string;

  warehouseId?: string;
  warehouseName?: string;
  zoneId?: string;
  zoneName?: string;

  assignedToUserId?: string;
  assignedToUserName?: string;

  lastMaintenanceDate?: string;
  nextMaintenanceDate?: string;
  maintenanceNotes?: string;

  // Type-specific specifications
  specifications?: Record<string, unknown>;

  serialNumber?: string;
  manufacturer?: string;
  model?: string;
  purchaseDate?: string;
  warrantyExpiryDate?: string;

  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateEquipmentRequest {
  code: string;
  name: string;
  description?: string;

  type: EquipmentType;
  status?: EquipmentStatus;

  warehouseId?: string;
  zoneId?: string;

  assignedToUserId?: string;

  lastMaintenanceDate?: string;
  nextMaintenanceDate?: string;
  maintenanceNotes?: string;

  specifications?: string;

  serialNumber?: string;
  manufacturer?: string;
  model?: string;
  purchaseDate?: string;
  warrantyExpiryDate?: string;

  isActive?: boolean;
}

export interface UpdateEquipmentRequest {
  name?: string;
  description?: string;

  type?: EquipmentType;
  status?: EquipmentStatus;

  warehouseId?: string;
  zoneId?: string;

  assignedToUserId?: string;

  lastMaintenanceDate?: string;
  nextMaintenanceDate?: string;
  maintenanceNotes?: string;

  specifications?: string;

  serialNumber?: string;
  manufacturer?: string;
  model?: string;
  purchaseDate?: string;
  warrantyExpiryDate?: string;

  isActive?: boolean;
}

export interface AssignEquipmentRequest {
  userId?: string;
}

export interface UpdateEquipmentStatusRequest {
  status: EquipmentStatus;
  notes?: string;
}

export interface EquipmentFilters extends PaginationParams {
  search?: string;
  type?: EquipmentType;
  status?: EquipmentStatus;
  warehouseId?: string;
  zoneId?: string;
  assignedToUserId?: string;
  isAssigned?: boolean;
  isActive?: boolean;
}

// ============================================================================
// Response Types
// ============================================================================

export type EquipmentResponse = ApiResponse<EquipmentDto>;
export type EquipmentListResponse = ApiResponse<PaginatedResponse<EquipmentDto>>;
