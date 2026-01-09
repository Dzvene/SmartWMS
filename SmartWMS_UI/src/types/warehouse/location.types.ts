import { Entity, Dimensions } from '@/models';

/**
 * Storage location (bin) within warehouse
 */
export interface Location extends Entity {
  code: string;
  name?: string;
  warehouseId: string;
  warehouseCode: string;
  zoneId?: string;
  zoneCode?: string;

  aisle?: string;
  rack?: string;
  level?: string;
  position?: string;

  locationType: LocationType;
  dimensions?: Dimensions;
  maxWeight?: number;
  maxVolume?: number;

  isActive: boolean;
  isPickLocation: boolean;
  isPutawayLocation: boolean;
  isReceivingDock: boolean;
  isShippingDock: boolean;

  pickSequence?: number;
  putawaySequence?: number;

  currentOccupancy?: number;
  skuCount?: number;
}

/**
 * Location type classification
 */
export enum LocationType {
  Bulk = 'BULK',
  Pick = 'PICK',
  Staging = 'STAGING',
  Receiving = 'RECEIVING',
  Shipping = 'SHIPPING',
  Returns = 'RETURNS',
  Quarantine = 'QUARANTINE',
  Reserve = 'RESERVE',
}

/**
 * Warehouse zone grouping locations
 */
export interface Zone extends Entity {
  code: string;
  name: string;
  warehouseId: string;
  warehouseCode: string;

  zoneType: ZoneType;
  description?: string;

  locationCount: number;
  isActive: boolean;
}

/**
 * Zone type for organization
 */
export enum ZoneType {
  Storage = 'STORAGE',
  Picking = 'PICKING',
  Packing = 'PACKING',
  Staging = 'STAGING',
  Shipping = 'SHIPPING',
  Receiving = 'RECEIVING',
  Returns = 'RETURNS',
}

/**
 * Warehouse/facility definition
 */
export interface Warehouse extends Entity {
  code: string;
  name: string;
  siteId?: string;
  siteCode?: string;

  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  postalCode?: string;
  countryCode?: string;

  timezone: string;
  isActive: boolean;
  isPrimary: boolean;

  zoneCount: number;
  locationCount: number;
}

/**
 * Location filter parameters
 */
export interface LocationFilter {
  warehouseId?: string;
  zoneId?: string;
  locationType?: LocationType;
  isActive?: boolean;
  hasStock?: boolean;
  isEmpty?: boolean;
}
